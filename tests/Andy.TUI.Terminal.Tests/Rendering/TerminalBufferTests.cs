using Xunit;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests.Rendering;

public class TerminalBufferTests
{
    [Fact]
    public void Constructor_CreatesBufferWithCorrectDimensions()
    {
        // Arrange & Act
        var buffer = new TerminalBuffer(80, 24);
        
        // Assert
        Assert.Equal(80, buffer.Width);
        Assert.Equal(24, buffer.Height);
    }
    
    [Fact]
    public void SetCell_MarksRegionAsDirty()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        
        // Act
        buffer.SetCell(5, 5, new Cell('A'));
        
        // Assert
        Assert.True(buffer.IsDirty);
    }
    
    [Fact]
    public void SetCell_WithCharAndStyle_SetsCorrectCell()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        var style = new Style { Foreground = Color.Red, Bold = true };
        
        // Act
        buffer.SetCell(3, 4, 'X', style);
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Single(regions);
        var region = regions[0];
        Assert.Equal(3, region.X);
        Assert.Equal(4, region.Y);
        Assert.Equal('X', region.NewCell.Character);
        Assert.Equal(style, region.NewCell.Style);
    }
    
    [Fact]
    public void WriteText_WritesTextAtPosition()
    {
        // Arrange
        var buffer = new TerminalBuffer(20, 10);
        
        // Act
        buffer.WriteText(5, 2, "Hello");
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(5, regions.Count);
        Assert.Equal('H', regions[0].NewCell.Character);
        Assert.Equal(5, regions[0].X);
        Assert.Equal('e', regions[1].NewCell.Character);
        Assert.Equal(6, regions[1].X);
    }
    
    [Fact]
    public void WriteText_HandlesNewlines()
    {
        // Arrange
        var buffer = new TerminalBuffer(20, 10);
        
        // Act
        buffer.WriteText(5, 2, "Line1\nLine2");
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        var line1Chars = regions.Where(r => r.Y == 2).ToList();
        var line2Chars = regions.Where(r => r.Y == 3).ToList();
        
        Assert.Equal(5, line1Chars.Count);
        Assert.Equal(5, line2Chars.Count);
        Assert.Equal('L', line2Chars[0].NewCell.Character);
        Assert.Equal(5, line2Chars[0].X); // Should reset to original X position
    }
    
    [Fact]
    public void WriteText_WrapsLongLines()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 5);
        
        // Act
        buffer.WriteText(8, 0, "Hello"); // Should wrap after "He"
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        var line0 = regions.Where(r => r.Y == 0).ToList();
        var line1 = regions.Where(r => r.Y == 1).ToList();
        
        Assert.Equal(2, line0.Count); // "He"
        Assert.Equal(3, line1.Count); // "llo"
    }
    
    [Fact]
    public void Clear_MarksEntireBufferAsDirty()
    {
        // Arrange
        var buffer = new TerminalBuffer(5, 5);
        buffer.SetCell(2, 2, 'X');
        buffer.SwapBuffers(); // Clear dirty regions
        
        // Act
        buffer.Clear();
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(25, regions.Count); // 5x5 = 25 cells
        Assert.All(regions, r => Assert.Equal(' ', r.NewCell.Character));
    }
    
    [Fact]
    public void ClearRect_ClearsSpecifiedRegion()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        // Fill entire buffer first
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                buffer.SetCell(x, y, 'X');
        buffer.SwapBuffers(); // Clear dirty regions
        
        // Act
        buffer.ClearRect(2, 2, 3, 3);
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(9, regions.Count); // 3x3 = 9 cells
        Assert.All(regions, r =>
        {
            Assert.True(r.X >= 2 && r.X < 5);
            Assert.True(r.Y >= 2 && r.Y < 5);
            Assert.Equal(' ', r.NewCell.Character);
        });
    }
    
    [Fact]
    public void FillRect_FillsSpecifiedRegion()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        var cell = new Cell('#', new Style { Foreground = Color.Blue });
        
        // Act
        buffer.FillRect(3, 3, 2, 2, cell);
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(4, regions.Count); // 2x2 = 4 cells
        Assert.All(regions, r =>
        {
            Assert.Equal('#', r.NewCell.Character);
            Assert.Equal(Color.Blue, r.NewCell.Style.Foreground);
        });
    }
    
    [Fact]
    public void SwapBuffers_ReturnsOnlyChangedCells()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        buffer.SetCell(5, 5, 'A');
        buffer.SwapBuffers(); // First swap
        
        // Act
        buffer.SetCell(5, 5, 'A'); // Same value
        buffer.SetCell(6, 6, 'B'); // Different cell
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Single(regions); // Only the changed cell
        Assert.Equal(6, regions[0].X);
        Assert.Equal(6, regions[0].Y);
        Assert.Equal('B', regions[0].NewCell.Character);
    }
    
    [Fact]
    public void SwapBuffers_ClearsDirtyRegions()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        buffer.SetCell(5, 5, 'A');
        
        // Act
        Assert.True(buffer.IsDirty);
        buffer.SwapBuffers();
        
        // Assert
        Assert.False(buffer.IsDirty);
    }
    
    [Fact]
    public void Resize_PreservesExistingContent()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        buffer.SetCell(5, 5, 'X');
        buffer.SwapBuffers();
        
        // Act
        buffer.Resize(20, 20);
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(400, regions.Count); // All cells marked dirty after resize
        var cellAt5x5 = regions.FirstOrDefault(r => r.X == 5 && r.Y == 5);
        Assert.Contains(regions, r => r.X == 5 && r.Y == 5);
        Assert.Equal('X', cellAt5x5.NewCell.Character);
    }
    
    [Fact]
    public void Resize_TruncatesContentWhenShrinking()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        buffer.SetCell(8, 8, 'X');
        buffer.SwapBuffers();
        
        // Act
        buffer.Resize(5, 5);
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(25, regions.Count); // 5x5 = 25
        Assert.All(regions, r =>
        {
            Assert.True(r.X < 5);
            Assert.True(r.Y < 5);
        });
    }
    
    [Fact]
    public void MarkAllDirty_MarksEveryCell()
    {
        // Arrange
        var buffer = new TerminalBuffer(3, 3);
        buffer.SwapBuffers(); // Clear any initial dirty state
        
        // Act
        buffer.MarkAllDirty();
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert
        Assert.Equal(9, regions.Count); // 3x3 = 9
    }
    
    [Fact]
    public void GetFrontBuffer_ReturnsCloneOfFrontBuffer()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 10);
        buffer.SetCell(5, 5, 'X');
        buffer.SwapBuffers();
        
        // Act
        var frontBuffer = buffer.GetFrontBuffer();
        
        // Assert
        Assert.Equal(10, frontBuffer.Width);
        Assert.Equal(10, frontBuffer.Height);
        Assert.Equal('X', frontBuffer[5, 5].Character);
    }
}