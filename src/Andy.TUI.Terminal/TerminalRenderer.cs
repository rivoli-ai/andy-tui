using System.Text;

namespace Andy.TUI.Terminal;

/// <summary>
/// A renderer that implements double buffering for efficient terminal rendering.
/// </summary>
public class TerminalRenderer : IRenderer, IDisposable
{
    private readonly ITerminal _terminal;
    private Buffer _frontBuffer;
    private Buffer _backBuffer;
    private Rectangle _clipRegion;
    private bool _inFrame;
    private bool _disposed;
    
    public ITerminal Terminal => _terminal;
    public int Width => _backBuffer.Width;
    public int Height => _backBuffer.Height;
    
    public TerminalRenderer(ITerminal terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _frontBuffer = new Buffer(terminal.Width, terminal.Height);
        _backBuffer = new Buffer(terminal.Width, terminal.Height);
        _clipRegion = new Rectangle(0, 0, terminal.Width, terminal.Height);
        
        // Subscribe to terminal size changes
        _terminal.SizeChanged += OnTerminalSizeChanged;
        
        // Enter alternate screen if supported
        _terminal.EnterAlternateScreen();
    }
    
    private void OnTerminalSizeChanged(object? sender, TerminalSizeChangedEventArgs e)
    {
        // Resize buffers to match new terminal size
        var newFrontBuffer = new Buffer(e.Width, e.Height);
        var newBackBuffer = new Buffer(e.Width, e.Height);
        
        // Copy existing content
        newFrontBuffer.CopyFrom(_frontBuffer);
        newBackBuffer.CopyFrom(_backBuffer);
        
        _frontBuffer = newFrontBuffer;
        _backBuffer = newBackBuffer;
        
        // Update clip region
        _clipRegion = new Rectangle(0, 0, e.Width, e.Height);
        
        // Mark everything as dirty to force redraw
        _backBuffer.MarkAllDirty();
    }
    
    public void BeginFrame()
    {
        if (_inFrame)
            throw new InvalidOperationException("Already in a frame. Call EndFrame first.");
            
        _inFrame = true;
    }
    
