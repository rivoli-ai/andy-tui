namespace Andy.TUI.Declarative;

/// <summary>
/// Interface for components that can receive keyboard focus.
/// </summary>
public interface IFocusable : ISimpleComponent
{
    /// <summary>
    /// Gets whether this component can currently receive focus.
    /// </summary>
    bool CanFocus { get; }
    
    /// <summary>
    /// Gets whether this component currently has focus.
    /// </summary>
    bool IsFocused { get; }
    
    /// <summary>
    /// Called when the component receives focus.
    /// </summary>
    void OnGotFocus();
    
    /// <summary>
    /// Called when the component loses focus.
    /// </summary>
    void OnLostFocus();
    
    /// <summary>
    /// Handles keyboard input when focused.
    /// </summary>
    /// <param name="keyInfo">The key press information.</param>
    /// <returns>True if the key was handled, false otherwise.</returns>
    bool HandleKeyPress(ConsoleKeyInfo keyInfo);
}