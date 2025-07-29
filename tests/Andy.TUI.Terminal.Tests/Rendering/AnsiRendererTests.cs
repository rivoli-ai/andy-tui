using Xunit;
using Moq;
using Andy.TUI.Terminal;
using System.Text;

namespace Andy.TUI.Terminal.Tests.Rendering;

public class AnsiRendererTests
{
    private readonly Mock<ITerminal> _terminalMock;
    private readonly AnsiRenderer _renderer;
    private readonly StringBuilder _output;
    
    public AnsiRendererTests()
    {
        _terminalMock = new Mock<ITerminal>();
        _output = new StringBuilder();
        _terminalMock.Setup(t => t.Write(It.IsAny<string>()))
            .Callback<string>(s => _output.Append(s));
        _terminalMock.Setup(t => t.SupportsColor).Returns(true);
        _terminalMock.Setup(t => t.SupportsAnsi).Returns(true);
        
        _renderer = new AnsiRenderer(_terminalMock.Object);
    }
    
    [Fact]
    public void BeginFrame_ClearsInternalState()
    {
        // Act
        _renderer.BeginFrame();
        _renderer.EndFrame();
        
        // Assert
        _terminalMock.Verify(t => t.Flush(), Times.Once);
    }
    
    [Fact]
    public void RenderCell_MovesToCorrectPosition()
    {
        // Arrange
        var cell = new Cell('A');
        
        // Act
        _renderer.BeginFrame();
        _renderer.DrawChar(5, 3, cell.Character, cell.Style);
        _renderer.EndFrame();
        
        // Assert
        Assert.Contains("\x1b[4;6H", _output.ToString()); // ANSI is 1-based
        Assert.Contains("A", _output.ToString());
    }
    
    [Fact]
    public void RenderCell_AppliesBasicColors()
    {
        // Arrange
        var cell = new Cell('X', new Style 
        { 
            Foreground = Color.Red,
            Background = Color.Blue
        });
        
        // Act
        _renderer.BeginFrame();
        _renderer.DrawChar(0, 0, cell.Character, cell.Style);
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("\x1b[0m", output); // Reset
        Assert.Contains("\x1b[31m", output); // Red foreground
        Assert.Contains("\x1b[44m", output); // Blue background
    }
    
    [Fact]
    public void RenderCell_AppliesTextDecorations()
    {
        // Arrange
        var cell = new Cell('B', new Style 
        { 
            Bold = true,
            Italic = true,
            Underline = true
        });
        
        // Act
        _renderer.BeginFrame();
        _renderer.DrawChar(0, 0, cell.Character, cell.Style);
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("\x1b[1m", output); // Bold
        Assert.Contains("\x1b[3m", output); // Italic
        Assert.Contains("\x1b[4m", output); // Underline
    }
    
    [Fact]
    public void RenderCell_AppliesAllDecorations()
    {
        // Arrange
        var cell = new Cell('D', new Style 
        { 
            Bold = true,
            Italic = true,
            Underline = true,
            Strikethrough = true,
            Dim = true,
            Blink = true,
            Inverse = true
        });
        
        // Act
        _renderer.BeginFrame();
        _renderer.DrawChar(0, 0, cell.Character, cell.Style);
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("\x1b[1m", output); // Bold
        Assert.Contains("\x1b[3m", output); // Italic
        Assert.Contains("\x1b[4m", output); // Underline
        Assert.Contains("\x1b[9m", output); // Strikethrough
        Assert.Contains("\x1b[2m", output); // Dim
        Assert.Contains("\x1b[5m", output); // Blink
        Assert.Contains("\x1b[7m", output); // Inverse
    }
    
    [Fact]
    public void RenderCell_HandlesBrightColors()
    {
        // Arrange
        var cell = new Cell('B', new Style 
        { 
            Foreground = Color.BrightRed,
            Background = Color.BrightCyan
        });
        
        // Act
        _renderer.BeginFrame();
        _renderer.DrawChar(0, 0, cell.Character, cell.Style);
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("\x1b[91m", output); // Bright red foreground
        Assert.Contains("\x1b[106m", output); // Bright cyan background
    }
    
