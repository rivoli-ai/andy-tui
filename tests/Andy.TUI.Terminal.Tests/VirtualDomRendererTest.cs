using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Terminal.Tests;

/// <summary>
/// Tests for VirtualDomRenderer to ensure proper clearing and rendering.
/// </summary>
public class VirtualDomRendererTest
{
    private readonly ITestOutputHelper _output;
    
    public VirtualDomRendererTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void Render_ShouldClearScreen_BeforeRendering()
    {
        // Arrange
        var terminal = new TestTerminal(40, 10);
        var renderingSystem = new RenderingSystem(terminal);
        var renderer = new VirtualDomRenderer(renderingSystem);
        renderingSystem.Initialize();
        
        // Pre-fill terminal with garbage to ensure clearing works
        for (int y = 0; y < 10; y++)
        {
            terminal.MoveCursor(0, y);
            terminal.Write("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
        }
        
        // Act - Render a simple tree
        var tree = new ElementNode("fragment");
        var text = new ElementNode("text");
        text.Props["x"] = 5;
        text.Props["y"] = 2;
        text.AddChild(new TextNode("Hello"));
        tree.AddChild(text);
        
        renderer.Render(tree);
        renderingSystem.Render(); // Flush
        
        // Assert - Check that areas not written to are cleared
        _output.WriteLine("Terminal after render:");
        for (int y = 0; y < 5; y++)
        {
            var line = terminal.GetLine(y);
            _output.WriteLine($"Line {y}: |{line}|");
        }
        
        // Line 0 should be empty (spaces)
        Assert.Equal("", terminal.GetLine(0).TrimEnd());
        
        // Line 2 should have "Hello" at position 5
        var line2 = terminal.GetLine(2);
        Assert.Equal("     Hello", line2.TrimEnd());
        Assert.StartsWith("     Hello", line2); // Hello at position 5
    }
    
    [Fact]
    public void MultipleRenders_ShouldClearPreviousContent()
    {
        // Arrange
        var terminal = new TestTerminal(40, 10);
        var renderingSystem = new RenderingSystem(terminal);
        var renderer = new VirtualDomRenderer(renderingSystem);
        renderingSystem.Initialize();
        
        // Act - First render with long text
        var tree1 = new ElementNode("fragment");
        var text1 = new ElementNode("text");
        text1.Props["x"] = 0;
        text1.Props["y"] = 0;
        text1.AddChild(new TextNode("This is a very long text"));
        tree1.AddChild(text1);
        
        renderer.Render(tree1);
        renderingSystem.Render();
        
        _output.WriteLine("After first render:");
        _output.WriteLine($"Line 0: |{terminal.GetLine(0)}|");
        
        // Second render with short text
        var tree2 = new ElementNode("fragment");
        var text2 = new ElementNode("text");
        text2.Props["x"] = 0;
        text2.Props["y"] = 0;
        text2.AddChild(new TextNode("Short"));
        tree2.AddChild(text2);
        
        renderer.Render(tree2);
        renderingSystem.Render();
        
        // Assert
        _output.WriteLine("After second render:");
        _output.WriteLine($"Line 0: |{terminal.GetLine(0)}|");
        
        var line = terminal.GetLine(0);
        // Should only show "Short", not "Short is a very long text"
        Assert.Equal("Short", line.TrimEnd());
    }
    
    [Fact]
    public void ApplyPatches_UpdateProps_ShouldClearOldPosition()
    {
        // Arrange
        var terminal = new TestTerminal(40, 10);
        var renderingSystem = new RenderingSystem(terminal);
        var renderer = new VirtualDomRenderer(renderingSystem);
        var diffEngine = new DiffEngine();
        renderingSystem.Initialize();
        
        // Initial render
        var tree1 = new ElementNode("fragment");
        var text1 = new ElementNode("text");
        text1.Props["x"] = 0;
        text1.Props["y"] = 0;
        text1.AddChild(new TextNode("Moving text"));
        tree1.AddChild(text1);
        
        renderer.Render(tree1);
        renderingSystem.Render();
        
        _output.WriteLine("Initial render:");
        _output.WriteLine($"Line 0: |{terminal.GetLine(0)}|");
        _output.WriteLine($"Line 1: |{terminal.GetLine(1)}|");
        
        // Move text to different position via patches
        var tree2 = new ElementNode("fragment");
        var text2 = new ElementNode("text");
        text2.Props["x"] = 5;
        text2.Props["y"] = 1;
        text2.AddChild(new TextNode("Moving text"));
        tree2.AddChild(text2);
        
        var patches = diffEngine.Diff(tree1, tree2);
        _output.WriteLine($"Generated {patches.Count} patches");
        
        renderer.ApplyPatches(patches);
        renderingSystem.Render();
        
        // Assert
        _output.WriteLine("After applying patches:");
        _output.WriteLine($"Line 0: |{terminal.GetLine(0)}|");
        _output.WriteLine($"Line 1: |{terminal.GetLine(1)}|");
        
        // Line 0 should be cleared
        Assert.Equal("", terminal.GetLine(0).TrimEnd());
        
        // Line 1 should have text at position 5
        var line1 = terminal.GetLine(1);
        Assert.Equal("     Moving text", line1.TrimEnd());
    }
    
    private class TestTerminal : ITerminal
    {
        private readonly char[,] _buffer;
        private int _cursorX, _cursorY;
        
        public TestTerminal(int width, int height)
        {
            Width = width;
            Height = height;
            _buffer = new char[height, width];
            Clear();
        }
        
        public int Width { get; }
        public int Height { get; }
        public bool CursorVisible { get; set; } = true;
        public (int Column, int Row) CursorPosition 
        { 
            get => (_cursorX, _cursorY);
            set { _cursorX = value.Column; _cursorY = value.Row; }
        }
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
            _cursorX = 0;
            _cursorY = 0;
        }
        
        public void ClearLine()
        {
            if (_cursorY < Height)
                for (int x = _cursorX; x < Width; x++)
                    _buffer[_cursorY, x] = ' ';
        }
        
        public void MoveCursor(int column, int row)
        {
            _cursorX = Math.Max(0, Math.Min(column, Width - 1));
            _cursorY = Math.Max(0, Math.Min(row, Height - 1));
        }
        
        public void Write(string text)
        {
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    _cursorY++;
                    _cursorX = 0;
                }
                else if (_cursorX < Width && _cursorY < Height)
                {
                    _buffer[_cursorY, _cursorX] = c;
                    _cursorX++;
                    if (_cursorX >= Width)
                    {
                        _cursorX = 0;
                        _cursorY++;
                    }
                }
            }
        }
        
        public void WriteLine(string text)
        {
            Write(text);
            Write("\n");
        }
        
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