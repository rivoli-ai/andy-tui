namespace Andy.TUI.Terminal;

/// <summary>
/// Defines the interface for terminal operations.
/// </summary>
public interface ITerminal
{
    /// <summary>
    /// Gets the width of the terminal in columns.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the terminal in rows.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Gets or sets the cursor position.
    /// </summary>
    (int Column, int Row) CursorPosition { get; set; }

    /// <summary>
    /// Gets or sets whether the cursor is visible.
    /// </summary>
    bool CursorVisible { get; set; }

    /// <summary>
    /// Gets whether the terminal supports color.
    /// </summary>
    bool SupportsColor { get; }

    /// <summary>
    /// Gets whether the terminal supports ANSI escape sequences.
    /// </summary>
    bool SupportsAnsi { get; }

    /// <summary>
    /// Clears the entire terminal screen.
    /// </summary>
    void Clear();

    /// <summary>
    /// Clears from the cursor to the end of the line.
    /// </summary>
    void ClearLine();

    /// <summary>
    /// Moves the cursor to the specified position.
    /// </summary>
    /// <param name="column">The column (0-based).</param>
    /// <param name="row">The row (0-based).</param>
    void MoveCursor(int column, int row);

    /// <summary>
    /// Writes text at the current cursor position.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void Write(string text);

    /// <summary>
    /// Writes text at the current cursor position with a newline.
    /// </summary>
    /// <param name="text">The text to write.</param>
    void WriteLine(string text);

    /// <summary>
    /// Sets the foreground color.
    /// </summary>
    /// <param name="color">The color to set.</param>
    void SetForegroundColor(ConsoleColor color);

    /// <summary>
    /// Sets the background color.
    /// </summary>
    /// <param name="color">The color to set.</param>
    void SetBackgroundColor(ConsoleColor color);

    /// <summary>
    /// Resets colors to defaults.
    /// </summary>
    void ResetColors();

    /// <summary>
    /// Saves the current cursor position.
    /// </summary>
    void SaveCursorPosition();

    /// <summary>
    /// Restores the previously saved cursor position.
    /// </summary>
    void RestoreCursorPosition();

    /// <summary>
    /// Enters alternate screen buffer (if supported).
    /// </summary>
    void EnterAlternateScreen();

    /// <summary>
    /// Exits alternate screen buffer (if supported).
    /// </summary>
    void ExitAlternateScreen();

    /// <summary>
    /// Flushes any buffered output to the terminal.
    /// </summary>
    void Flush();

    /// <summary>
    /// Event raised when the terminal size changes.
    /// </summary>
    event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged;
}

/// <summary>
/// Event arguments for terminal size changes.
/// </summary>
public class TerminalSizeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new width in columns.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the new height in rows.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the previous width in columns.
    /// </summary>
    public int OldWidth { get; }

    /// <summary>
    /// Gets the previous height in rows.
    /// </summary>
    public int OldHeight { get; }

    public TerminalSizeChangedEventArgs(int width, int height, int oldWidth, int oldHeight)
    {
        Width = width;
        Height = height;
        OldWidth = oldWidth;
        OldHeight = oldHeight;
    }
}