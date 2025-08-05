using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Layout;

/// <summary>
/// Tests for Box component layout behavior.
/// </summary>
public class BoxLayoutTests
{
    private readonly ITestOutputHelper _output;
    private readonly DeclarativeContext _context;
    
    public BoxLayoutTests(ITestOutputHelper output)
    {
        _output = output;
        _context = new DeclarativeContext(() => { });
    }
    
    #region Fixed Size Tests
    
    [Fact]
    public void Box_WithFixedSize_ShouldRespectDimensions()
    {
        // Arrange
        var box = new Box { Width = 200, Height = 100 };
        box.Add(new Text("Content"));
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(400, 300);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(200, result.RootLayout.Width);
        Assert.Equal(100, result.RootLayout.Height);
    }
    
    [Fact]
    public void Box_WithFixedSize_ExceedingConstraints_ShouldBeConstrained()
    {
        // Arrange
        var box = new Box { Width = 500, Height = 400 };
        box.Add(new Text("Content"));
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Tight(300, 200);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(300, result.RootLayout.Width);
        Assert.Equal(200, result.RootLayout.Height);
    }
    
    #endregion
    
    #region Auto Size Tests
    
    [Fact]
    public void Box_WithAutoHeight_ShouldSizeToContent()
    {
        // Arrange
        var box = new Box { Width = 200, Height = Length.Auto };
        box.Add(new Box { Width = 150, Height = 50 });
        box.Add(new Box { Width = 100, Height = 75 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(200, result.RootLayout.Width);
        // In row layout (default), height should be max of children heights
        Assert.Equal(75, result.RootLayout.Height);
    }
    
    [Fact]
    public void Box_WithAutoWidth_ShouldSizeToContent()
    {
        // Arrange
        var box = new Box { Width = Length.Auto, Height = 100 };
        box.Add(new Box { Width = 250, Height = 50 });
        box.Add(new Box { Width = 150, Height = 30 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(500, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        // In row layout (default), width should be sum of children widths
        Assert.Equal(400, result.RootLayout.Width); // 250 + 150
        Assert.Equal(100, result.RootLayout.Height);
    }
    
    [Fact]
    public void Box_WithAutoBothDimensions_ShouldSizeToContent()
    {
        // Arrange
        var box = new Box { Width = Length.Auto, Height = Length.Auto };
        box.Add(new Box { Width = 150, Height = 60 });
        box.Add(new Box { Width = 200, Height = 40 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Unconstrained();
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        // In row layout (default), width is sum and height is max
        Assert.Equal(350, result.RootLayout.Width); // Sum of child widths
        Assert.Equal(60, result.RootLayout.Height); // Max child height
    }
    
    #endregion
    
    #region Padding Tests
    
    [Fact]
    public void Box_WithPadding_ShouldInflateSize()
    {
        // Arrange
        var box = new Box 
        { 
            Width = Length.Auto, 
            Height = Length.Auto,
            Padding = new Spacing(20, 10, 20, 10) // top, right, bottom, left
        };
        box.Add(new Box { Width = 100, Height = 50 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(120, result.RootLayout.Width); // 100 + 10 + 10
        Assert.Equal(90, result.RootLayout.Height); // 50 + 20 + 20
    }
    
    [Fact]
    public void Box_WithPadding_ShouldPositionChildrenCorrectly()
    {
        // Arrange
        var box = new Box 
        { 
            Width = 200, 
            Height = 150,
            Padding = new Spacing(10, 15, 10, 15) // top, right, bottom, left
        };
        box.Add(new Box { Width = 100, Height = 50 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        root.CalculateLayout(constraints);
        root.Layout.AbsoluteX = 0;
        root.Layout.AbsoluteY = 0;
        root.Render();
        
        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);
        
        var children = boxInstance.GetChildInstances();
        Assert.Single(children);
        
        // Child should be positioned at (0,0) relative to content area
        // The padding offset is handled when calculating absolute positions
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(0, children[0].Layout.Y);
        
        // Absolute position should include padding
        Assert.Equal(15, children[0].Layout.AbsoluteX); // left padding
        Assert.Equal(10, children[0].Layout.AbsoluteY); // top padding
    }
    
    #endregion
    
    #region Nested Box Tests
    
    [Fact]
    public void NestedBoxes_WithMixedSizing_ShouldLayoutCorrectly()
    {
        // Arrange
        var outerBox = new Box 
        { 
            Width = 300, 
            Height = Length.Auto,
            Padding = new Spacing(10)
        };
        var innerBox = new Box 
        { 
            Width = Length.Auto, 
            Height = 100,
            Padding = new Spacing(5)
        };
        innerBox.Add(new Box { Width = 200, Height = 50 });
        outerBox.Add(innerBox);
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(outerBox, "root");
        var constraints = LayoutTestHelper.Loose(500, 500);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(300, result.RootLayout.Width);
        Assert.Equal(120, result.RootLayout.Height); // 100 (inner box) + 20 (outer padding)
    }
    
    [Fact]
    public void DeeplyNestedBoxes_ShouldPropagateConstraints()
    {
        // Arrange - 3 levels of boxes
        var innermost = new Box { Width = Length.Auto, Height = Length.Auto };
        innermost.Add(new Text("Deep"));
        
        var middle = new Box { Width = Length.Auto, Height = Length.Auto, Padding = new Spacing(5) };
        middle.Add(innermost);
        
        var outer = new Box { Width = 200, Height = 150, Padding = new Spacing(10) };
        outer.Add(middle);
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(outer, "root");
        var constraints = LayoutTestHelper.Loose(500, 500);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(200, result.RootLayout.Width);
        Assert.Equal(150, result.RootLayout.Height);
    }
    
    #endregion
    
    #region Edge Case Tests
    
    [Fact]
    public void Box_WithZeroPadding_ShouldNotAffectLayout()
    {
        // Arrange
        var box = new Box 
        { 
            Width = Length.Auto, 
            Height = Length.Auto,
            Padding = new Spacing(0)
        };
        box.Add(new Box { Width = 100, Height = 50 });
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(100, result.RootLayout.Width);
        Assert.Equal(50, result.RootLayout.Height);
    }
    
    [Fact]
    public void Box_EmptyWithAutoSize_ShouldHaveZeroSize()
    {
        // Arrange
        var box = new Box { Width = Length.Auto, Height = Length.Auto };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    [Fact]
    public void Box_WithInfiniteConstraints_AndAutoSize_ShouldNotBeInfinite()
    {
        // Arrange
        var box = new Box { Width = Length.Auto, Height = Length.Auto };
        box.Add(new Text("Some content"));
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Unconstrained();
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Width, "Width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Height, "Height should not be infinite");
        LayoutTestHelper.AssertReasonableSize(result.RootLayout.Width, 0, 1000, "Width");
        LayoutTestHelper.AssertReasonableSize(result.RootLayout.Height, 0, 1000, "Height");
    }
    
    #endregion
}

/// <summary>
/// Extension methods to help with Box testing.
/// </summary>
internal static class BoxTestExtensions
{
    public static ViewInstance? GetContentInstance(this BoxInstance box)
    {
        // Box should have exactly one child which is the content wrapper
        var childField = typeof(BoxInstance).GetField("_contentInstance", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return childField?.GetValue(box) as ViewInstance;
    }
}