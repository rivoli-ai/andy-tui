namespace Andy.TUI.Terminal;

/// <summary>
/// Defines the interface for handling terminal input.
/// </summary>
public interface IInputHandler : IDisposable
{
    /// <summary>
    /// Event raised when a key is pressed.
    /// </summary>
    event EventHandler<KeyEventArgs>? KeyPressed;
    
    /// <summary>
    /// Event raised when the mouse is moved.
    /// </summary>
    event EventHandler<MouseEventArgs>? MouseMoved;
    
    /// <summary>
    /// Event raised when a mouse button is pressed.
    /// </summary>
    event EventHandler<MouseEventArgs>? MousePressed;
    
    /// <summary>
    /// Event raised when a mouse button is released.
    /// </summary>
    event EventHandler<MouseEventArgs>? MouseReleased;
    
    /// <summary>
    /// Event raised when the mouse wheel is scrolled.
    /// </summary>
    event EventHandler<MouseWheelEventArgs>? MouseWheel;
    
    /// <summary>
    /// Gets whether mouse input is supported.
    /// </summary>
    bool SupportsMouseInput { get; }
    
    /// <summary>
    /// Starts listening for input events.
    /// </summary>
    void Start();
    
    /// <summary>
    /// Stops listening for input events.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Polls for input events. Should be called regularly in the main loop.
    /// </summary>
    void Poll();
}

/// <summary>
/// Event arguments for keyboard input.
/// </summary>
public class KeyEventArgs : EventArgs
{
    /// <summary>
    /// Gets the key that was pressed.
    /// </summary>
    public ConsoleKey Key { get; }
    
    /// <summary>
    /// Gets the character representation of the key.
    /// </summary>
    public char KeyChar { get; }
    
    /// <summary>
    /// Gets the modifier keys that were pressed.
    /// </summary>
    public ConsoleModifiers Modifiers { get; }
    
    /// <summary>
    /// Gets or sets whether this event has been handled.
    /// </summary>
    public bool Handled { get; set; }
    
    public KeyEventArgs(ConsoleKey key, char keyChar, ConsoleModifiers modifiers)
    {
        Key = key;
        KeyChar = keyChar;
        Modifiers = modifiers;
    }
    
    public KeyEventArgs(ConsoleKeyInfo keyInfo)
        : this(keyInfo.Key, keyInfo.KeyChar, keyInfo.Modifiers)
    {
    }
    
    /// <summary>
    /// Gets whether the Shift key was pressed.
    /// </summary>
    public bool Shift => (Modifiers & ConsoleModifiers.Shift) != 0;
    
    /// <summary>
    /// Gets whether the Alt key was pressed.
    /// </summary>
    public bool Alt => (Modifiers & ConsoleModifiers.Alt) != 0;
    
    /// <summary>
    /// Gets whether the Control key was pressed.
    /// </summary>
    public bool Control => (Modifiers & ConsoleModifiers.Control) != 0;
}

/// <summary>
/// Event arguments for mouse input.
/// </summary>
public class MouseEventArgs : EventArgs
{
    /// <summary>
    /// Gets the X coordinate of the mouse.
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// Gets the Y coordinate of the mouse.
    /// </summary>
    public int Y { get; }
    
    /// <summary>
    /// Gets which mouse button was involved.
    /// </summary>
    public MouseButton Button { get; }
    
    /// <summary>
    /// Gets the modifier keys that were pressed.
    /// </summary>
    public ConsoleModifiers Modifiers { get; }
    
    /// <summary>
    /// Gets or sets whether this event has been handled.
    /// </summary>
    public bool Handled { get; set; }
    
    public MouseEventArgs(int x, int y, MouseButton button, ConsoleModifiers modifiers = 0)
    {
        X = x;
        Y = y;
        Button = button;
        Modifiers = modifiers;
    }
}

/// <summary>
/// Event arguments for mouse wheel input.
/// </summary>
public class MouseWheelEventArgs : MouseEventArgs
{
    /// <summary>
    /// Gets the direction and amount of wheel movement.
    /// Positive values indicate scrolling up, negative values indicate scrolling down.
    /// </summary>
    public int Delta { get; }
    
    public MouseWheelEventArgs(int x, int y, int delta, ConsoleModifiers modifiers = 0)
        : base(x, y, MouseButton.None, modifiers)
    {
        Delta = delta;
    }
}

/// <summary>
/// Defines mouse button types.
/// </summary>
public enum MouseButton
{
    /// <summary>
    /// No button.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Left mouse button.
    /// </summary>
    Left = 1,
    
    /// <summary>
    /// Middle mouse button.
    /// </summary>
    Middle = 2,
    
    /// <summary>
    /// Right mouse button.
    /// </summary>
    Right = 3
}