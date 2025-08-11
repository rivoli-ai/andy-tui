using System;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

/// <summary>
/// Tests to verify TerminalBuffer rendering behavior and text overwriting.
/// </summary>
public class TerminalBufferRenderingTest
{
    private readonly ITestOutputHelper _output;

    public TerminalBufferRenderingTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WriteText_ShouldOverwriteExistingText_NotClearFirst()
    {
        // Arrange
        var buffer = new TerminalBuffer(40, 10);

        // Act - Write long text first
        buffer.WriteText(0, 0, "This is a long text", Style.Default);
        buffer.SwapBuffers(); // Commit to front buffer

        // Write shorter text at same position
        buffer.WriteText(0, 0, "Short", Style.Default);
        var changes = buffer.SwapBuffers();

        // Assert - Check what's in the buffer
        var line = GetLineFromBuffer(buffer, 0);
        _output.WriteLine($"Line after overwrite: '{line}'");

        // The line should be "Shortis a long text" because WriteText doesn't clear
        // "This is a long text" -> positions: T h i s ' ' i s ' ' a ...
        // "Short" overwrites positions 0-4: S h o r t
        // Result: "Shortis a long text" (position 5 'i' is preserved)
        Assert.Equal("Shortis a long text", line.TrimEnd());
    }

    [Fact]
    public void FillRect_ThenWriteText_ShouldShowOnlyNewText()
    {
        // Arrange
        var buffer = new TerminalBuffer(40, 10);

        // Act - Write long text first
        buffer.WriteText(0, 0, "This is a long text", Style.Default);
        buffer.SwapBuffers();

        // Clear the area first, then write new text
        for (int x = 0; x < 40; x++)
            buffer.SetCell(x, 0, ' ', Style.Default);
        buffer.WriteText(0, 0, "Short", Style.Default);
        buffer.SwapBuffers();

        // Assert
        var line = GetLineFromBuffer(buffer, 0);
        _output.WriteLine($"Line after clear and write: '{line}'");

        // Should only show "Short" with trailing spaces
        Assert.Equal("Short", line.TrimEnd());
    }

    [Fact]
    public void MultipleWrites_AtSamePosition_ShouldOverlap()
    {
        // Arrange
        var buffer = new TerminalBuffer(40, 10);

        // Act - Write multiple texts at overlapping positions
        buffer.WriteText(0, 0, "AAAAAAA", Style.Default);
        buffer.WriteText(2, 0, "BBB", Style.Default);
        buffer.WriteText(1, 0, "CCC", Style.Default);
        buffer.SwapBuffers();

        // Assert
        var line = GetLineFromBuffer(buffer, 0);
        _output.WriteLine($"Overlapped text: '{line}'");

        // Result should be "ACCCBAA" - last write wins for each position
        Assert.Equal("ACCCBAA", line.TrimEnd());
    }

    [Fact]
    public void WriteText_WithNewlines_ShouldRenderOnMultipleLines()
    {
        // Arrange
        var buffer = new TerminalBuffer(40, 10);

        // Act
        buffer.WriteText(0, 0, "Line1\nLine2\nLine3", Style.Default);
        buffer.SwapBuffers();

        // Assert
        var line0 = GetLineFromBuffer(buffer, 0);
        var line1 = GetLineFromBuffer(buffer, 1);
        var line2 = GetLineFromBuffer(buffer, 2);

        _output.WriteLine($"Line 0: '{line0}'");
        _output.WriteLine($"Line 1: '{line1}'");
        _output.WriteLine($"Line 2: '{line2}'");

        Assert.Equal("Line1", line0.TrimEnd());
        Assert.Equal("Line2", line1.TrimEnd());
        Assert.Equal("Line3", line2.TrimEnd());
    }

    private string GetLineFromBuffer(TerminalBuffer buffer, int y)
    {
        var frontBuffer = buffer.GetFrontBuffer();
        var line = "";
        for (int x = 0; x < buffer.Width; x++)
        {
            if (frontBuffer.TryGetCell(x, y, out var cell))
            {
                line += cell.Character;
            }
            else
            {
                line += ' ';
            }
        }
        return line;
    }

    private class MockTerminal : ITerminal
    {
        private readonly char[,] _buffer;

        public MockTerminal(int width, int height)
        {
            Width = width;
            Height = height;
            _buffer = new char[height, width];
            Clear();
        }

        public int Width { get; }
        public int Height { get; }
        public bool CursorVisible { get; set; }
        public (int Column, int Row) CursorPosition { get; set; }
        public bool SupportsColor => true;
        public bool SupportsAnsi => true;

#pragma warning disable CS0067
        public event EventHandler<TerminalSizeChangedEventArgs>? SizeChanged;
#pragma warning restore CS0067

        public void Clear()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _buffer[y, x] = ' ';
        }

        public void ClearLine() { }
        public void MoveCursor(int column, int row) => CursorPosition = (column, row);
        public void Write(string text) { }
        public void WriteLine(string text) { }
        public void SetForegroundColor(ConsoleColor color) { }
        public void SetBackgroundColor(ConsoleColor color) { }
        public void ResetColors() { }
        public void SaveCursorPosition() { }
        public void RestoreCursorPosition() { }
        public void EnterAlternateScreen() { }
        public void ExitAlternateScreen() { }
        public void Flush() { }

        public string GetLine(int y)
        {
            if (y < 0 || y >= Height) return "";
            var line = "";
            for (int x = 0; x < Width; x++)
                line += _buffer[y, x];
            return line;
        }
    }
}