using FluentAssertions;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using static Andy.TUI.Components.Tests.TestHelpers;

namespace Andy.TUI.Components.Tests.Layout;

public class GridTests
{
    [Fact]
    public void GridLength_Factories_CreateCorrectTypes()
    {
        var absolute = GridLength.Absolute(50);
        absolute.Type.Should().Be(GridLength.GridUnitType.Absolute);
        absolute.Value.Should().Be(50);
        
        var auto = GridLength.Auto;
        auto.Type.Should().Be(GridLength.GridUnitType.Auto);
        
        var star = GridLength.Star(2);
        star.Type.Should().Be(GridLength.GridUnitType.Star);
        star.Value.Should().Be(2);
    }
    
    [Fact]
    public void Grid_SetColumns_CreatesColumnDefinitions()
    {
        var grid = new Grid();
        
        grid.SetColumns(
            GridLength.Absolute(100),
            GridLength.Star(1),
            GridLength.Star(2),
            GridLength.Auto
        );
        
        grid.Columns.Should().HaveCount(4);
        grid.Columns[0].Width.Type.Should().Be(GridLength.GridUnitType.Absolute);
        grid.Columns[1].Width.Type.Should().Be(GridLength.GridUnitType.Star);
        grid.Columns[2].Width.Type.Should().Be(GridLength.GridUnitType.Star);
        grid.Columns[3].Width.Type.Should().Be(GridLength.GridUnitType.Auto);
    }
    
    [Fact]
    public void Grid_SetRows_CreatesRowDefinitions()
    {
        var grid = new Grid();
        
        grid.SetRows(
            GridLength.Absolute(50),
            GridLength.Star(1)
        );
        
        grid.Rows.Should().HaveCount(2);
        grid.Rows[0].Height.Type.Should().Be(GridLength.GridUnitType.Absolute);
        grid.Rows[1].Height.Type.Should().Be(GridLength.GridUnitType.Star);
    }
    
    [Fact]
    public void Grid_AddChild_PlacesChildCorrectly()
    {
        var grid = new Grid();
        var child = new TextNode("Cell");
        
        grid.AddChild(child, row: 1, column: 2);
        
        grid.Children.Should().HaveCount(1);
        grid.Children[0].Row.Should().Be(1);
        grid.Children[0].Column.Should().Be(2);
        grid.Children[0].Node.Should().Be(child);
    }
    
    [Fact]
    public void Grid_AddChild_WithSpan()
    {
        var grid = new Grid();
        var child = new TextNode("Spanning Cell");
        
        grid.AddChild(child, row: 0, column: 0, rowSpan: 2, columnSpan: 3);
        
        grid.Children[0].RowSpan.Should().Be(2);
        grid.Children[0].ColumnSpan.Should().Be(3);
    }
    
    [Fact]
    public void Grid_MeasureCore_WithAbsoluteSizes()
    {
        var grid = new Grid
        {
            RowGap = 5,
            ColumnGap = 10
        };
        
        grid.SetColumns(GridLength.Absolute(100), GridLength.Absolute(150));
        grid.SetRows(GridLength.Absolute(50), GridLength.Absolute(75));
        
        var size = grid.Measure(new Size(1000, 1000));
        
        // Width: 100 + 10 + 150 = 260
        // Height: 50 + 5 + 75 = 130
        size.Width.Should().Be(260);
        size.Height.Should().Be(130);
    }
    
    [Fact]
    public void Grid_MeasureCore_WithStarSizes()
    {
        var grid = new Grid();
        
        grid.SetColumns(GridLength.Star(1), GridLength.Star(2));
        grid.SetRows(GridLength.Star(1), GridLength.Star(1));
        
        var availableSize = new Size(300, 200);
        var size = grid.Measure(availableSize);
        
        // Should use available space
        size.Width.Should().Be(300);
        size.Height.Should().Be(200);
    }
    
    [Fact]
    public void Grid_WithGaps_IncludesInCalculation()
    {
        var grid = new Grid
        {
            RowGap = 10,
            ColumnGap = 20
        };
        
        grid.SetColumns(GridLength.Absolute(100), GridLength.Absolute(100), GridLength.Absolute(100));
        grid.SetRows(GridLength.Absolute(50), GridLength.Absolute(50));
        
        var size = grid.Measure(new Size(1000, 1000));
        
        // Width: 100 + 20 + 100 + 20 + 100 = 340
        // Height: 50 + 10 + 50 = 110
        size.Width.Should().Be(340);
        size.Height.Should().Be(110);
    }
    
    [Fact]
    public void Grid_Render_CreatesGridElement()
    {
        var grid = new Grid();
        grid.SetColumns(GridLength.Star(1), GridLength.Star(1));
        grid.SetRows(GridLength.Star(1));
        
        grid.AddChild(new TextNode("Cell 1"), 0, 0);
        grid.AddChild(new TextNode("Cell 2"), 0, 1);
        
        grid.Initialize(new MockComponentContext());
        var result = grid.Render();
        
        result.Should().BeOfType<ElementNode>();
        var element = (ElementNode)result;
        element.TagName.Should().Be("grid");
        element.Props["rows"].Should().Be(1);
        element.Props["columns"].Should().Be(2);
        
        // Should have grid-item children
        var children = element.Children.ToList();
        children.Should().HaveCount(2);
        children.Should().AllBeOfType<ElementNode>();
        children.Cast<ElementNode>().Should().AllSatisfy(child => 
            child.TagName.Should().Be("grid-item"));
    }
    
    [Fact]
    public void Grid_EmptyGrid_MeasuresWithPadding()
    {
        var grid = new Grid
        {
            Padding = new Spacing(10)
        };
        
        var size = grid.Measure(new Size(100, 100));
        
        size.Width.Should().Be(20);
        size.Height.Should().Be(20);
    }
    
}