# Terminal Abstraction API Reference

## Overview

The Terminal abstraction layer provides a cross-platform interface for terminal operations with support for colors, styles, input handling, and efficient rendering through double buffering.

## Core Components

### ITerminal Interface

The main interface for terminal operations:

```csharp
public interface ITerminal
{
    int Width { get; }
    int Height { get; }
    (int Column, int Row) CursorPosition { get; set; }
    bool CursorVisible { get; set; }
    bool SupportsColor { get; }
    event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged;
    
    void Clear();
    void ClearLine();
    void MoveCursor(int column, int row);
    void Write(string text);
    void WriteLine(string text);
    void SetColors(Color foreground, Color background);
    void ResetColors();
    void ApplyStyle(Style style);
    void Flush();
    void SaveCursorPosition();
    void RestoreCursorPosition();
}
```

### AnsiTerminal

Cross-platform terminal implementation using ANSI escape sequences:

- Automatic platform detection for cursor visibility
- Efficient buffered output
- Size change detection
- Full ANSI escape sequence support

```csharp
using var terminal = new AnsiTerminal();
terminal.Clear();
terminal.MoveCursor(10, 5);
terminal.WriteLine("Hello, Terminal!");
```

## Color System

### Color Struct

Supports multiple color types:

```csharp
// Standard 16 console colors
var red = Color.Red;
var darkBlue = Color.DarkBlue;

// RGB colors (24-bit)
var orange = Color.FromRgb(255, 165, 0);

// 256-color palette
var color = Color.FromIndex(214);

// No color (default)
var none = Color.None;
```

### Predefined Colors

- Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow, Gray
- DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White

## Style System

### Style Struct

Immutable styling with fluent API:

```csharp
// Create styles with fluent API
var style = Style.Default
    .WithForegroundColor(Color.Blue)
    .WithBackgroundColor(Color.White)
    .WithBold()
    .WithUnderline();

// Combine styles
var combined = style1.Merge(style2);

// Static factory methods
var bold = Style.WithBold();
var redText = Style.WithForeground(Color.Red);
```

### Style Attributes

- Bold
- Italic
- Underline
- Strikethrough
- Dim (faint)
- Inverse (reverse video)
- Blink

## Rendering System

### TerminalRenderer

Double-buffered renderer for smooth animations:

```csharp
var renderer = new TerminalRenderer(terminal);

// Begin a new frame
renderer.BeginFrame();

// Drawing operations
renderer.Clear();
renderer.DrawText(10, 5, "Hello", style);
renderer.DrawChar(20, 10, '█', style);
renderer.DrawLine(0, 15, 80, 15, '─', style);
renderer.DrawBox(0, 0, 80, 24, BorderStyle.Double, style);

// Commit the frame (only changes are rendered)
renderer.EndFrame();
```

### Border Styles

```csharp
public enum BorderStyle
{
    Single,    // ┌─┐│└┘
    Double,    // ╔═╗║╚╝
    Rounded,   // ╭─╮│╰╯
    Thick      // ┏━┓┃┗┛
}
```

### Cell and Buffer

Efficient cell-based rendering:

```csharp
// Each cell stores a character and style
public readonly struct Cell
{
    public char Character { get; }
    public Style Style { get; }
}

// Buffer manages a 2D grid of cells
var buffer = new Buffer(80, 24);
buffer.SetCell(10, 5, new Cell('A', style));
```

## Input Handling

### IInputHandler Interface

```csharp
public interface IInputHandler : IDisposable
{
    event EventHandler<KeyEventArgs>? KeyPressed;
    event EventHandler<MouseEventArgs>? MouseMoved;
    event EventHandler<MouseEventArgs>? MousePressed;
    event EventHandler<MouseEventArgs>? MouseReleased;
    event EventHandler<MouseWheelEventArgs>? MouseWheel;
    
    bool SupportsMouseInput { get; }
    void Start();
    void Stop();
    void Poll();
}
```

### ConsoleInputHandler

Basic keyboard input handling:

```csharp
var inputHandler = new ConsoleInputHandler();
inputHandler.KeyPressed += (sender, e) =>
{
    Console.WriteLine($"Key: {e.Key}, Char: {e.KeyChar}");
    if (e.Key == ConsoleKey.Escape)
        Environment.Exit(0);
};
inputHandler.Start();
```

## Usage Examples

### Basic Terminal Operations

```csharp
using var terminal = new AnsiTerminal();

// Clear screen and move cursor
terminal.Clear();
terminal.MoveCursor(10, 5);

// Write colored text
terminal.SetColors(Color.Green, Color.Black);
terminal.WriteLine("Success!");
terminal.ResetColors();

// Save and restore cursor
terminal.SaveCursorPosition();
terminal.MoveCursor(0, 20);
terminal.Write("Status bar");
terminal.RestoreCursorPosition();
```

### Animation with Double Buffering

```csharp
var terminal = new AnsiTerminal();
var renderer = new TerminalRenderer(terminal);

for (int frame = 0; frame < 100; frame++)
{
    renderer.BeginFrame();
    renderer.Clear();
    
    // Animate a moving box
    int x = frame % (renderer.Width - 10);
    renderer.DrawBox(x, 5, 10, 5, BorderStyle.Single);
    
    renderer.EndFrame();
    Thread.Sleep(50);
}
```

### Styled Text Gallery

```csharp
var renderer = new TerminalRenderer(terminal);

renderer.BeginFrame();

// Rainbow gradient
for (int i = 0; i < 40; i++)
{
    var hue = i * 9; // 0-360 degrees
    var color = Color.FromRgb(
        (byte)(128 + 127 * Math.Sin(hue * Math.PI / 180)),
        (byte)(128 + 127 * Math.Sin((hue + 120) * Math.PI / 180)),
        (byte)(128 + 127 * Math.Sin((hue + 240) * Math.PI / 180))
    );
    renderer.DrawChar(i, 10, '█', Style.WithForeground(color));
}

// Text attributes
renderer.DrawText(0, 12, "Bold", Style.WithBold());
renderer.DrawText(10, 12, "Italic", Style.WithItalic());
renderer.DrawText(20, 12, "Underline", Style.WithUnderline());

renderer.EndFrame();
```

## Performance Considerations

1. **Double Buffering**: The TerminalRenderer only sends changes between frames, minimizing terminal I/O
2. **Batch Operations**: Group multiple drawing operations between BeginFrame/EndFrame
3. **String Building**: AnsiTerminal uses StringBuilder for efficient string concatenation
4. **Platform Checks**: Platform-specific code paths are checked once and cached

## Platform Support

- **Windows**: Full support with fallbacks for Windows-specific console APIs
- **macOS/Linux**: Full ANSI escape sequence support
- **SSH/Telnet**: Works over remote connections that support ANSI sequences

## Thread Safety

- ITerminal implementations are not thread-safe by default
- Use external synchronization for multi-threaded access
- Input handlers run on separate threads and use thread-safe event dispatch