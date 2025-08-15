using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using System.ComponentModel;
using System.Reflection;

namespace Andy.TUI.Declarative.Rendering;

/// <summary>
/// Renders declarative UI components with event handling and state management.
/// </summary>
public class DeclarativeRenderer
{
    private readonly IRenderingSystem _renderingSystem;
    private readonly VirtualDomRenderer _virtualDomRenderer;
    private readonly DeclarativeContext _context;
    private readonly DiffEngine _diffEngine;
    private readonly ILogger _logger;
    private VirtualNode? _previousTree;
    private bool _needsRender = true;
    private bool _hasSetInitialFocus = false;
    private bool _autoFocus = true;  // Make auto-focus configurable

    public DeclarativeRenderer(IRenderingSystem renderingSystem, object? owner = null, bool autoFocus = true)
    {
        _renderingSystem = renderingSystem ?? throw new ArgumentNullException(nameof(renderingSystem));
        _virtualDomRenderer = new VirtualDomRenderer(renderingSystem);
        _context = new DeclarativeContext(() => _needsRender = true);
        _diffEngine = new DiffEngine();
        _logger = DebugContext.Logger.ForCategory("DeclarativeRenderer");
        _autoFocus = autoFocus;

        _logger.Info($"DeclarativeRenderer initialized (autoFocus={autoFocus})");

        // Auto-subscribe to owner (and its immediate state fields) change notifications to trigger re-renders
        if (owner != null)
        {
            TrySubscribeOwnerChangeNotifications(owner);
        }

        // Re-render on terminal resize when possible
        if (_renderingSystem is RenderingSystem rs)
        {
            rs.Terminal.SizeChanged += (_, __) => { _needsRender = true; };
        }
    }

    private void TrySubscribeOwnerChangeNotifications(object owner)
    {
        void SubscribeObject(object target)
        {
            if (target == null) return;

            // Subscribe INotifyPropertyChanged
            if (target is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += (_, __) => { _needsRender = true; };
            }

            // Subscribe to an Action OnPropertyChanged event if present
            var evt = target.GetType().GetEvent("OnPropertyChanged", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (evt != null && evt.EventHandlerType == typeof(Action))
            {
                var handler = (Action)(() => { _needsRender = true; });
                evt.AddEventHandler(target, handler);
            }
        }

        // Subscribe owner itself
        SubscribeObject(owner);

        // Subscribe immediate fields of owner (e.g., private state container)
        var fields = owner.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            try
            {
                var value = field.GetValue(owner);
                SubscribeObject(value!);
            }
            catch
            {
                // Ignore reflection failures
            }
        }
    }

    /// <summary>
    /// Exposes the underlying declarative context for advanced scenarios.
    /// </summary>
    public DeclarativeContext Context => _context;

    public DeclarativeRenderer(IRenderingSystem renderingSystem, IInputHandler inputHandler, object? owner = null, bool autoFocus = true)
        : this(renderingSystem, owner, autoFocus)
    {
        _externalInputHandler = inputHandler;
    }

    /// <summary>
    /// Public API to request a re-render from external events (timers, async work, etc.).
    /// </summary>
    public void RequestRender()
    {
        _needsRender = true;
    }

    /// <summary>
    /// Runs the declarative UI with event handling.
    /// </summary>
    public void Run(Func<ISimpleComponent> createRoot)
    {
        // Use injected handler if provided; otherwise default to ConsoleInputHandler
        var inputHandler = _externalInputHandler ?? new ConsoleInputHandler();
        inputHandler.KeyPressed += OnKeyPressed;
        inputHandler.Start();

        try
        {
            while (true)
            {
                // Poll for input events
                inputHandler.Poll();

                if (_needsRender)
                {
                    _logger.Debug("Render requested, executing render cycle");
                    // Debug logging (uncomment to debug render cycles)
                    // Console.Error.WriteLine("[DeclarativeRenderer] Executing render cycle");
                    // Recreate the root component tree on each render to capture fresh state
                    var root = createRoot();
                    Render(root);
                    _needsRender = false;
                    // Console.Error.WriteLine("[DeclarativeRenderer] Render cycle complete");
                }

                // Light sleep to avoid busy loop; frame pacing is handled by scheduler
                System.Threading.Thread.Sleep(2);
            }
        }
        finally
        {
            inputHandler.Stop();
        }
    }

    private readonly IInputHandler? _externalInputHandler;

