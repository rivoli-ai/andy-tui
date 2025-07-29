using Xunit;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

public class BufferTests
{
    [Fact]
    public void Constructor_WithInvalidDimensions_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Buffer(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Buffer(10, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Buffer(-1, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Buffer(10, -1));
    }
    
    [Fact]
    public void Constructor_CreatesBufferWithCorrectSize()
    {
        var buffer = new Buffer(80, 25);
        
        Assert.Equal(80, buffer.Width);
        Assert.Equal(25, buffer.Height);
    }
    
    [Fact]
    public void Constructor_InitializesWithEmptyCells()
    {
        var buffer = new Buffer(10, 10);
        
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                var cell = buffer[x, y];
                Assert.Equal(' ', cell.Character);
                Assert.Equal(Style.Default, cell.Style);
                Assert.True(cell.IsDirty);
            }
        }
    }
    
    [Fact]
    public void Indexer_Get_WithInvalidPosition_ThrowsException()
    {
        var buffer = new Buffer(10, 10);
        
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[-1, 0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[0, -1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[10, 0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[0, 10]);
    }
    
    [Fact]
    public void Indexer_Set_WithInvalidPosition_ThrowsException()
    {
        var buffer = new Buffer(10, 10);
        var cell = new Cell('A');
        
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[-1, 0] = cell);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[0, -1] = cell);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[10, 0] = cell);
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer[0, 10] = cell);
    }
    
    [Fact]
    public void Indexer_SetGet_WorksCorrectly()
    {
        var buffer = new Buffer(10, 10);
        var cell = new Cell('X', Style.WithForeground(Color.Red));
        
        buffer[5, 5] = cell;
        var retrieved = buffer[5, 5];
        
        Assert.Equal('X', retrieved.Character);
        Assert.Equal(Color.Red, retrieved.Style.Foreground);
        Assert.True(retrieved.IsDirty);
    }
    
    [Fact]
    public void Indexer_Set_MarksCellAsDirty()
    {
        var buffer = new Buffer(10, 10);
        var cleanCell = new Cell('A').AsClean();
        
        buffer[0, 0] = cleanCell;
        
        Assert.True(buffer[0, 0].IsDirty);
    }
    
    [Fact]
    public void IsInBounds_ReturnsCorrectValues()
    {
        var buffer = new Buffer(10, 5);
        
        Assert.True(buffer.IsInBounds(0, 0));
        Assert.True(buffer.IsInBounds(9, 4));
        Assert.True(buffer.IsInBounds(5, 2));
        
        Assert.False(buffer.IsInBounds(-1, 0));
        Assert.False(buffer.IsInBounds(0, -1));
        Assert.False(buffer.IsInBounds(10, 0));
        Assert.False(buffer.IsInBounds(0, 5));
    }
    
    [Fact]
    public void TrySetCell_WithValidPosition_ReturnsTrue()
    {
        var buffer = new Buffer(10, 10);
        var cell = new Cell('A');
        
        Assert.True(buffer.TrySetCell(5, 5, cell));
        Assert.Equal('A', buffer[5, 5].Character);
    }
    
    [Fact]
    public void TrySetCell_WithInvalidPosition_ReturnsFalse()
    {
        var buffer = new Buffer(10, 10);
        var cell = new Cell('A');
        
        Assert.False(buffer.TrySetCell(-1, 0, cell));
        Assert.False(buffer.TrySetCell(0, -1, cell));
        Assert.False(buffer.TrySetCell(10, 0, cell));
        Assert.False(buffer.TrySetCell(0, 10, cell));
    }
    
    [Fact]
    public void TryGetCell_WithValidPosition_ReturnsTrue()
    {
        var buffer = new Buffer(10, 10);
        buffer[5, 5] = new Cell('X');
        
        Assert.True(buffer.TryGetCell(5, 5, out var cell));
        Assert.Equal('X', cell.Character);
    }
    
    [Fact]
    public void TryGetCell_WithInvalidPosition_ReturnsFalse()
    {
        var buffer = new Buffer(10, 10);
        
        Assert.False(buffer.TryGetCell(-1, 0, out var cell));
        Assert.Equal(Cell.Empty, cell);
        
        Assert.False(buffer.TryGetCell(10, 10, out cell));
        Assert.Equal(Cell.Empty, cell);
    }
    
    [Fact]
    public void Clear_ResetsAllCells()
    {
        var buffer = new Buffer(5, 5);
        
        // Set some cells
        for (int i = 0; i < 5; i++)
        {
            buffer[i, i] = new Cell((char)('A' + i));
        }
        
        buffer.Clear();
        
        // Check all cells are empty
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                var cell = buffer[x, y];
                Assert.Equal(' ', cell.Character);
                Assert.Equal(Style.Default, cell.Style);
                Assert.True(cell.IsDirty);
            }
        }
    }
    
    [Fact]
    public void ClearRect_ClearsSpecifiedArea()
    {
        var buffer = new Buffer(10, 10);
        
        // Fill entire buffer with 'X'
        buffer.FillRect(0, 0, 10, 10, new Cell('X'));
        
        // Clear a rectangle
        buffer.ClearRect(2, 2, 4, 3);
        
        // Check cleared area
        for (int y = 2; y < 5; y++)
        {
            for (int x = 2; x < 6; x++)
            {
                Assert.Equal(' ', buffer[x, y].Character);
            }
        }
        
        // Check outside area still has 'X'
        Assert.Equal('X', buffer[0, 0].Character);
        Assert.Equal('X', buffer[9, 9].Character);
    }
    
    [Fact]
    public void ClearRect_ClipsToBounds()
    {
        var buffer = new Buffer(10, 10);
        buffer.FillRect(0, 0, 10, 10, new Cell('X'));
        
        // Clear rect that extends beyond bounds
        buffer.ClearRect(8, 8, 5, 5);
        
        // Only the area within bounds should be cleared
        Assert.Equal(' ', buffer[8, 8].Character);
        Assert.Equal(' ', buffer[9, 9].Character);
        Assert.Equal('X', buffer[7, 7].Character);
    }
    
    [Fact]
    public void FillRect_FillsSpecifiedArea()
    {
        var buffer = new Buffer(10, 10);
        var fillCell = new Cell('*', Style.WithForeground(Color.Red));
        
        buffer.FillRect(3, 3, 4, 2, fillCell);
        
        // Check filled area
        for (int y = 3; y < 5; y++)
        {
            for (int x = 3; x < 7; x++)
            {
                var cell = buffer[x, y];
                Assert.Equal('*', cell.Character);
                Assert.Equal(Color.Red, cell.Style.Foreground);
            }
        }
        
        // Check outside area is unchanged
        Assert.Equal(' ', buffer[2, 2].Character);
        Assert.Equal(' ', buffer[7, 5].Character);
    }
    
    [Fact]
    public void FillRect_ClipsToBounds()
    {
        var buffer = new Buffer(10, 10);
        var fillCell = new Cell('#');
        
        // Fill rect that extends beyond bounds
        buffer.FillRect(-2, -2, 5, 5, fillCell);
        
        // Only the area within bounds should be filled
        Assert.Equal('#', buffer[0, 0].Character);
        Assert.Equal('#', buffer[2, 2].Character);
        Assert.Equal(' ', buffer[3, 3].Character);
    }
    
    [Fact]
    public void MarkAllClean_CleansAllCells()
    {
        var buffer = new Buffer(5, 5);
        
        // Set some cells (which marks them dirty)
        buffer[0, 0] = new Cell('A');
        buffer[2, 2] = new Cell('B');
        
        buffer.MarkAllClean();
        
        // All cells should be clean
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.False(buffer[x, y].IsDirty);
            }
        }
    }
    
    [Fact]
    public void MarkAllDirty_DirtiesAllCells()
    {
        var buffer = new Buffer(5, 5);
        
        // Mark all clean first
        buffer.MarkAllClean();
        
        buffer.MarkAllDirty();
        
        // All cells should be dirty
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.True(buffer[x, y].IsDirty);
            }
        }
    }
    
    [Fact]
    public void CopyFrom_WithNull_ThrowsException()
    {
        var buffer = new Buffer(10, 10);
        
        Assert.Throws<ArgumentNullException>(() => buffer.CopyFrom(null!));
    }
    
    [Fact]
    public void CopyFrom_CopiesContent()
    {
        var source = new Buffer(10, 10);
        var dest = new Buffer(10, 10);
        
        // Set up source
        source[5, 5] = new Cell('A');
        source[7, 7] = new Cell('B', Style.WithBold());
        
        dest.CopyFrom(source);
        
        Assert.Equal('A', dest[5, 5].Character);
        Assert.Equal('B', dest[7, 7].Character);
        Assert.True(dest[7, 7].Style.Bold);
    }
    
    [Fact]
    public void CopyFrom_WithDifferentSizes_CopiesMinimumArea()
    {
        var source = new Buffer(5, 5);
        var dest = new Buffer(10, 10);
        
        // Fill source
        source.FillRect(0, 0, 5, 5, new Cell('S'));
        
        // Fill dest with different character
        dest.FillRect(0, 0, 10, 10, new Cell('D'));
        
        dest.CopyFrom(source);
        
        // Check copied area
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.Equal('S', dest[x, y].Character);
            }
        }
        
        // Check non-copied area
        Assert.Equal('D', dest[5, 0].Character);
        Assert.Equal('D', dest[0, 5].Character);
        Assert.Equal('D', dest[9, 9].Character);
    }
    
    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var original = new Buffer(5, 5);
        original[2, 2] = new Cell('X', Style.WithForeground(Color.Green));
        
        var clone = original.Clone();
        
        // Verify clone has same content
        Assert.Equal(original.Width, clone.Width);
        Assert.Equal(original.Height, clone.Height);
        Assert.Equal('X', clone[2, 2].Character);
        Assert.Equal(Color.Green, clone[2, 2].Style.Foreground);
        
        // Modify clone
        clone[2, 2] = new Cell('Y');
        
        // Original should be unchanged
        Assert.Equal('X', original[2, 2].Character);
    }
}