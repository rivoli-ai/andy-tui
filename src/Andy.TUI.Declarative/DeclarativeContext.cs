using Andy.TUI.Declarative.Focus;
using Andy.TUI.Declarative.Events;
using System;

namespace Andy.TUI.Declarative;

/// <summary>
/// Provides context for declarative components including focus management, event routing, and rendering.
/// </summary>
public class DeclarativeContext
{
    private readonly Action _requestRender;
    
    /// <summary>
    /// Gets the focus manager for this context.
    /// </summary>
    public FocusManager FocusManager { get; }
    
    /// <summary>
    /// Gets the event router for this context.
    /// </summary>
    public EventRouter EventRouter { get; }
    
    /// <summary>
    /// Gets the component bounds for hit testing and layout.
    /// </summary>
    public ComponentBounds Bounds { get; }
    
    /// <summary>
    /// Gets the view instance manager.
    /// </summary>
    public ViewInstanceManager ViewInstanceManager { get; }
    
    public DeclarativeContext(Action requestRender)
    {
        _requestRender = requestRender ?? throw new ArgumentNullException(nameof(requestRender));
        FocusManager = new FocusManager();
        EventRouter = new EventRouter(this);
        Bounds = new ComponentBounds();
        ViewInstanceManager = new ViewInstanceManager(this);
    }
    
    /// <summary>
    /// Requests a re-render of the UI.
    /// </summary>
    public void RequestRender()
    {
        _requestRender();
    }
    
    /// <summary>
    /// Clears all context state.
    /// </summary>
    public void Clear()
    {
        FocusManager.Clear();
        Bounds.Clear();
        ViewInstanceManager.Clear();
    }
}