    /// <summary>
    /// Renders a single frame.
    /// </summary>
    public void Render(ISimpleComponent root)
    {
        _logger.Debug("Render() called");

        // Get or create the root view instance
        var rootInstance = _context.ViewInstanceManager.GetOrCreateInstance(root, "root");
        _logger.Debug("Root instance: {0}", rootInstance.GetType().Name);

        // Calculate layout with terminal constraints
        var terminalWidth = _renderingSystem.Width;
        var terminalHeight = _renderingSystem.Height;
        // Prevent negative or degenerate constraints (can happen during terminal resize)
        var safeWidth = Math.Max(0, terminalWidth);
        var safeHeight = Math.Max(0, terminalHeight);
        var constraints = LayoutConstraints.Loose(safeWidth, safeHeight);
        _logger.Debug("Layout constraints: {0}x{1}", terminalWidth, terminalHeight);

        rootInstance.CalculateLayout(constraints);
        // Clamp invalid results (NaN, Infinity, negatives) defensively
        if (float.IsNaN(rootInstance.Layout.Width) || float.IsNegativeInfinity(rootInstance.Layout.Width) || rootInstance.Layout.Width < 0)
        {
            _logger.Warning("Root layout produced invalid width (W={0}). Clamping to 0.", rootInstance.Layout.Width);
            rootInstance.Layout.Width = 0;
        }
        else if (float.IsPositiveInfinity(rootInstance.Layout.Width))
        {
            rootInstance.Layout.Width = safeWidth;
        }

        if (float.IsNaN(rootInstance.Layout.Height) || float.IsNegativeInfinity(rootInstance.Layout.Height) || rootInstance.Layout.Height < 0)
        {
            _logger.Warning("Root layout produced invalid height (H={0}). Clamping to 0.", rootInstance.Layout.Height);
            rootInstance.Layout.Height = 0;
        }
        else if (float.IsPositiveInfinity(rootInstance.Layout.Height))
        {
            rootInstance.Layout.Height = safeHeight;
        }
        // Layout invariants: sizes non-negative after clamping
        Guard(rootInstance.Layout.Width >= 0 && rootInstance.Layout.Height >= 0, "Root layout produced negative size");
        // Layout invariants: content size non-negative; absolute position set next
        Guard(rootInstance.Layout.ContentWidth >= 0 && rootInstance.Layout.ContentHeight >= 0, "Root content size negative");
        _logger.Debug("Layout calculated");

        // Set the root's absolute position BEFORE rendering
        rootInstance.Layout.AbsoluteX = 0;
        rootInstance.Layout.AbsoluteY = 0;
        _logger.Debug("Set root absolute position to (0, 0)");

        // Update absolute z-indices from root
        rootInstance.UpdateAbsoluteZIndex(0);
        _logger.Debug("Updated absolute z-indices");

        // Register focusable components in document order after tree is built
        _context.ViewInstanceManager.RegisterFocusableComponents(rootInstance);
        _logger.Debug("Registered focusable components in document order");

        // Set initial focus if needed and auto-focus is enabled
        if (!_hasSetInitialFocus && _autoFocus && _context.FocusManager.FocusableCount > 0 && _context.FocusManager.FocusedComponent == null)
        {
            _logger.Debug("Setting initial focus during first render");
            _context.FocusManager.MoveFocus(Focus.FocusDirection.Next);
            _hasSetInitialFocus = true;
            _logger.Debug("Initial focus set to: {0}",
                _context.FocusManager.FocusedComponent?.GetType().Name ?? "null");
        }

        // Focus invariants: exactly one focused component when focusables exist
        var focusables = _context.FocusManager.FocusableCount;
        var hasFocus = _context.FocusManager.FocusedComponent != null;
        if (focusables > 0)
        {
            Guard(hasFocus, "No focused component while focusables exist");
        }

        // First, fill the entire terminal with a background color to prevent gaps
        // This happens before any virtual DOM rendering
        _renderingSystem.FillRect(0, 0, terminalWidth, terminalHeight, ' ', 
            Style.Default.WithBackgroundColor(Color.Black));
        
        // Render the virtual DOM from instances
        var newTree = rootInstance.Render();
        
        // VDOM invariants: ready to render
        VirtualDomInvariants.AssertTreeIsRenderable(newTree);
        _logger.Debug("Virtual DOM rendered - tree depth: {0}, node count: {1}",
            CalculateTreeDepth(newTree), CountNodes(newTree));

        // Apply diff-based rendering
        if (_previousTree == null)
        {
            _logger.Debug("Performing initial full render");
            _virtualDomRenderer.Render(newTree);
        }
        else
        {
            _logger.Debug("Performing diff-based render");
            var patches = _diffEngine.Diff(_previousTree, newTree);
            _logger.Debug("Generated {0} patches", patches.Count);

            // Log patch summary
            if (patches.Count > 0)
            {
                var patchTypes = patches.GroupBy(p => p.Type)
                    .Select(g => $"{g.Key}={g.Count()}")
                    .ToArray();
                _logger.Debug("Patch types: {0}", string.Join(", ", patchTypes));
            }

            // If structure changes occurred, fall back to full render to keep renderer's path map coherent
            if (patches.Any(p => p.Type == PatchType.Insert || p.Type == PatchType.Remove || p.Type == PatchType.Replace || p.Type == PatchType.Reorder))
            {
                _logger.Debug("Structural patches detected, using full render");
                _virtualDomRenderer.Render(newTree);
            }
            else
            {
                _virtualDomRenderer.ApplyPatches(patches);
                // Diff invariants: applying only Update* patches should not change structure; verify by re-diffing structure
                var structurePatches = patches.Where(p => p.Type != PatchType.UpdateProps && p.Type != PatchType.UpdateText).ToList();
                Guard(structurePatches.Count == 0, $"Unexpected structural patches during incremental update: {string.Join(",", structurePatches.Select(p => p.Type))}");
            }
        }

        // Force the rendering system to flush changes to screen
        if (_renderingSystem is RenderingSystem rs)
        {
            // Process any queued render operations first to ensure buffer is updated
            rs.Scheduler.ProcessQueuedOperations();

            _logger.Debug("Buffer dirty state before flush: {0}", rs.Buffer.IsDirty);
            // Debug logging (uncomment to debug buffer state)
            // Console.Error.WriteLine($"[DeclarativeRenderer] Buffer dirty before flush: {rs.Buffer.IsDirty}");

            // Now force a render to flush the buffer to screen, synchronously to reduce flicker
            rs.Scheduler.ForceRenderSync();
            _logger.Debug("Forced render flush");
            // Console.Error.WriteLine("[DeclarativeRenderer] Called rs.Render() to flush");

            // Give the render thread time to process
            System.Threading.Thread.Sleep(1);
            _logger.Debug("Buffer dirty state after flush: {0}", rs.Buffer.IsDirty);
            // Console.Error.WriteLine($"[DeclarativeRenderer] Buffer dirty after flush: {rs.Buffer.IsDirty}");
        }
        else
        {
            _logger.Warning("RenderingSystem is not the expected type!");
        }

        _previousTree = newTree;
        _logger.Debug("Render complete");
    }

