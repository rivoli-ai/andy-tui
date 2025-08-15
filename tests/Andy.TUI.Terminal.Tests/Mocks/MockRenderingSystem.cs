using System.Collections.Generic;
using Andy.TUI.Terminal.Rendering;

namespace Andy.TUI.Terminal.Tests.Rendering;

/// <summary>
/// Mock implementation of IRenderingSystem for testing.
/// </summary>
public class MockRenderingSystem : IRenderingSystem
{
    private readonly List<TextWrite> _textWrites = new();
    private readonly List<FillRectCall> _fillRects = new();
    private readonly List<DrawBoxCall> _drawBoxes = new();

    public int Width { get; set; } = 80;
    public int Height { get; set; } = 24;

    public class TextWrite
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Text { get; set; } = "";
        public Style Style { get; set; }
    }

    public class FillRectCall
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public char Character { get; set; }
        public Style Style { get; set; }
    }

    public class DrawBoxCall
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Style Style { get; set; }
        public BoxStyle BoxStyle { get; set; }
    }

    public void Initialize()
    {
        // No-op for testing
    }

    public void BeginFrame()
    {
        // No-op for testing
    }

    public void EndFrame()
    {
        // No-op for testing
    }

    public void WriteText(int x, int y, string text, Style style)
    {
        _textWrites.Add(new TextWrite { X = x, Y = y, Text = text, Style = style });
    }

    public void FillRect(int x, int y, int width, int height, char character, Style style)
    {
        _fillRects.Add(new FillRectCall
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Character = character,
            Style = style
        });
    }

    public void DrawBox(int x, int y, int width, int height, Style style, BoxStyle boxStyle)
    {
        _drawBoxes.Add(new DrawBoxCall
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Style = style,
            BoxStyle = boxStyle
        });
    }

    public void SetClipRegion(int x, int y, int width, int height)
    {
        // No-op for tests; clipping behavior is validated indirectly via item ordering
    }

    public void ResetClipRegion()
    {
        // No-op for tests
    }

    public void Dispose()
    {
        // No-op for testing
    }

    // Test helper methods
    public List<TextWrite> GetTextWrites() => new(_textWrites);
    public List<FillRectCall> GetFillRects() => new(_fillRects);
    public List<DrawBoxCall> GetDrawBoxes() => new(_drawBoxes);

    public void Clear()
    {
        _textWrites.Clear();
        _fillRects.Clear();
        _drawBoxes.Clear();
    }
}