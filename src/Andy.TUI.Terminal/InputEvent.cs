namespace Andy.TUI.Terminal;

/// <summary>
/// Represents different types of input events.
/// </summary>
public enum InputEventType
{
    /// <summary>
    /// A key was pressed.
    /// </summary>
    KeyPress,
    
    /// <summary>
    /// A key was released.
    /// </summary>
    KeyRelease,
    
    /// <summary>
    /// The mouse was moved.
    /// </summary>
    MouseMove,
    
    /// <summary>
    /// A mouse button was pressed.
    /// </summary>
    MousePress,
    
    /// <summary>
    /// A mouse button was released.
    /// </summary>
    MouseRelease,
    
    /// <summary>
    /// The mouse wheel was scrolled.
    /// </summary>
    MouseWheel,
    
    /// <summary>
    /// The terminal was resized.
    /// </summary>
    Resize
}

/// <summary>
/// Unified input event that can represent keyboard, mouse, and terminal events.
/// </summary>
public class InputEvent
{
    /// <summary>
    /// Gets the type of input event.
    /// </summary>
    public InputEventType Type { get; }
    
    /// <summary>
    /// Gets the keyboard information for key events.
    /// </summary>
    public KeyInfo? Key { get; }
    
    /// <summary>
    /// Gets the mouse information for mouse events.
    /// </summary>
    public MouseInfo? Mouse { get; }
    
    /// <summary>
    /// Gets the resize information for resize events.
    /// </summary>
    public ResizeInfo? Resize { get; }
    
    /// <summary>
    /// Gets the timestamp when this event occurred.
    /// </summary>
    public DateTime Timestamp { get; }
    
    /// <summary>
    /// Gets or sets whether this event has been handled.
    /// </summary>
    public bool Handled { get; set; }
    
    /// <summary>
    /// Creates a new keyboard input event.
    /// </summary>
    public InputEvent(InputEventType type, KeyInfo keyInfo)
    {
        Type = type;
        Key = keyInfo;
        Timestamp = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Creates a new mouse input event.
    /// </summary>
    public InputEvent(InputEventType type, MouseInfo mouseInfo)
    {
        Type = type;
        Mouse = mouseInfo;
        Timestamp = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Creates a new resize input event.
    /// </summary>
    public InputEvent(ResizeInfo resizeInfo)
    {
        Type = InputEventType.Resize;
        Resize = resizeInfo;
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Enhanced keyboard information with special key sequence support.
/// </summary>
public class KeyInfo
{
    /// <summary>
    /// Gets the console key.
    /// </summary>
    public ConsoleKey Key { get; }
    
    /// <summary>
    /// Gets the character representation of the key.
    /// </summary>
    public char KeyChar { get; }
    
    /// <summary>
    /// Gets the modifier keys.
    /// </summary>
    public ConsoleModifiers Modifiers { get; }
    
    /// <summary>
    /// Gets the raw escape sequence for special keys.
    /// </summary>
    public string? EscapeSequence { get; }
    
    /// <summary>
    /// Gets whether the Command key (⌘) was pressed on macOS.
    /// </summary>
    public bool Command { get; }
    
    /// <summary>
    /// Gets whether this is a special key sequence.
    /// </summary>
    public bool IsSpecialKey => !string.IsNullOrEmpty(EscapeSequence);
    
    public KeyInfo(ConsoleKey key, char keyChar, ConsoleModifiers modifiers, string? escapeSequence = null, bool command = false)
    {
        Key = key;
        KeyChar = keyChar;
        Modifiers = modifiers;
        EscapeSequence = escapeSequence;
        Command = command;
    }
    
    public KeyInfo(ConsoleKeyInfo keyInfo) 
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
    
    public override string ToString()
    {
        var parts = new List<string>();
        
        if (Control) parts.Add("Ctrl");
        if (Alt) parts.Add("Alt");
        if (Shift) parts.Add("Shift");
        if (Command) parts.Add("Cmd");
        
        parts.Add(Key.ToString());
        
        return string.Join("+", parts);
    }
}

/// <summary>
/// Enhanced mouse information with drag and wheel support.
/// </summary>
public class MouseInfo
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
    /// Gets whether the Command key (⌘) was pressed on macOS.
    /// </summary>
    public bool Command { get; }
    
    /// <summary>
    /// Gets the wheel delta for wheel events.
    /// </summary>
    public int WheelDelta { get; }
    
    /// <summary>
    /// Gets whether this is a drag operation.
    /// </summary>
    public bool IsDrag { get; }
    
    /// <summary>
    /// Gets the starting position for drag operations.
    /// </summary>
    public (int X, int Y)? DragStart { get; }
    
    public MouseInfo(int x, int y, MouseButton button, ConsoleModifiers modifiers = 0, 
                    int wheelDelta = 0, bool isDrag = false, (int X, int Y)? dragStart = null, bool command = false)
    {
        X = x;
        Y = y;
        Button = button;
        Modifiers = modifiers;
        Command = command;
        WheelDelta = wheelDelta;
        IsDrag = isDrag;
        DragStart = dragStart;
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
    
    public override string ToString()
    {
        var parts = new List<string>();
        
        if (Control) parts.Add("Ctrl");
        if (Alt) parts.Add("Alt");
        if (Shift) parts.Add("Shift");
        if (Command) parts.Add("Cmd");
        
        if (Button != MouseButton.None)
            parts.Add($"{Button}Button");
            
        parts.Add($"({X},{Y})");
        
        if (WheelDelta != 0)
            parts.Add($"Wheel:{WheelDelta}");
            
        if (IsDrag)
            parts.Add("Drag");
        
        return string.Join(" ", parts);
    }
}

/// <summary>
/// Terminal resize information.
/// </summary>
public class ResizeInfo
{
    /// <summary>
    /// Gets the new width of the terminal.
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// Gets the new height of the terminal.
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// Gets the previous width of the terminal.
    /// </summary>
    public int PreviousWidth { get; }
    
    /// <summary>
    /// Gets the previous height of the terminal.
    /// </summary>
    public int PreviousHeight { get; }
    
    public ResizeInfo(int width, int height, int previousWidth, int previousHeight)
    {
        Width = width;
        Height = height;
        PreviousWidth = previousWidth;
        PreviousHeight = previousHeight;
    }
    
    public override string ToString()
    {
        return $"Resize: {PreviousWidth}x{PreviousHeight} -> {Width}x{Height}";
    }
}