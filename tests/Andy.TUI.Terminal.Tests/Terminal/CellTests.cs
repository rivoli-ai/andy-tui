using Xunit;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests;

public class CellTests
{
    [Fact]
    public void Empty_HasSpaceAndDefaultStyle()
    {
        var cell = Cell.Empty;
        
        Assert.Equal(' ', cell.Character);
        Assert.Equal(Style.Default, cell.Style);
        Assert.False(cell.IsDirty);
    }
    
    [Fact]
    public void Constructor_SetsValuesCorrectly()
    {
        var style = Style.WithForeground(Color.Red);
        var cell = new Cell('A', style);
        
        Assert.Equal('A', cell.Character);
        Assert.Equal(style, cell.Style);
        Assert.True(cell.IsDirty);
    }
    
    [Fact]
    public void Constructor_WithDefaultStyle_Works()
    {
        var cell = new Cell('X');
        
        Assert.Equal('X', cell.Character);
        Assert.Equal(Style.Default, cell.Style);
        Assert.True(cell.IsDirty);
    }
    
    [Fact]
    public void AsClean_ReturnsCleanCell()
    {
        var cell = new Cell('A');
        var clean = cell.AsClean();
        
        Assert.Equal('A', clean.Character);
        Assert.Equal(cell.Style, clean.Style);
        Assert.False(clean.IsDirty);
    }
    
    [Fact]
    public void AsDirty_ReturnsDirtyCell()
    {
        var cell = Cell.Empty;
        var dirty = cell.AsDirty();
        
        Assert.Equal(' ', dirty.Character);
        Assert.Equal(Style.Default, dirty.Style);
        Assert.True(dirty.IsDirty);
    }
    
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var style = Style.WithBold();
        var cell1 = new Cell('A', style);
        var cell2 = new Cell('A', style);
        
        Assert.True(cell1.Equals(cell2));
        Assert.True(cell1 == cell2);
        Assert.False(cell1 != cell2);
    }
    
    [Fact]
    public void Equals_IgnoresDirtyFlag()
    {
        var cell1 = new Cell('A').AsClean();
        var cell2 = new Cell('A').AsDirty();
        
        Assert.True(cell1.Equals(cell2));
    }
    
    [Fact]
    public void Equals_WithDifferentCharacter_ReturnsFalse()
    {
        var cell1 = new Cell('A');
        var cell2 = new Cell('B');
        
        Assert.False(cell1.Equals(cell2));
        Assert.False(cell1 == cell2);
        Assert.True(cell1 != cell2);
    }
    
    [Fact]
    public void Equals_WithDifferentStyle_ReturnsFalse()
    {
        var cell1 = new Cell('A', Style.WithForeground(Color.Red));
        var cell2 = new Cell('A', Style.WithForeground(Color.Blue));
        
        Assert.False(cell1.Equals(cell2));
    }
    
    [Fact]
    public void Equals_WithBoxedValue_Works()
    {
        var cell1 = new Cell('A');
        object cell2 = new Cell('A');
        object cell3 = new Cell('B');
        
        Assert.True(cell1.Equals(cell2));
        Assert.False(cell1.Equals(cell3));
        Assert.False(cell1.Equals(null));
        Assert.False(cell1.Equals("not a cell"));
    }
    
    [Fact]
    public void GetHashCode_SameValues_ProduceSameHash()
    {
        var style = Style.WithItalic();
        var cell1 = new Cell('X', style);
        var cell2 = new Cell('X', style);
        
        Assert.Equal(cell1.GetHashCode(), cell2.GetHashCode());
    }
    
    [Fact]
    public void GetHashCode_IgnoresDirtyFlag()
    {
        var cell1 = new Cell('A').AsClean();
        var cell2 = new Cell('A').AsDirty();
        
        Assert.Equal(cell1.GetHashCode(), cell2.GetHashCode());
    }
    
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var cell1 = new Cell('A');
        var cell2 = new Cell('*', Style.WithForeground(Color.Red).WithBold());
        
        Assert.Contains("'A'", cell1.ToString());
        Assert.Contains("'*'", cell2.ToString());
        Assert.Contains("Style", cell2.ToString());
    }
}