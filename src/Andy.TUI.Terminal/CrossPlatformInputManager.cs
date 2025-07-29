using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace Andy.TUI.Terminal;

/// <summary>
/// Cross-platform input manager that handles keyboard, mouse, and terminal events.
/// </summary>
public class CrossPlatformInputManager : IInputManager
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentQueue<InputEvent> _inputBuffer;
    private readonly object _lockObject = new();
    
    private Task? _inputTask;
    private Task? _resizeTask;
    private bool _isRunning;
    private bool _disposed;
    private bool _mouseInputEnabled;
    private (int X, int Y)? _lastMousePosition;
    private (int X, int Y)? _dragStart;
    private MouseButton _pressedButton = MouseButton.None;
    private int _terminalWidth;
    private int _terminalHeight;
    
    public event EventHandler<InputEvent>? InputReceived;
    
    public bool SupportsMouseInput { get; }
    public bool IsRunning => _isRunning;
    public bool BufferingEnabled { get; set; } = true;
    public int BufferSize { get; set; } = 1000;
    
    public CrossPlatformInputManager()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _inputBuffer = new ConcurrentQueue<InputEvent>();
        
        // Detect mouse support based on platform and terminal
        SupportsMouseInput = DetectMouseSupport();
        
        // Initialize terminal size
        _terminalWidth = Console.WindowWidth;
        _terminalHeight = Console.WindowHeight;
    }
    
    public void Start()
    {
        if (_isRunning)
            return;
            
        _isRunning = true;
        
        // Start keyboard input task
        _inputTask = Task.Run(HandleInputAsync, _cancellationTokenSource.Token);
        
        // Start resize monitoring task
        _resizeTask = Task.Run(MonitorResizeAsync, _cancellationTokenSource.Token);
    }
    
    public void Stop()
    {
        if (!_isRunning)
            return;
            
        _isRunning = false;
        DisableMouseInput();
        _cancellationTokenSource.Cancel();
        
        try
        {
            Task.WaitAll(new[] { _inputTask, _resizeTask }.Where(t => t != null).ToArray()!, 
                        TimeSpan.FromSeconds(1));
        }
        catch
        {
            // Ignore exceptions during shutdown
        }
    }
    
    public void Poll()
    {
        // Input is handled by background tasks, but we can trigger immediate processing
        ProcessBufferedEvents();
    }
    
    public void EnableMouseInput()
    {
        if (!SupportsMouseInput || _mouseInputEnabled)
            return;
            
        _mouseInputEnabled = true;
        
        // Enable mouse reporting in terminal
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            EnableWindowsMouseInput();
        }
        else
        {
            EnableUnixMouseInput();
        }
    }
    
    public void DisableMouseInput()
    {
        if (!_mouseInputEnabled)
            return;
            
        _mouseInputEnabled = false;
        
        // Disable mouse reporting in terminal
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DisableWindowsMouseInput();
        }
        else
        {
            DisableUnixMouseInput();
        }
    }
    
    public InputEvent[] FlushBuffer()
    {
        var events = new List<InputEvent>();
        
        while (_inputBuffer.TryDequeue(out var inputEvent))
        {
            events.Add(inputEvent);
        }
        
        return events.ToArray();
    }
    
    public void ClearBuffer()
    {
        while (_inputBuffer.TryDequeue(out _))
        {
            // Just clear the buffer
        }
    }
    
    public (int X, int Y)? GetMousePosition()
    {
        return _lastMousePosition;
    }
    
    private async Task HandleInputAsync()
    {
        var buffer = new StringBuilder();
        
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    
                    // Check if this is the start of an escape sequence
                    if (keyInfo.Key == ConsoleKey.Escape && Console.KeyAvailable)
                    {
                        await ProcessEscapeSequenceAsync(buffer);
                    }
                    else
                    {
                        // Regular key press
                        var inputEvent = new InputEvent(InputEventType.KeyPress, new KeyInfo(keyInfo));
                        ProcessInputEvent(inputEvent);
                    }
                }
                else
                {
                    await Task.Delay(10, _cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Ignore input exceptions
            }
        }
    }
    
    private async Task ProcessEscapeSequenceAsync(StringBuilder buffer)
    {
        buffer.Clear();
        buffer.Append((char)27); // ESC
        
        // Read the escape sequence with timeout
        var timeout = DateTime.UtcNow.AddMilliseconds(100);
        
        while (DateTime.UtcNow < timeout && Console.KeyAvailable)
        {
            var keyInfo = Console.ReadKey(intercept: true);
            buffer.Append(keyInfo.KeyChar);
            
            // Check if we have a complete sequence
            var sequence = buffer.ToString();
            
            if (TryParseEscapeSequence(sequence, out var inputEvent) && inputEvent != null)
            {
                ProcessInputEvent(inputEvent);
                return;
            }
            
            await Task.Delay(1);
        }
        
        // If we get here, treat as a regular escape key
        var escapeEvent = new InputEvent(InputEventType.KeyPress, 
            new KeyInfo(ConsoleKey.Escape, (char)27, 0));
        ProcessInputEvent(escapeEvent);
    }
    
    private bool TryParseEscapeSequence(string sequence, out InputEvent? inputEvent)
    {
        inputEvent = null;
        
        // Mouse sequences
        if (_mouseInputEnabled && TryParseMouseSequence(sequence, out inputEvent))
        {
            return true;
        }
        
        // Special key sequences
        if (TryParseSpecialKeySequence(sequence, out inputEvent))
        {
            return true;
        }
        
        return false;
    }
    
    private bool TryParseMouseSequence(string sequence, out InputEvent? inputEvent)
    {
        inputEvent = null;
        
        // SGR mouse sequences: ESC[<...M or ESC[<...m
        if (sequence.StartsWith("\x1b[<"))
        {
            var parts = sequence.Substring(3).Split(';');
            if (parts.Length >= 3 && (sequence.EndsWith('M') || sequence.EndsWith('m')))
            {
                if (int.TryParse(parts[0], out var cb) && 
                    int.TryParse(parts[1], out var cx) && 
                    int.TryParse(parts[2].TrimEnd('M', 'm'), out var cy))
                {
                    var isPress = sequence.EndsWith('M');
                    var button = (cb & 0x03) switch
                    {
                        0 => MouseButton.Left,
                        1 => MouseButton.Middle,
                        2 => MouseButton.Right,
                        _ => MouseButton.None
                    };
                    
                    // Extract modifier keys from control byte
                    var modifiers = (ConsoleModifiers)0;
                    if ((cb & 0x04) != 0) modifiers |= ConsoleModifiers.Shift;
                    if ((cb & 0x08) != 0) modifiers |= ConsoleModifiers.Alt;
                    if ((cb & 0x10) != 0) modifiers |= ConsoleModifiers.Control;
                    
                    // Check for Command key (macOS) - might be bit 5 (0x20) in some terminals
                    var command = (cb & 0x20) != 0;
                    
                    var eventType = isPress ? InputEventType.MousePress : InputEventType.MouseRelease;
                    var mouseInfo = new MouseInfo(cx, cy, button, modifiers, 0, false, null, command);
                    inputEvent = new InputEvent(eventType, mouseInfo);
                    return true;
                }
            }
        }
        
        // Standard mouse sequences: ESC[M followed by 3 bytes
        if (sequence.StartsWith("\x1b[M") && sequence.Length >= 6)
        {
            var cb = sequence[3] - 32;  // Control byte
            var cx = sequence[4] - 33;  // X coordinate (1-based)
            var cy = sequence[5] - 33;  // Y coordinate (1-based)
            
            var button = MouseButton.None;
            var eventType = InputEventType.MouseMove;
            
            // Extract modifier keys from control byte
            var modifiers = (ConsoleModifiers)0;
            if ((cb & 0x04) != 0) modifiers |= ConsoleModifiers.Shift;
            if ((cb & 0x08) != 0) modifiers |= ConsoleModifiers.Alt;
            if ((cb & 0x10) != 0) modifiers |= ConsoleModifiers.Control;
            
            // Check for Command key (macOS) - experimental, might conflict with drag bit
            // Note: bit 5 (0x20) is typically used for drag, so Command detection might not work here
            var commandBit = (cb & 0x80) != 0; // Try bit 7 instead for Command
            
            // Parse button and event type
            var buttonBits = cb & 0x03;
            var isDrag = (cb & 0x20) != 0;
            var isRelease = (cb & 0x03) == 3;
            
            if (!isRelease)
            {
                button = buttonBits switch
                {
                    0 => MouseButton.Left,
                    1 => MouseButton.Middle,
                    2 => MouseButton.Right,
                    _ => MouseButton.None
                };
                
                eventType = isDrag ? InputEventType.MouseMove : InputEventType.MousePress;
                
                if (!isDrag)
                {
                    _pressedButton = button;
                    _dragStart = (cx, cy);
                }
            }
            else
            {
                eventType = InputEventType.MouseRelease;
                button = _pressedButton;
                _pressedButton = MouseButton.None;
                _dragStart = null;
            }
            
            // Handle wheel events
            if ((cb & 0x40) != 0)
            {
                eventType = InputEventType.MouseWheel;
                var wheelDelta = buttonBits == 0 ? 1 : -1; // Up or down
                
                var mouseInfo = new MouseInfo(cx, cy, MouseButton.None, modifiers, wheelDelta, false, null, commandBit);
                inputEvent = new InputEvent(eventType, mouseInfo);
                _lastMousePosition = (cx, cy);
                return true;
            }
            
            var isDragOperation = isDrag && _dragStart.HasValue;
            var mouseInfoRegular = new MouseInfo(cx, cy, button, modifiers, 0, isDragOperation, _dragStart, commandBit);
            inputEvent = new InputEvent(eventType, mouseInfoRegular);
            _lastMousePosition = (cx, cy);
            return true;
        }
        
        return false;
    }
    
    private bool TryParseSpecialKeySequence(string sequence, out InputEvent? inputEvent)
    {
        inputEvent = null;
        
        // Common special key sequences
        var specialKeys = new Dictionary<string, (ConsoleKey key, ConsoleModifiers modifiers)>
        {
            { "\x1b[A", (ConsoleKey.UpArrow, 0) },
            { "\x1b[B", (ConsoleKey.DownArrow, 0) },
            { "\x1b[C", (ConsoleKey.RightArrow, 0) },
            { "\x1b[D", (ConsoleKey.LeftArrow, 0) },
            { "\x1b[H", (ConsoleKey.Home, 0) },
            { "\x1b[F", (ConsoleKey.End, 0) },
            { "\x1b[3~", (ConsoleKey.Delete, 0) },
            { "\x1b[2~", (ConsoleKey.Insert, 0) },
            { "\x1b[5~", (ConsoleKey.PageUp, 0) },
            { "\x1b[6~", (ConsoleKey.PageDown, 0) },
            
            // Function keys
            { "\x1bOP", (ConsoleKey.F1, 0) },
            { "\x1bOQ", (ConsoleKey.F2, 0) },
            { "\x1bOR", (ConsoleKey.F3, 0) },
            { "\x1bOS", (ConsoleKey.F4, 0) },
            { "\x1b[15~", (ConsoleKey.F5, 0) },
            { "\x1b[17~", (ConsoleKey.F6, 0) },
            { "\x1b[18~", (ConsoleKey.F7, 0) },
            { "\x1b[19~", (ConsoleKey.F8, 0) },
            { "\x1b[20~", (ConsoleKey.F9, 0) },
            { "\x1b[21~", (ConsoleKey.F10, 0) },
            { "\x1b[23~", (ConsoleKey.F11, 0) },
            { "\x1b[24~", (ConsoleKey.F12, 0) },
        };
        
        if (specialKeys.TryGetValue(sequence, out var keyData))
        {
            var keyInfo = new KeyInfo(keyData.key, (char)0, keyData.modifiers, sequence);
            inputEvent = new InputEvent(InputEventType.KeyPress, keyInfo);
            return true;
        }
        
        return false;
    }
    
    private async Task MonitorResizeAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                var currentWidth = Console.WindowWidth;
                var currentHeight = Console.WindowHeight;
                
                if (currentWidth != _terminalWidth || currentHeight != _terminalHeight)
                {
                    var resizeInfo = new ResizeInfo(currentWidth, currentHeight, _terminalWidth, _terminalHeight);
                    var resizeEvent = new InputEvent(resizeInfo);
                    
                    ProcessInputEvent(resizeEvent);
                    
                    _terminalWidth = currentWidth;
                    _terminalHeight = currentHeight;
                }
                
                await Task.Delay(250, _cancellationTokenSource.Token); // Check 4 times per second
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Ignore resize monitoring exceptions
            }
        }
    }
    
    private void ProcessInputEvent(InputEvent inputEvent)
    {
        // Add to buffer if buffering is enabled
        if (BufferingEnabled)
        {
            _inputBuffer.Enqueue(inputEvent);
            
            // Trim buffer if it's too large
            while (_inputBuffer.Count > BufferSize && _inputBuffer.TryDequeue(out _))
            {
                // Just remove excess events
            }
        }
        
        // Raise event
        InputReceived?.Invoke(this, inputEvent);
    }
    
    private void ProcessBufferedEvents()
    {
        // This method could be used for batch processing if needed
        // For now, events are processed immediately
    }
    
    private bool DetectMouseSupport()
    {
        // Check environment variables and terminal capabilities
        var term = Environment.GetEnvironmentVariable("TERM");
        var termProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
        
        // Most modern terminals support mouse input
        if (!string.IsNullOrEmpty(term))
        {
            var supportedTerms = new[] { "xterm", "screen", "tmux", "alacritty", "kitty" };
            if (supportedTerms.Any(t => term.Contains(t, StringComparison.OrdinalIgnoreCase)))
                return true;
        }
        
        if (!string.IsNullOrEmpty(termProgram))
        {
            var supportedPrograms = new[] { "vscode", "hyper", "iterm", "terminal", "ghostty" };
            if (supportedPrograms.Any(p => termProgram.Contains(p, StringComparison.OrdinalIgnoreCase)))
                return true;
        }
        
        // Windows Terminal always supports mouse
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return true;
        
        return false;
    }
    
    private void EnableWindowsMouseInput()
    {
        // Windows console mouse input would require P/Invoke to enable
        // For now, we'll just mark it as enabled
    }
    
    private void DisableWindowsMouseInput()
    {
        // Windows console mouse input cleanup
    }
    
    private void EnableUnixMouseInput()
    {
        // Enable mouse reporting in Unix terminals
        Console.Write("\x1b[?1000h"); // Normal mouse reporting
        Console.Write("\x1b[?1002h"); // Button event mouse reporting
        Console.Write("\x1b[?1015h"); // Extended mouse reporting
        Console.Write("\x1b[?1006h"); // SGR mouse reporting
    }
    
    private void DisableUnixMouseInput()
    {
        // Disable mouse reporting in Unix terminals
        Console.Write("\x1b[?1000l");
        Console.Write("\x1b[?1002l");
        Console.Write("\x1b[?1015l");
        Console.Write("\x1b[?1006l");
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
            
        Stop();
        _cancellationTokenSource.Dispose();
        _disposed = true;
    }
}