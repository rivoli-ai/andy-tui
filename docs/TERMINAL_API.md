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

### RenderingSystem

High-level rendering API with automatic double buffering and frame rate control:

```csharp
using var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

// Drawing operations (no need for explicit frame management)
renderingSystem.Clear();
renderingSystem.WriteText(10, 5, "Hello", style);
renderingSystem.DrawChar(20, 10, '█', style);
renderingSystem.DrawLine(0, 15, 80, 15, '─', style);
renderingSystem.DrawBox(0, 0, 80, 24, style, BoxStyle.Double);

// Automatic rendering happens through the scheduler
renderingSystem.Render();
```

### Box Styles

```csharp
public enum BoxStyle
{
    Single,    // ┌─┐│└┘
    Double,    // ╔═╗║╚╝
    Rounded,   // ╭─╮│╰╯
    Heavy,     // ┏━┓┃┗┛
    Ascii      // +-+|+-+
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

### Animation with RenderingSystem

```csharp
var terminal = new AnsiTerminal();
using var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

int frame = 0;
Action renderFrame = null;
renderFrame = () => {
    if (frame >= 100) return;
    
    renderingSystem.Clear();
    
    // Animate a moving box
    int x = frame % (renderingSystem.Terminal.Width - 10);
    renderingSystem.DrawBox(x, 5, 10, 5, BoxStyle.Single);
    
    frame++;
    renderingSystem.Scheduler.QueueRender(renderFrame);
};

renderingSystem.Scheduler.QueueRender(renderFrame);
```

### Styled Text Gallery

```csharp
var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

// Rainbow gradient
for (int i = 0; i < 40; i++)
{
    var hue = i * 9; // 0-360 degrees
    var color = Color.FromRgb(
        (byte)(128 + 127 * Math.Sin(hue * Math.PI / 180)),
        (byte)(128 + 127 * Math.Sin((hue + 120) * Math.PI / 180)),
        (byte)(128 + 127 * Math.Sin((hue + 240) * Math.PI / 180))
    );
    renderingSystem.Buffer.SetCell(i, 10, '█', Style.Default.WithForegroundColor(color));
}

// Text attributes
renderingSystem.WriteText(0, 12, "Bold", Style.Default.WithBold());
renderingSystem.WriteText(10, 12, "Italic", Style.Default.WithItalic());
renderingSystem.WriteText(20, 12, "Underline", Style.Default.WithUnderline());
```

## Performance Considerations

1. **Double Buffering**: The RenderingSystem automatically handles double buffering and only sends changes between frames
2. **Batch Operations**: Use QueueRender callbacks to batch multiple drawing operations
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