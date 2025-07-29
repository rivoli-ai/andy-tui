using Andy.TUI.Components.Layout;
using Andy.TUI.Terminal;

namespace Andy.TUI.Components.Input;

/// <summary>
/// Base class for input components that handle keyboard and mouse input.
/// </summary>
public abstract class InputComponent : LayoutComponent
{
    private bool _isFocused;
    private bool _isEnabled = true;
    
    /// <summary>
    /// Gets or sets whether this component has keyboard focus.
    /// </summary>
    public bool IsFocused
    {
        get => _isFocused;
        set
        {
            if (_isFocused != value)
            {
                _isFocused = value;
                OnFocusChanged();
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets whether this component is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                if (!value && IsFocused)
                {
                    IsFocused = false;
                }
                OnEnabledChanged();
                RequestRender();
            }
        }
    }
    
    /// <summary>
    /// Gets or sets the tab index for keyboard navigation.
    /// </summary>
    public int TabIndex { get; set; }
    
    /// <summary>
    /// Occurs when the component gains or loses focus.
    /// </summary>
    public event EventHandler<FocusEventArgs>? FocusChanged;
    
    /// <summary>
    /// Handles keyboard input when this component has focus.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns>True if the input was handled, false otherwise.</returns>
    public virtual bool HandleKeyPress(KeyEventArgs args)
    {
        if (!IsEnabled || !IsFocused)
            return false;
            
        return OnKeyPress(args);
    }
    
    /// <summary>
    /// Handles mouse input for this component.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <returns>True if the input was handled, false otherwise.</returns>
    public virtual bool HandleMouseEvent(MouseEventArgs args)
    {
        if (!IsEnabled)
            return false;
            
        // Check if the mouse event is within our bounds
        if (IsPointInBounds(args.X, args.Y))
        {
            return OnMouseEvent(args);
        }
        
        return false;
    }
    
    /// <summary>
    /// Requests focus for this component.
    /// </summary>
    public void Focus()
    {
        if (IsEnabled && !IsFocused)
        {
            Context?.SetFocus(this);
        }
    }
    
    /// <summary>
    /// Removes focus from this component.
    /// </summary>
    public void Blur()
    {
        if (IsFocused)
        {
            Context?.ClearFocus();
        }
    }
    
    /// <summary>
    /// When overridden in a derived class, handles keyboard input.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns>True if the input was handled, false otherwise.</returns>
    protected abstract bool OnKeyPress(KeyEventArgs args);
    
    /// <summary>
    /// When overridden in a derived class, handles mouse input.
    /// </summary>
    /// <param name="args">The mouse event arguments.</param>
    /// <returns>True if the input was handled, false otherwise.</returns>
    protected virtual bool OnMouseEvent(MouseEventArgs args)
    {
        // Default implementation - request focus on click
        if (args.Button == MouseButton.Left)
        {
            Focus();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Called when the focus state changes.
    /// </summary>
    protected virtual void OnFocusChanged()
    {
        FocusChanged?.Invoke(this, new FocusEventArgs(IsFocused));
    }
    
    /// <summary>
    /// Called when the enabled state changes.
    /// </summary>
    protected virtual void OnEnabledChanged()
    {
    }
    
    /// <summary>
    /// Checks if a point is within the component's bounds.
    /// </summary>
    protected bool IsPointInBounds(int x, int y)
    {
        return x >= Bounds.X && x < Bounds.Right &&
               y >= Bounds.Y && y < Bounds.Bottom;
    }
}

/// <summary>
/// Event arguments for focus events.
/// </summary>
public class FocusEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether the component has focus.
    /// </summary>
    public bool HasFocus { get; }
    
    public FocusEventArgs(bool hasFocus)
    {
        HasFocus = hasFocus;
    }
}