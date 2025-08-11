namespace Andy.TUI.Terminal;

/// <summary>
/// Defines the interface for rendering content to a terminal.
/// </summary>
public interface IRenderer
{
    /// <summary>
    /// Gets the terminal used for rendering.
    /// </summary>
    ITerminal Terminal { get; }

    /// <summary>
    /// Gets the width of the render area.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the render area.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Begins a new frame for rendering.
    /// </summary>
    void BeginFrame();

    /// <summary>
    /// Ends the current frame and flushes changes to the terminal.
    /// </summary>
    void EndFrame();

    /// <summary>
    /// Draws text at the specified position with optional style.
    /// </summary>
    /// <param name="x">The column position.</param>
    /// <param name="y">The row position.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="style">The style to apply.</param>
    void DrawText(int x, int y, string text, Style style = default);

    /// <summary>
    /// Draws a single character at the specified position with optional style.
    /// </summary>
    /// <param name="x">The column position.</param>
    /// <param name="y">The row position.</param>
    /// <param name="ch">The character to draw.</param>
    /// <param name="style">The style to apply.</param>
    void DrawChar(int x, int y, char ch, Style style = default);

    /// <summary>
    /// Fills a rectangular area with a character and style.
    /// </summary>
    /// <param name="x">The starting column.</param>
    /// <param name="y">The starting row.</param>
    /// <param name="width">The width of the area.</param>
    /// <param name="height">The height of the area.</param>
    /// <param name="ch">The character to fill with.</param>
    /// <param name="style">The style to apply.</param>
    void FillRect(int x, int y, int width, int height, char ch, Style style = default);

    /// <summary>
    /// Clears the entire render area.
    /// </summary>
    void Clear();

    /// <summary>
    /// Clears a rectangular area.
    /// </summary>
    /// <param name="x">The starting column.</param>
    /// <param name="y">The starting row.</param>
    /// <param name="width">The width of the area.</param>
    /// <param name="height">The height of the area.</param>
    void ClearRect(int x, int y, int width, int height);

    /// <summary>
    /// Sets a clipping region for subsequent drawing operations.
    /// </summary>
    /// <param name="x">The starting column.</param>
    /// <param name="y">The starting row.</param>
    /// <param name="width">The width of the clip region.</param>
    /// <param name="height">The height of the clip region.</param>
    void SetClipRegion(int x, int y, int width, int height);

    /// <summary>
    /// Resets the clipping region to the full render area.
    /// </summary>
    void ResetClipRegion();
}