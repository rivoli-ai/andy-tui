using System;
using Andy.TUI.Declarative.Focus;

namespace Andy.TUI.Declarative.Events;

/// <summary>
/// Routes input events to the appropriate components.
/// </summary>
public class EventRouter
{
    private readonly DeclarativeContext _context;
    
    public EventRouter(DeclarativeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    /// <summary>
    /// Routes a keyboard event to the focused component.
    /// </summary>
    public bool RouteKeyPress(ConsoleKeyInfo keyInfo)
    {
        // Handle focus navigation
        if (keyInfo.Key == ConsoleKey.Tab)
        {
            var direction = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift) 
                ? FocusDirection.Previous 
                : FocusDirection.Next;
            _context.FocusManager.MoveFocus(direction);
            _context.RequestRender();
            return true;
        }
        
        // Route to focused component
        var focused = _context.FocusManager.FocusedComponent;
        if (focused?.HandleKeyPress(keyInfo) == true)
        {
            _context.RequestRender();
            return true;
        }
        
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