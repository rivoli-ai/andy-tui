using System;
using System.Linq;
using Xunit;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.ViewInstances;

namespace Andy.TUI.Declarative.Tests;

public class LayoutComponentTests
{
    [Fact]
    public void Box_CreatesBoxInstance()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var box = new Box {
            new Text("Hello"),
            new Text("World")
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(box, "box1");
        
        // Assert
        Assert.IsType<BoxInstance>(instance);
    }
    
    [Fact]
    public void Box_AppliesFlexProperties()
    {
        // Arrange
        var box = new Box()
            .Direction(FlexDirection.Row)
            .Justify(JustifyContent.Center)
            .Align(AlignItems.Center)
            .Grow(1)
            .Shrink(2)
            .Basis(100);
        
        // Assert
        Assert.Equal(FlexDirection.Row, box.FlexDirection);
        Assert.Equal(JustifyContent.Center, box.JustifyContent);
        Assert.Equal(AlignItems.Center, box.AlignItems);
        Assert.Equal(1, box.FlexGrow);
        Assert.Equal(2, box.FlexShrink);
        Assert.Equal(100, box.FlexBasis.Value);
    }
    
    [Fact]
    public void Box_AppliesSizeProperties()
    {
        // Arrange
        var box = new Box()
            .WithWidth(100)
            .WithHeight(50)
            .WithMinWidth(80)
            .WithMaxWidth(120)
            .WithPadding(new Spacing(10))
            .WithMargin(new Spacing(5));
        
        // Assert
        Assert.Equal(100, box.Width.Value);
        Assert.Equal(50, box.Height.Value);
        Assert.Equal(80, box.MinWidth.Value);
        Assert.Equal(120, box.MaxWidth.Value);
        Assert.Equal(10, box.Padding.Top.Value);
        Assert.Equal(5, box.Margin.Top.Value);
    }
    
