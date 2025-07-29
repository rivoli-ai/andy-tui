using System.Runtime.InteropServices;
using System.Text;

namespace Andy.TUI.Terminal;

/// <summary>
/// ANSI terminal implementation that uses escape sequences for terminal control.
/// </summary>
public class AnsiTerminal : ITerminal, IDisposable
{
    private readonly TextWriter _output;
    private readonly StringBuilder _buffer;
    private int _savedColumn;
    private int _savedRow;
    private int _width;
    private int _height;
    private bool _alternateScreen;
    
    public int Width => _width;
    public int Height => _height;
    
    public (int Column, int Row) CursorPosition
    {
        get => (Console.CursorLeft, Console.CursorTop);
        set => MoveCursor(value.Column, value.Row);
    }
    
    public bool CursorVisible
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Console.CursorVisible;
            }
            // Default to true for non-Windows platforms
            return true;
        }
        set
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.CursorVisible = value;
            }
            _buffer.Append(value ? "\x1b[?25h" : "\x1b[?25l");
        }
    }
    
    public bool SupportsColor => true;
    public bool SupportsAnsi => true;
    
    public event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged;
    
    public AnsiTerminal() : this(Console.Out)
    {
    }
    
    public AnsiTerminal(TextWriter output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _buffer = new StringBuilder(1024);
        UpdateSize();
        
        // Start monitoring size changes
        if (OperatingSystem.IsWindows())
        {
            // Windows console doesn't reliably support SIGWINCH
            // We'll check size periodically or on demand
        }
        else
        {
            // Unix-like systems support SIGWINCH
            Console.CancelKeyPress += OnCancelKeyPress;
        }
    }
    
    private void UpdateSize()
    {
        var oldWidth = _width;
        var oldHeight = _height;
        
        try
        {
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
        }
        catch
        {
            // Fallback sizes if console properties aren't available
            _width = 80;
            _height = 24;
        }
        
        if (oldWidth != _width || oldHeight != _height)
        {
            SizeChanged?.Invoke(this, new TerminalSizeChangedEventArgs(_width, _height, oldWidth, oldHeight));
        }
    }
    
    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        // Check for size changes on Ctrl+C (not ideal but works)
        UpdateSize();
    }
    
    public void Clear()
    {
        _buffer.Append("\x1b[2J\x1b[H");
        Flush();
    }
    
    public void ClearLine()
    {
        _buffer.Append("\x1b[K");
    }
    
    public void MoveCursor(int column, int row)
    {
        // ANSI uses 1-based positioning
        _buffer.Append($"\x1b[{row + 1};{column + 1}H");
    }
    
    public void Write(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
            
        _buffer.Append(text);
    }
    
    public void WriteLine(string text)
    {
        Write(text);
        _buffer.AppendLine();
    }
    
    public void SetForegroundColor(ConsoleColor color)
    {
        var ansiCode = GetAnsiForegroundCode(color);
        _buffer.Append($"\x1b[{ansiCode}m");
    }
    
    public void SetBackgroundColor(ConsoleColor color)
    {
        var ansiCode = GetAnsiBackgroundCode(color);
        _buffer.Append($"\x1b[{ansiCode}m");
    }
    
    public void ResetColors()
    {
        _buffer.Append("\x1b[0m");
    }
    
    public void SaveCursorPosition()
    {
        _buffer.Append("\x1b[s");
        var pos = CursorPosition;
        _savedColumn = pos.Column;
        _savedRow = pos.Row;
    }
    
    public void RestoreCursorPosition()
    {
        _buffer.Append("\x1b[u");
    }
    
    public void EnterAlternateScreen()
    {
        if (!_alternateScreen)
        {
            _buffer.Append("\x1b[?1049h");
            _alternateScreen = true;
            Flush();
        }
    }
    
    public void ExitAlternateScreen()
    {
        if (_alternateScreen)
        {
            _buffer.Append("\x1b[?1049l");
            _alternateScreen = false;
            Flush();
        }
    }
    
    public void Flush()
    {
        if (_buffer.Length > 0)
        {
            _output.Write(_buffer.ToString());
            _output.Flush();
            _buffer.Clear();
        }
        
        // Check for size changes
        UpdateSize();
    }
    
    /// <summary>
    /// Applies a style using ANSI escape sequences.
    /// </summary>
    public void ApplyStyle(Style style)
    {
        var codes = new List<int>();
        
        // Reset first if needed
        if (style.Bold || style.Italic || style.Underline || style.Strikethrough || 
            style.Dim || style.Inverse || style.Blink)
        {
            codes.Add(0); // Reset
        }
        
        // Text attributes
        if (style.Bold) codes.Add(1);
        if (style.Dim) codes.Add(2);
        if (style.Italic) codes.Add(3);
        if (style.Underline) codes.Add(4);
        if (style.Blink) codes.Add(5);
        if (style.Inverse) codes.Add(7);
        if (style.Strikethrough) codes.Add(9);
        
        // Foreground color
        if (style.Foreground.Type == ColorType.ConsoleColor && style.Foreground.ConsoleColor.HasValue)
        {
            codes.Add(GetAnsiForegroundCode(style.Foreground.ConsoleColor.Value));
        }
        else if (style.Foreground.Type == ColorType.Rgb && style.Foreground.Rgb.HasValue)
        {
            var rgb = style.Foreground.Rgb.Value;
            _buffer.Append($"\x1b[38;2;{rgb.R};{rgb.G};{rgb.B}m");
        }
        else if (style.Foreground.Type == ColorType.EightBit && style.Foreground.ColorIndex.HasValue)
        {
            _buffer.Append($"\x1b[38;5;{style.Foreground.ColorIndex.Value}m");
        }
        
        // Background color
        if (style.Background.Type == ColorType.ConsoleColor && style.Background.ConsoleColor.HasValue)
        {
            codes.Add(GetAnsiBackgroundCode(style.Background.ConsoleColor.Value));
        }
        else if (style.Background.Type == ColorType.Rgb && style.Background.Rgb.HasValue)
        {
            var rgb = style.Background.Rgb.Value;
            _buffer.Append($"\x1b[48;2;{rgb.R};{rgb.G};{rgb.B}m");
        }
        else if (style.Background.Type == ColorType.EightBit && style.Background.ColorIndex.HasValue)
        {
            _buffer.Append($"\x1b[48;5;{style.Background.ColorIndex.Value}m");
        }
        
        // Apply collected codes
        if (codes.Count > 0)
        {
            _buffer.Append($"\x1b[{string.Join(";", codes)}m");
        }
    }
    
    private static int GetAnsiForegroundCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => 30,
            ConsoleColor.DarkRed => 31,
            ConsoleColor.DarkGreen => 32,
            ConsoleColor.DarkYellow => 33,
            ConsoleColor.DarkBlue => 34,
            ConsoleColor.DarkMagenta => 35,
            ConsoleColor.DarkCyan => 36,
            ConsoleColor.Gray => 37,
            ConsoleColor.DarkGray => 90,
            ConsoleColor.Red => 91,
            ConsoleColor.Green => 92,
            ConsoleColor.Yellow => 93,
            ConsoleColor.Blue => 94,
            ConsoleColor.Magenta => 95,
            ConsoleColor.Cyan => 96,
            ConsoleColor.White => 97,
            _ => 37
        };
    }
    
    private static int GetAnsiBackgroundCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => 40,
            ConsoleColor.DarkRed => 41,
            ConsoleColor.DarkGreen => 42,
            ConsoleColor.DarkYellow => 43,
            ConsoleColor.DarkBlue => 44,
            ConsoleColor.DarkMagenta => 45,
            ConsoleColor.DarkCyan => 46,
            ConsoleColor.Gray => 47,
            ConsoleColor.DarkGray => 100,
            ConsoleColor.Red => 101,
            ConsoleColor.Green => 102,
            ConsoleColor.Yellow => 103,
            ConsoleColor.Blue => 104,
            ConsoleColor.Magenta => 105,
            ConsoleColor.Cyan => 106,
            ConsoleColor.White => 107,
            _ => 40
        };
    }
    
    public void Dispose()
    {
        // Restore cursor visibility on dispose
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.CursorVisible = true;
        }
        else
        {
            _buffer.Append("\x1b[?25h"); // Show cursor using ANSI
        }
        // Ensure any buffered output is flushed
        Flush();
    }
}