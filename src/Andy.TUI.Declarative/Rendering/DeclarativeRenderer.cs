using System;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Core.Diagnostics;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;

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
    
    public DeclarativeRenderer(IRenderingSystem renderingSystem, object? owner = null)
    {
        _renderingSystem = renderingSystem ?? throw new ArgumentNullException(nameof(renderingSystem));
        _virtualDomRenderer = new VirtualDomRenderer(renderingSystem);
        _context = new DeclarativeContext(() => _needsRender = true);
        _diffEngine = new DiffEngine();
        _logger = DebugContext.Logger.ForCategory("DeclarativeRenderer");
        
        _logger.Info("DeclarativeRenderer initialized");
    }
    
    /// <summary>
    /// Runs the declarative UI with event handling.
    /// </summary>
    public void Run(Func<ISimpleComponent> createRoot)
    {
        var inputHandler = new ConsoleInputHandler();
        inputHandler.KeyPressed += OnKeyPressed;
        inputHandler.Start();
        
        // Create the root component once
        var root = createRoot();
        
        try
        {
            while (true)
            {
                if (_needsRender)
                {
                    _logger.Debug("Render requested, executing render cycle");
                    Render(root);
                    _needsRender = false;
                }
                
                System.Threading.Thread.Sleep(16); // ~60 FPS
            }
        }
        finally
        {
            inputHandler.Stop();
        }
    }
    
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
        var constraints = LayoutConstraints.Loose(terminalWidth, terminalHeight);
        _logger.Debug("Layout constraints: {0}x{1}", terminalWidth, terminalHeight);
        
        rootInstance.CalculateLayout(constraints);
        _logger.Debug("Layout calculated");
        
        // Set the root's absolute position BEFORE rendering
        rootInstance.Layout.AbsoluteX = 0;
        rootInstance.Layout.AbsoluteY = 0;
        _logger.Debug("Set root absolute position to (0, 0)");
        
        // Render the virtual DOM from instances
        var newTree = rootInstance.Render();
        _logger.Debug("Virtual DOM rendered");
        
        // For now, always do a full render until patch application is fixed
        _logger.Debug("Performing full render");
        _virtualDomRenderer.Render(newTree);
        
        // Force the rendering system to flush changes to screen
        if (_renderingSystem is RenderingSystem rs)
        {
            _logger.Debug("Buffer dirty state before flush: {0}", rs.Buffer.IsDirty);
            rs.Render();
            _logger.Debug("Forced render flush");
            
            // Give the render thread time to process
            System.Threading.Thread.Sleep(10);
            _logger.Debug("Buffer dirty state after flush: {0}", rs.Buffer.IsDirty);
        }
        
        _previousTree = newTree;
        _logger.Debug("Render complete");
    }
    
    private void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        _logger.Debug("Key pressed: {0} (Modifiers: {1})", e.Key, e.Modifiers);
        
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
    }
    
}