    [Fact]
    public void RenderCells_OptimizesConsecutiveCells()
    {
        // Arrange
        var style = new Style { Foreground = Color.Green };
        var regions = new[]
        {
            new DirtyRegion(0, 0, Cell.Empty, new Cell('H', style)),
            new DirtyRegion(1, 0, Cell.Empty, new Cell('e', style)),
            new DirtyRegion(2, 0, Cell.Empty, new Cell('l', style)),
            new DirtyRegion(3, 0, Cell.Empty, new Cell('l', style)),
            new DirtyRegion(4, 0, Cell.Empty, new Cell('o', style))
        };
        
        // Act
        _renderer.BeginFrame();
        foreach (var region in regions)
        {
            _renderer.DrawChar(region.X, region.Y, region.NewCell.Character, region.NewCell.Style);
        }
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("Hello", output);
        // Should only move cursor once for consecutive cells
        var cursorMoves = output.Split("\x1b[").Count(s => s.StartsWith("1;"));
        Assert.Equal(2, cursorMoves); // One in the position string, one for initial position
    }
    
    [Fact]
    public void Clear_SendsClearScreenSequence()
    {
        // Act
        _renderer.BeginFrame();
        _renderer.Clear();
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("\x1b[2J", output); // Clear screen
        Assert.Contains("\x1b[H", output); // Home cursor
    }
    
    [Fact]
    public void HideCursor_SendsHideCursorSequence()
    {
        // Act
        _renderer.BeginFrame();
        _renderer.HideCursor();
        _renderer.EndFrame();
        
        // Assert
        Assert.Contains("\x1b[?25l", _output.ToString());
    }
    
    [Fact]
    public void ShowCursor_SendsShowCursorSequence()
    {
        // Act
        _renderer.BeginFrame();
        _renderer.ShowCursor();
        _renderer.EndFrame();
        
        // Assert
        Assert.Contains("\x1b[?25h", _output.ToString());
    }
    
    [Fact]
    public void RenderCell_SkipsUnchangedStyle()
    {
        // Arrange
        var style = new Style { Foreground = Color.Yellow };
        var cell1 = new Cell('A', style);
        var cell2 = new Cell('B', style);
        
        // Act
        _renderer.BeginFrame();
        _renderer.DrawChar(0, 0, cell1.Character, cell1.Style);
        _renderer.DrawChar(1, 0, cell2.Character, cell2.Style);
        _renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        // Should only set the style once
        var yellowCount = output.Split("\x1b[33m").Length - 1;
        Assert.Equal(1, yellowCount);
    }
    
    [Fact]
    public void Constructor_DetectsTrueColorSupport()
    {
        // Arrange
        Environment.SetEnvironmentVariable("COLORTERM", "truecolor");
        
        // Act
        var renderer = new AnsiRenderer(_terminalMock.Object);
        
        // Assert
        Assert.True(renderer.SupportsTrueColor);
        Assert.True(renderer.Supports256Colors);
        
        // Cleanup
        Environment.SetEnvironmentVariable("COLORTERM", null);
    }
    
    [Fact]
    public void RenderCell_HandlesRgbColors_WithTrueColorSupport()
    {
        // Arrange
        Environment.SetEnvironmentVariable("COLORTERM", "truecolor");
        var renderer = new AnsiRenderer(_terminalMock.Object);
        var cell = new Cell('R', new Style 
        { 
            Foreground = Color.FromRgb(255, 128, 64),
            Background = Color.FromRgb(32, 64, 128)
        });
        
        // Act
        renderer.BeginFrame();
        renderer.DrawChar(0, 0, cell.Character, cell.Style);
        renderer.EndFrame();
        
        var output = _output.ToString();
        
        // Assert
        Assert.Contains("\x1b[38;2;255;128;64m", output); // True color foreground
        Assert.Contains("\x1b[48;2;32;64;128m", output); // True color background
        
        // Cleanup
        Environment.SetEnvironmentVariable("COLORTERM", null);
    }
    
    [Fact]
    public void EndFrame_OnlyFlushesWhenBufferHasContent()
    {
        // Act
        _renderer.BeginFrame();
        _renderer.EndFrame();
        
        // Assert
        _terminalMock.Verify(t => t.Write(It.IsAny<string>()), Times.Never);
        _terminalMock.Verify(t => t.Flush(), Times.Once);
    }
}