        private static void Guard(bool condition, string message)
        {
            if (!condition) throw new LayoutInvariantViolationException(message);
        }

        private void OnKeyPressed(object? sender, KeyEventArgs e)
        {
        _logger.Debug("Key pressed: {0} (Modifiers: {1})", e.Key, e.Modifiers);
        // Debug logging (uncomment to debug key handling)
        // Console.Error.WriteLine($"[DeclarativeRenderer] Key pressed: {e.Key} Char: '{e.KeyChar}'");

        if (e.Key == ConsoleKey.C && e.Modifiers.HasFlag(System.ConsoleModifiers.Control))
        {
            _logger.Info("Ctrl+C detected, exiting");
            Environment.Exit(0);
        }

        var keyInfo = new ConsoleKeyInfo(
            e.KeyChar,
            e.Key,
            e.Modifiers.HasFlag(System.ConsoleModifiers.Shift),
            e.Modifiers.HasFlag(System.ConsoleModifiers.Alt),
            e.Modifiers.HasFlag(System.ConsoleModifiers.Control)
        );

        _context.EventRouter.RouteKeyPress(keyInfo);
        // Console.Error.WriteLine($"[DeclarativeRenderer] After routing key, needsRender: {_needsRender}");

        // Do not force an extra render here; components request renders as needed
        }

    private int CalculateTreeDepth(VirtualNode node, int currentDepth = 0)
    {
        if (node.Children == null || node.Children.Count == 0)
            return currentDepth;

        int maxDepth = currentDepth;
        foreach (var child in node.Children)
        {
            maxDepth = Math.Max(maxDepth, CalculateTreeDepth(child, currentDepth + 1));
        }
        return maxDepth;
    }

    private int CountNodes(VirtualNode node)
    {
        if (node.Children == null || node.Children.Count == 0)
            return 1;

        return 1 + node.Children.Sum(CountNodes);
    }

}

public sealed class LayoutInvariantViolationException : Exception
{
    public LayoutInvariantViolationException(string message) : base(message) { }
}