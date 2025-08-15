namespace Andy.TUI.Terminal;

/// <summary>
/// Interface for rendering operations to the terminal.
/// </summary>
public interface IRenderingSystem
{
    /// <summary>
    /// Gets the width of the terminal.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the terminal.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Writes text at the specified position.
    /// </summary>
    void WriteText(int x, int y, string text, Style style);

    /// <summary>
    /// Draws a box at the specified position.
    /// </summary>
    void DrawBox(int x, int y, int width, int height, Style style, BoxStyle boxStyle);

    /// <summary>
    /// Fills a rectangle with the specified character and style.
    /// </summary>
    void FillRect(int x, int y, int width, int height, char character, Style style);

    // Clipping API used by display list rasterization
    void SetClipRegion(int x, int y, int width, int height);
    void ResetClipRegion();
}