using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Tests.TestHelpers;
using System.Linq;

namespace Andy.TUI.Declarative.Tests.Layout;

/// <summary>
/// Tests for Grid component layout behavior.
/// </summary>
public class GridLayoutTests
{
    private readonly ITestOutputHelper _output;
    private readonly DeclarativeContext _context;
    
    public GridLayoutTests(ITestOutputHelper output)
    {
        _output = output;
        _context = new DeclarativeContext(() => { });
    }
    
    #region Basic Grid Layout Tests
    
    [Fact]
    public void Grid_WithFixedColumns_ShouldDistributeEvenly()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1));
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(300, 200);
        
        // Act
        root.CalculateLayout(constraints);
        root.Render();
        
        // Assert
        Assert.Equal(150, root.Layout.Width); // 3 columns * 50
        Assert.Equal(60, root.Layout.Height); // 2 rows * 30
        
        // Verify positions
        var gridInstance = root as GridInstance;
        Assert.NotNull(gridInstance);
        var children = gridInstance.GetChildInstances();
        
        // First row
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(0, children[0].Layout.Y);
        Assert.Equal(50, children[1].Layout.X);
        Assert.Equal(0, children[1].Layout.Y);
        Assert.Equal(100, children[2].Layout.X);
        Assert.Equal(0, children[2].Layout.Y);
        
        // Second row
        Assert.Equal(0, children[3].Layout.X);
        Assert.Equal(30, children[3].Layout.Y);
        Assert.Equal(50, children[4].Layout.X);
        Assert.Equal(30, children[4].Layout.Y);
        Assert.Equal(100, children[5].Layout.X);
        Assert.Equal(30, children[5].Layout.Y);
    }
    
    [Fact]
    public void Grid_WithVariableSizeChildren_ShouldUseMaxDimensions()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Auto, GridTrackSize.Auto);
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 80, Height = 40 });
        grid.Add(new Box { Width = 60, Height = 50 });
        grid.Add(new Box { Width = 70, Height = 35 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        // Grid should use max column width and max row height
        Assert.Equal(140, result.RootLayout.Width); // max(50,60) + max(80,70) = 60 + 80
        Assert.Equal(90, result.RootLayout.Height); // max(30,40) + max(50,35) = 40 + 50
    }
    
    #endregion
    
    #region Grid with Spacing Tests
    
    [Fact]
    public void Grid_WithColumnGap_ShouldAddHorizontalSpacing()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Pixels(50), GridTrackSize.Pixels(50), GridTrackSize.Pixels(50))
            .WithColumnGap(10);
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(170, result.RootLayout.Width); // 50*3 + 10*2 = 170
        Assert.Equal(30, result.RootLayout.Height);
        
        // Verify child positions include gap
        var gridInstance = root as GridInstance;
        var children = gridInstance!.GetChildInstances();
        
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(60, children[1].Layout.X);   // 50 + 10
        Assert.Equal(120, children[2].Layout.X);  // 50 + 10 + 50 + 10
    }
    
    [Fact]
    public void Grid_WithRowGap_ShouldAddVerticalSpacing()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Pixels(50), GridTrackSize.Pixels(50))
            .WithRowGap(15);
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(100, result.RootLayout.Width);
        Assert.Equal(75, result.RootLayout.Height); // 30*2 + 15 = 75
        
        // Verify row positions include gap
        var gridInstance = root as GridInstance;
        var children = gridInstance!.GetChildInstances();
        
        Assert.Equal(0, children[0].Layout.Y);
        Assert.Equal(0, children[1].Layout.Y);
        Assert.Equal(45, children[2].Layout.Y);  // 30 + 15
        Assert.Equal(45, children[3].Layout.Y);
    }
    
    [Fact]
    public void Grid_WithBothGaps_ShouldAddBothSpacings()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Pixels(40), GridTrackSize.Pixels(40))
            .WithColumnGap(10)
            .WithRowGap(20);
        grid.Add(new Box { Width = 40, Height = 30 });
        grid.Add(new Box { Width = 40, Height = 30 });
        grid.Add(new Box { Width = 40, Height = 30 });
        grid.Add(new Box { Width = 40, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(90, result.RootLayout.Width);   // 40*2 + 10 = 90
        Assert.Equal(80, result.RootLayout.Height);  // 30*2 + 20 = 80
    }
    
    #endregion
    
    #region Grid with Different Column Counts
    
    [Fact]
    public void Grid_WithSingleColumn_ShouldBehaveLikeVStack()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Fr(1));
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 60, Height = 40 });
        grid.Add(new Box { Width = 55, Height = 35 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(60, result.RootLayout.Width);   // Max width
        Assert.Equal(105, result.RootLayout.Height); // Sum of heights
    }
    
    [Fact]
    public void Grid_WithPartialLastRow_ShouldHandleCorrectly()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Pixels(50), GridTrackSize.Pixels(50), GridTrackSize.Pixels(50));
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });  // Only 2 items in last row
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(150, result.RootLayout.Width);  // Still 3 columns wide
        Assert.Equal(60, result.RootLayout.Height);  // 2 rows
    }
    
    #endregion
    
    #region Constraint Propagation Tests
    
    [Fact]
    public void Grid_WithTightConstraints_ShouldConstrainChildren()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1));
        grid.Add(new Box { Width = 100, Height = 100 });
        grid.Add(new Box { Width = 100, Height = 100 });
        grid.Add(new Box { Width = 100, Height = 100 });
        grid.Add(new Box { Width = 100, Height = 100 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Tight(120, 80);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(120, result.RootLayout.Width);
        Assert.Equal(80, result.RootLayout.Height);
        
        // Children should be constrained
        var gridInstance = root as GridInstance;
        var children = gridInstance!.GetChildInstances();
        
        // Each child should fit within allocated cell space
        foreach (var child in children)
        {
            Assert.True(child.Layout.Width <= 60);  // 120/2 = 60 per column
            Assert.True(child.Layout.Height <= 40); // 80/2 = 40 per row
        }
    }
    
    [Fact]
    public void Grid_WithInfiniteConstraints_ShouldNotProduceInfiniteSize()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Auto, GridTrackSize.Auto);
        grid.Add(new Text("Cell 1"));
        grid.Add(new Text("Cell 2"));
        grid.Add(new Text("Cell 3"));
        grid.Add(new Text("Cell 4"));
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Unconstrained();
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Width, "Width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Height, "Height should not be infinite");
        LayoutTestHelper.AssertReasonableSize(result.RootLayout.Width, 0, 1000);
        LayoutTestHelper.AssertReasonableSize(result.RootLayout.Height, 0, 100);
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact]
    public void Grid_WithZeroColumns_ShouldHandleGracefully()
    {
        // Arrange
        var grid = new Grid(); // No columns specified
        grid.Add(new Box { Width = 50, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act & Assert - should not throw
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Grid should handle gracefully (treat as 1 column or return 0 size)
        Assert.True(result.RootLayout.Width >= 0);
        Assert.True(result.RootLayout.Height >= 0);
    }
    
    [Fact]
    public void EmptyGrid_ShouldHaveZeroSize()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1));
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    [Fact]
    public void Grid_WithNegativeGaps_ShouldHandleGracefully()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(GridTrackSize.Pixels(50), GridTrackSize.Pixels(50))
            .WithColumnGap(-10)
            .WithRowGap(-20);
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        grid.Add(new Box { Width = 50, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act & Assert - should not throw
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Grid should handle negative gaps (possibly as overlaps or treat as 0)
        Assert.True(result.RootLayout.Width > 0);
        Assert.True(result.RootLayout.Height > 0);
    }
    
    [Fact]
    public void Grid_WithManyColumns_FewChildren_ShouldNotCrash()
    {
        // Arrange
        var grid = new Grid();
        grid.WithColumns(
            GridTrackSize.Pixels(30), GridTrackSize.Pixels(30), GridTrackSize.Pixels(30),
            GridTrackSize.Pixels(30), GridTrackSize.Pixels(30), GridTrackSize.Pixels(30),
            GridTrackSize.Pixels(30), GridTrackSize.Pixels(30), GridTrackSize.Pixels(30),
            GridTrackSize.Pixels(30)
        );
        grid.Add(new Box { Width = 30, Height = 20 });
        grid.Add(new Box { Width = 30, Height = 20 });  // Only 2 children
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(grid, "root");
        var constraints = LayoutTestHelper.Loose(500, 500);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        // Should only size to actual content, not all 10 columns
        Assert.Equal(60, result.RootLayout.Width); // 2 * 30
        Assert.Equal(20, result.RootLayout.Height); // Single row
    }
    
    #endregion
}

/// <summary>
/// Extension methods to help with Grid testing.
/// </summary>
internal static class GridTestExtensions
{
    public static IReadOnlyList<ViewInstance> GetChildInstances(this GridInstance grid)
    {
        var childrenField = typeof(GridInstance).GetField("_childInstances", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (childrenField?.GetValue(grid) as IReadOnlyList<ViewInstance>) ?? new List<ViewInstance>();
    }
}