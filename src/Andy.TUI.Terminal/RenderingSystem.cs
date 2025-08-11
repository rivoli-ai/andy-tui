namespace Andy.TUI.Terminal;

/// <summary>
/// High-level rendering system that coordinates buffer management, rendering, and scheduling.
/// </summary>
public class RenderingSystem : IRenderingSystem, IDisposable
{
    private readonly ITerminal _terminal;
    private readonly TerminalBuffer _buffer;
    private readonly IRenderer _renderer;
    private readonly RenderScheduler _scheduler;
    private bool _isInitialized;

    /// <summary>
    /// Gets the terminal buffer for drawing operations.
    /// </summary>
    public TerminalBuffer Buffer => _buffer;

    /// <summary>
    /// Gets the render scheduler for controlling rendering.
    /// </summary>
    public RenderScheduler Scheduler => _scheduler;

    /// <summary>
    /// Gets the underlying terminal.
    /// </summary>
    public ITerminal Terminal => _terminal;

    /// <summary>
    /// Gets the renderer being used.
    /// </summary>
    public IRenderer Renderer => _renderer;

    /// <summary>
    /// Gets the width of the terminal.
    /// </summary>
    public int Width => _terminal.Width;

    /// <summary>
    /// Gets the height of the terminal.
    /// </summary>
    public int Height => _terminal.Height;

    /// <summary>
    /// Creates a new rendering system.
    /// </summary>
    public RenderingSystem(ITerminal terminal, IRenderer? renderer = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _buffer = new TerminalBuffer(terminal.Width, terminal.Height);
        _renderer = renderer ?? new AnsiRenderer(terminal);
        _scheduler = new RenderScheduler(terminal, _renderer, _buffer);
        // Default to OnDemand; callers can set Fixed for constant refresh
        _scheduler.Mode = RenderMode.OnDemand;

        // Subscribe to terminal resize events
        _terminal.SizeChanged += OnTerminalSizeChanged;
    }

    /// <summary>
    /// Initializes the rendering system.
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
            return;

        // Enter alternate screen if supported
        _terminal.EnterAlternateScreen();

        // Clear the screen
        _terminal.Clear();
        _terminal.CursorVisible = false;

        // Start the render scheduler
        _scheduler.Start();

        _isInitialized = true;
    }

    /// <summary>
    /// Shuts down the rendering system.
    /// </summary>
    public void Shutdown()
    {
        if (!_isInitialized)
            return;

        // Stop the render scheduler
        _scheduler.Stop();

        // Show cursor
        _terminal.CursorVisible = true;

        // Exit alternate screen if we entered it
        _terminal.ExitAlternateScreen();

        _isInitialized = false;
    }

    /// <summary>
    /// Clears the entire screen.
    /// </summary>
    public void Clear()
    {
        _scheduler.QueueRender(() =>
        {
            _buffer.Clear();
        });
    }

    /// <summary>
    /// Writes text at the specified position.
    /// </summary>
    public void WriteText(int x, int y, string text, Style style = default)
    {
        _scheduler.QueueRender(() =>
        {
            _buffer.WriteText(x, y, text, style);
        });
    }

    /// <summary>
    /// Draws a box at the specified position.
    /// </summary>
    public void DrawBox(int x, int y, int width, int height, Style style = default, BoxStyle boxStyle = BoxStyle.Single)
    {
        _scheduler.QueueRender(() =>
        {
            var chars = GetBoxCharacters(boxStyle);

            // Draw corners
            _buffer.SetCell(x, y, chars.TopLeft, style);
            _buffer.SetCell(x + width - 1, y, chars.TopRight, style);
            _buffer.SetCell(x, y + height - 1, chars.BottomLeft, style);
            _buffer.SetCell(x + width - 1, y + height - 1, chars.BottomRight, style);

            // Draw horizontal lines
            for (int i = 1; i < width - 1; i++)
            {
                _buffer.SetCell(x + i, y, chars.Horizontal, style);
                _buffer.SetCell(x + i, y + height - 1, chars.Horizontal, style);
            }

            // Draw vertical lines
            for (int i = 1; i < height - 1; i++)
            {
                _buffer.SetCell(x, y + i, chars.Vertical, style);
                _buffer.SetCell(x + width - 1, y + i, chars.Vertical, style);
            }
        });
    }

    /// <summary>
    /// Fills a rectangle with the specified character and style.
    /// </summary>
    public void FillRect(int x, int y, int width, int height, char character = ' ', Style style = default)
    {
        _scheduler.QueueRender(() =>
        {
            _buffer.FillRect(x, y, width, height, new Cell(character, style));
        });
    }

    /// <summary>
    /// Forces an immediate render of all pending changes.
    /// </summary>
    public void Render()
    {
        _scheduler.ForceRender();
    }

    /// <summary>
    /// Handles terminal size changes.
    /// </summary>
    private void OnTerminalSizeChanged(object? sender, TerminalSizeChangedEventArgs e)
    {
        _scheduler.QueueRender(() =>
        {
            _buffer.Resize(e.Width, e.Height);
            _buffer.MarkAllDirty();
        });
    }

    /// <summary>
    /// Gets box drawing characters for the specified style.
    /// </summary>
    private BoxCharacters GetBoxCharacters(BoxStyle style)
    {
        return style switch
        {
            BoxStyle.Single => new BoxCharacters
            {
                TopLeft = '┌',
                TopRight = '┐',
                BottomLeft = '└',
                BottomRight = '┘',
                Horizontal = '─',
                Vertical = '│'
            },
            BoxStyle.Double => new BoxCharacters
            {
                TopLeft = '╔',
                TopRight = '╗',
                BottomLeft = '╚',
                BottomRight = '╝',
                Horizontal = '═',
                Vertical = '║'
            },
            BoxStyle.Rounded => new BoxCharacters
            {
                TopLeft = '╭',
                TopRight = '╮',
                BottomLeft = '╰',
                BottomRight = '╯',
                Horizontal = '─',
                Vertical = '│'
            },
            BoxStyle.Heavy => new BoxCharacters
            {
                TopLeft = '┏',
                TopRight = '┓',
                BottomLeft = '┗',
                BottomRight = '┛',
                Horizontal = '━',
                Vertical = '┃'
            },
            _ => new BoxCharacters
            {
                TopLeft = '+',
                TopRight = '+',
                BottomLeft = '+',
                BottomRight = '+',
                Horizontal = '-',
                Vertical = '|'
            }
        };
    }

    /// <summary>
    /// Disposes the rendering system.
    /// </summary>
    public void Dispose()
    {
        Shutdown();
        _terminal.SizeChanged -= OnTerminalSizeChanged;
        _scheduler.Dispose();
    }

    private struct BoxCharacters
    {
        public char TopLeft;
        public char TopRight;
        public char BottomLeft;
        public char BottomRight;
        public char Horizontal;
        public char Vertical;
    }
}

/// <summary>
/// Defines box drawing styles.
/// </summary>
public enum BoxStyle
{
    /// <summary>
    /// Single line box (┌─┐).
    /// </summary>
    Single,

    /// <summary>
    /// Double line box (╔═╗).
    /// </summary>
    Double,

    /// <summary>
    /// Rounded corners box (╭─╮).
    /// </summary>
    Rounded,

    /// <summary>
    /// Heavy line box (┏━┓).
    /// </summary>
    Heavy,

    /// <summary>
    /// ASCII box (+-+).
    /// </summary>
    Ascii
}