    [Fact]
    public void HStack_PositionsChildrenHorizontally()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var hstack = new HStack(spacing: 10) {
            new Text("Item1"),
            new Text("Item2"),
            new Text("Item3")
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(hstack, "hstack1") as HStackInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(100, 50));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert - children should be positioned horizontally
        Assert.Equal(3, children.Count);
        Assert.True(children[0].Layout.X < children[1].Layout.X);
        Assert.True(children[1].Layout.X < children[2].Layout.X);
        Assert.Equal(children[0].Layout.Y, children[1].Layout.Y); // Same Y position
    }
    
    [Fact]
    public void VStack_PositionsChildrenVertically()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var vstack = new VStack(spacing: 5) {
            new Text("Item1"),
            new Text("Item2"),
            new Text("Item3")
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(vstack, "vstack1") as VStackInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(100, 50));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert - children should be positioned vertically
        Assert.Equal(3, children.Count);
        Assert.True(children[0].Layout.Y < children[1].Layout.Y);
        Assert.True(children[1].Layout.Y < children[2].Layout.Y);
        Assert.Equal(children[0].Layout.X, children[1].Layout.X); // Same X position
    }
    
    [Fact]
    public void ZStack_OverlaysChildren()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var zstack = new ZStack(AlignItems.Center) {
            new Box { new Text("Background") }.WithWidth(100).WithHeight(50),
            new Box { new Text("Foreground") }.WithWidth(50).WithHeight(25)
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(zstack, "zstack1") as ZStackInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(200, 100));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert - children should be overlaid with centering
        Assert.Equal(2, children.Count);
        Assert.Equal(100, instance.Layout.Width); // Takes size of largest child
        Assert.Equal(50, instance.Layout.Height);
        
        // Second child should be centered within the first
        var bg = children[0].Layout;
        var fg = children[1].Layout;
        Assert.Equal((bg.Width - fg.Width) / 2, fg.X, 1);
        Assert.Equal((bg.Height - fg.Height) / 2, fg.Y, 1);
    }
    
    [Fact]
    public void Grid_ArrangesChildrenInGrid()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var grid = new Grid()
            .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(2))
            .WithRows(GridTrackSize.Auto, GridTrackSize.Auto)
            .WithGap(10);
        grid.Add(new Text("Cell 1"));
        grid.Add(new Text("Cell 2"));
        grid.Add(new Text("Cell 3"));
        grid.Add(new Text("Cell 4"));
        
        // Act
        var instance = manager.GetOrCreateInstance(grid, "grid1") as GridInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(300, 200));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert - children should be in grid formation
        Assert.Equal(4, children.Count);
        
        // First row
        Assert.Equal(children[0].Layout.Y, children[1].Layout.Y);
        Assert.True(children[0].Layout.X < children[1].Layout.X);
        
        // Second row
        Assert.Equal(children[2].Layout.Y, children[3].Layout.Y);
        Assert.True(children[2].Layout.X < children[3].Layout.X);
        
        // Columns align
        Assert.Equal(children[0].Layout.X, children[2].Layout.X);
        Assert.Equal(children[1].Layout.X, children[3].Layout.X);
        
        // Rows are spaced
        Assert.True(children[2].Layout.Y > children[0].Layout.Y);
    }
    
    [Fact]
    public void Grid_HandlesSpannedCells()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var grid = new Grid()
            .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1))
            .WithRows(GridTrackSize.Auto, GridTrackSize.Auto);
        grid.Add(new Text("Header").GridArea(1, 1, columnSpan: 3));
        grid.Add(new Text("Cell 1").GridArea(2, 1));
        grid.Add(new Text("Cell 2").GridArea(2, 2));
        grid.Add(new Text("Cell 3").GridArea(2, 3));
        
        // Act
        var instance = manager.GetOrCreateInstance(grid, "grid1") as GridInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(300, 200));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert
        Assert.Equal(4, children.Count);
        
        // Header spans all columns
        var header = children[0];
        var cell1 = children[1];
        var cell3 = children[3];
        
        Assert.Equal(cell1.Layout.X, header.Layout.X); // Header starts at first column
        Assert.True(header.Layout.Width > cell1.Layout.Width); // Header is wider
    }
    
    [Fact]
    public void Spacer_ExpandsToFillSpace()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var hstack = new HStack {
            new Text("Start"),
            new Spacer(),
            new Text("End")
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(hstack, "hstack1") as HStackInstance;
        Assert.NotNull(instance);
        
        // Calculate layout with fixed width
        instance.CalculateLayout(LayoutConstraints.Tight(200, 50));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert - spacer should push items apart
        Assert.Equal(3, children.Count);
        var start = children[0];
        var spacer = children[1];
        var end = children[2];
        
        // Start should be at the beginning
        Assert.Equal(0, start.Layout.X);
        
        // End should be at the far right
        Assert.True(end.Layout.X + end.Layout.Width >= 195); // Allow small margin
        
        // Spacer should fill the gap
        Assert.True(spacer.Layout.Width > 0);
        Assert.True(spacer.Layout.X > start.Layout.X + start.Layout.Width);
        Assert.True(spacer.Layout.X + spacer.Layout.Width <= end.Layout.X);
    }
    
    [Fact]
    public void Spacer_WithMinLength()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var vstack = new VStack {
            new Text("Top"),
            new Spacer(minLength: 20),
            new Text("Bottom")
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(vstack, "vstack1") as VStackInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert - spacer should have at least minimum length
        Assert.Equal(3, children.Count);
        var spacer = children[1];
        Assert.True(spacer.Layout.Height >= 20);
    }
    
    [Fact]
    public void Box_OverflowHidden_ClipsChildren()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var box = new Box {
            new Text("This is a very long text that should be clipped")
        }
        .WithWidth(20)
        .WithHeight(1)
        .WithOverflow(Overflow.Hidden);
        
        // Act
        var instance = manager.GetOrCreateInstance(box, "box1") as BoxInstance;
        Assert.NotNull(instance);
        
        // Calculate layout
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Render
        var rendered = instance.Render();
        
        // Assert - should create a clipping node
        if (rendered is FragmentNode fragment && fragment.Children.Count > 0)
        {
            var hasClippingNode = fragment.Children.Any(child => child is ClippingNode);
            Assert.True(hasClippingNode, "Box with overflow:hidden should create a ClippingNode");
        }
    }
    
    [Fact]
    public void FlexShrink_ShrinksItemsWhenOverflowing()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        var hstack = new HStack {
            new Box { new Text("Item1") }.WithMinWidth(50).Shrink(1),
            new Box { new Text("Item2") }.WithMinWidth(50).Shrink(2),
            new Box { new Text("Item3") }.WithMinWidth(50).Shrink(0)
        };
        
        // Act
        var instance = manager.GetOrCreateInstance(hstack, "hstack1") as HStackInstance;
        Assert.NotNull(instance);
        
        // Calculate layout with constrained width (less than total min widths)
        instance.CalculateLayout(LayoutConstraints.Tight(100, 50));
        
        var children = instance.GetChildInstances().ToList();
        
        // Assert
        Assert.Equal(3, children.Count);
        
        // Item3 should keep its minimum width (shrink: 0)
        Assert.Equal(50, children[2].Layout.Width, 1);
        
        // Item2 should shrink more than Item1 (shrink: 2 vs 1)
        Assert.True(children[1].Layout.Width < children[0].Layout.Width);
        
        // Total width should fit constraint
        var totalWidth = children.Sum(c => c.Layout.Width);
        Assert.True(totalWidth <= 100);
    }
}