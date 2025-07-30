using System;
using Andy.TUI.Core.VirtualDom;
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
    private bool _needsRender = true;
    
    public DeclarativeRenderer(IRenderingSystem renderingSystem, object? owner = null)
    {
        _renderingSystem = renderingSystem ?? throw new ArgumentNullException(nameof(renderingSystem));
        _virtualDomRenderer = new VirtualDomRenderer(renderingSystem);
        _context = new DeclarativeContext(() => _needsRender = true);
    }
    
    /// <summary>
    /// Runs the declarative UI with event handling.
    /// </summary>
    public void Run(Func<ISimpleComponent> createRoot)
    {
        var inputHandler = new ConsoleInputHandler();
        inputHandler.KeyPressed += OnKeyPressed;
        inputHandler.Start();
        
        try
        {
            while (true)
            {
                if (_needsRender)
                {
                    Render(createRoot());
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
        // Get or create the root view instance
        var rootInstance = _context.ViewInstanceManager.GetOrCreateInstance(root, "root");
        
        // Calculate layout with terminal constraints
        var terminalWidth = _renderingSystem.Width;
        var terminalHeight = _renderingSystem.Height;
        var constraints = LayoutConstraints.Loose(terminalWidth, terminalHeight);
        rootInstance.CalculateLayout(constraints);
        
        // Walk the instance tree and register focusables
        RegisterViewInstances(rootInstance);
        
        // Render the virtual DOM from instances
        var virtualDom = rootInstance.Render();
        _virtualDomRenderer.Render(virtualDom);
    }
    
    private void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key == ConsoleKey.C && e.Modifiers.HasFlag(System.ConsoleModifiers.Control))
        {
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
    
    private void RegisterViewInstances(ViewInstance instance)
    {
        // Register focusable instances
        if (instance is IFocusable focusable)
        {
            _context.FocusManager.RegisterFocusable(focusable);
        }
        
        // Walk children for containers
        if (instance is VStackInstance vstack)
        {
            foreach (var child in vstack.GetChildInstances())
            {
                RegisterViewInstances(child);
            }
        }
        else if (instance is HStackInstance hstack)
        {
            foreach (var child in hstack.GetChildInstances())
            {
                RegisterViewInstances(child);
            }
        }
        else if (instance is BoxInstance box)
        {
            foreach (var child in box.GetChildInstances())
            {
                RegisterViewInstances(child);
            }
        }
    }
}

/// <summary>
/// Simple console input handler for keyboard events.
/// </summary>
public class ConsoleInputHandler
{
    private bool _running;
    private System.Threading.Thread? _thread;
    
    public event EventHandler<KeyEventArgs>? KeyPressed;
    
    public void Start()
    {
        _running = true;
        _thread = new System.Threading.Thread(ReadKeys) { IsBackground = true };
        _thread.Start();
    }
    
    public void Stop()
    {
        _running = false;
        _thread?.Join(100);
    }
    
    private void ReadKeys()
    {
        while (_running)
        {
            try
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    KeyPressed?.Invoke(this, new KeyEventArgs
                    {
                        Key = keyInfo.Key,
                        KeyChar = keyInfo.KeyChar,
                        Modifiers = GetModifiers(keyInfo)
                    });
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (InvalidOperationException)
            {
                // Console not available in this environment
                System.Threading.Thread.Sleep(100);
            }
        }
    }
    
    private System.ConsoleModifiers GetModifiers(ConsoleKeyInfo keyInfo)
    {
        return keyInfo.Modifiers;
    }
}

public class KeyEventArgs : EventArgs
{
    public ConsoleKey Key { get; set; }
    public char KeyChar { get; set; }
    public System.ConsoleModifiers Modifiers { get; set; }
}