    public void EndFrame()
    {
        if (!_inFrame)
            throw new InvalidOperationException("Not in a frame. Call BeginFrame first.");
            
        try
        {
            RenderDifferences();
            
            // Swap buffers
            (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
            
            // Mark all cells in the new back buffer as clean
            _backBuffer.MarkAllClean();
        }
        finally
        {
            _inFrame = false;
        }
    }
    
    private void RenderDifferences()
    {
        var currentStyle = Style.Default;
        var lastStyle = Style.Default;
        var needsStyleReset = true;
        
        // Save cursor position
        _terminal.SaveCursorPosition();
        _terminal.CursorVisible = false;
        
        try
        {
            for (int y = 0; y < Height; y++)
            {
                bool lineHasChanges = false;
                int firstChangeX = -1;
                int lastChangeX = -1;
                
                // Find changed cells in this line
                for (int x = 0; x < Width; x++)
                {
                    var backCell = _backBuffer[x, y];
                    var frontCell = _frontBuffer.TryGetCell(x, y, out var fc) ? fc : Cell.Empty;
                    
                    if (backCell.IsDirty || backCell != frontCell)
                    {
                        if (firstChangeX == -1)
                            firstChangeX = x;
                        lastChangeX = x;
                        lineHasChanges = true;
                    }
                }
                
                if (!lineHasChanges)
                    continue;
                    
                // Move to the first changed position in the line
                _terminal.MoveCursor(firstChangeX, y);
                
                // Render changed cells
                var sb = new StringBuilder();
                for (int x = firstChangeX; x <= lastChangeX; x++)
                {
                    var cell = _backBuffer[x, y];
                    
                    // Apply style if different from current
                    if (!cell.Style.Equals(currentStyle))
                    {
                        if (sb.Length > 0)
                        {
                            _terminal.Write(sb.ToString());
                            sb.Clear();
                        }
                        
                        ApplyStyle(cell.Style, ref currentStyle, ref needsStyleReset);
                    }
                    
                    sb.Append(cell.Character);
                }
                
                if (sb.Length > 0)
                {
                    _terminal.Write(sb.ToString());
                }
            }
            
            // Reset style if needed
            if (needsStyleReset)
            {
                _terminal.ResetColors();
            }
        }
        finally
        {
            // Restore cursor
            _terminal.RestoreCursorPosition();
            _terminal.Flush();
        }
    }
    
    private void ApplyStyle(Style style, ref Style currentStyle, ref bool needsStyleReset)
    {
        if (_terminal is AnsiTerminal ansiTerminal)
        {
            ansiTerminal.ApplyStyle(style);
            currentStyle = style;
            needsStyleReset = true;
        }
        else
        {
            // Fallback for basic terminals - only apply console colors
            if (style.Foreground.Type == ColorType.ConsoleColor && style.Foreground.ConsoleColor.HasValue)
            {
                _terminal.SetForegroundColor(style.Foreground.ConsoleColor.Value);
                needsStyleReset = true;
            }
            
            if (style.Background.Type == ColorType.ConsoleColor && style.Background.ConsoleColor.HasValue)
            {
                _terminal.SetBackgroundColor(style.Background.ConsoleColor.Value);
                needsStyleReset = true;
            }
            
            currentStyle = style;
        }
    }
    
    public void DrawText(int x, int y, string text, Style style = default)
    {
        if (string.IsNullOrEmpty(text))
            return;
            
        for (int i = 0; i < text.Length; i++)
        {
            DrawChar(x + i, y, text[i], style);
        }
    }
    
    public void DrawChar(int x, int y, char ch, Style style = default)
    {
        if (!_clipRegion.Contains(x, y))
            return;
            
        _backBuffer.TrySetCell(x, y, new Cell(ch, style));
    }
    
    public void FillRect(int x, int y, int width, int height, char ch, Style style = default)
    {
        var cell = new Cell(ch, style);
        
        for (int row = y; row < y + height; row++)
        {
            for (int col = x; col < x + width; col++)
            {
                if (_clipRegion.Contains(col, row))
                {
                    _backBuffer.TrySetCell(col, row, cell);
                }
            }
        }
    }
    
    public void DrawBox(int x, int y, int width, int height, BorderStyle borderStyle, Style style = default)
    {
        if (width < 2 || height < 2)
            return;
            
        var chars = BorderChars.GetBorderChars(borderStyle);
        
        // Top border
        DrawChar(x, y, chars.TopLeft, style);
        for (int i = 1; i < width - 1; i++)
        {
            DrawChar(x + i, y, chars.Top, style);
        }
        DrawChar(x + width - 1, y, chars.TopRight, style);
        
        // Side borders
        for (int i = 1; i < height - 1; i++)
        {
            DrawChar(x, y + i, chars.Left, style);
            DrawChar(x + width - 1, y + i, chars.Right, style);
        }
        
        // Bottom border
        DrawChar(x, y + height - 1, chars.BottomLeft, style);
        for (int i = 1; i < width - 1; i++)
        {
            DrawChar(x + i, y + height - 1, chars.Bottom, style);
        }
        DrawChar(x + width - 1, y + height - 1, chars.BottomRight, style);
    }
    
    public void Clear()
    {
        _backBuffer.Clear();
    }
    
    public void ClearRect(int x, int y, int width, int height)
    {
        _backBuffer.ClearRect(x, y, width, height);
    }
    
    public void SetClipRegion(int x, int y, int width, int height)
    {
        _clipRegion = new Rectangle(
            Math.Max(0, x),
            Math.Max(0, y),
            Math.Min(Width - x, width),
            Math.Min(Height - y, height)
        );
    }
    
    public void ResetClipRegion()
    {
        _clipRegion = new Rectangle(0, 0, Width, Height);
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
            
        _terminal.SizeChanged -= OnTerminalSizeChanged;
        _terminal.ExitAlternateScreen();
        _terminal.ResetColors();
        _terminal.CursorVisible = true;
        _terminal.Flush();
        
        _disposed = true;
    }
    
    private readonly struct Rectangle
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        public bool Contains(int x, int y)
        {
            return x >= X && x < X + Width && y >= Y && y < Y + Height;
        }
    }
}