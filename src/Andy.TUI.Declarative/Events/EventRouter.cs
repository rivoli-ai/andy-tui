using System;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Focus;

namespace Andy.TUI.Declarative.Events;

/// <summary>
/// Routes input events to the appropriate components.
/// </summary>
public class EventRouter
{
    private readonly DeclarativeContext _context;
    private readonly ILogger _logger;
    
    public EventRouter(DeclarativeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = DebugContext.Logger.ForCategory("EventRouter");
    }
    
    /// <summary>
    /// Routes a keyboard event to the focused component.
    /// </summary>
    public bool RouteKeyPress(ConsoleKeyInfo keyInfo)
    {
        _logger.Debug("RouteKeyPress: {0}", keyInfo.Key);
        
        // Handle focus navigation
        if (keyInfo.Key == ConsoleKey.Tab)
        {
            var direction = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift) 
                ? FocusDirection.Previous 
                : FocusDirection.Next;
            _logger.Debug("Tab navigation: {0}", direction);
            _context.FocusManager.MoveFocus(direction);
            _context.RequestRender();
            return true;
        }
        
        // Route to focused component
        var focused = _context.FocusManager.FocusedComponent;
        _logger.Debug("Focused component: {0}", focused?.GetType().Name ?? "null");
        
        if (focused?.HandleKeyPress(keyInfo) == true)
        {
            _logger.Debug("Key handled by {0}", focused.GetType().Name);
            _context.RequestRender();
            return true;
        }
        
        _logger.Debug("Key not handled");
        return false;
    }
    
    /// <summary>
    /// Routes a mouse event to the component at the specified position.
    /// </summary>
    public bool RouteMouseEvent(int x, int y, MouseButton button)
    {
        // Find component at position
        var component = _context.Bounds.HitTest(x, y);
        
        if (component is IFocusable focusable)
        {
            _context.FocusManager.SetFocus(focusable);
            _context.RequestRender();
            return true;
        }
        
        return false;
    }
}

/// <summary>
/// Represents a mouse button.
/// </summary>
public enum MouseButton
{
    Left,
    Right,
    Middle
}