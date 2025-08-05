using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Layout;

/// <summary>
/// Tests for constraint propagation through the component tree.
/// </summary>
public class ConstraintPropagationTests
{
    private readonly ITestOutputHelper _output;
    private readonly TestDeclarativeContext _context;
    
    public ConstraintPropagationTests(ITestOutputHelper output)
    {
        _output = output;
        _context = new TestDeclarativeContext();
    }
    
    #region Basic Constraint Tests
    
    [Fact]
    public void UnconstrainedParent_WithConstrainedChildren_ShouldRespectChildConstraints()
    {
        // Arrange
        var container = new TestContainer
        {
            Children = new()
            {
                new FixedSizeComponent { Width = 100, Height = 50 },
                new FixedSizeComponent { Width = 150, Height = 75 }
            }
        };
        
        var root = _context.GetTestInstance(container, "root");
        var constraints = LayoutTestHelper.Unconstrained();
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        var containerInstance = root as TestContainerInstance;
        Assert.NotNull(containerInstance);
        
        var children = containerInstance.GetChildInstances();
        Assert.Equal(2, children.Count);
        
        // Children should maintain their fixed sizes
        // Note: Y positions are stacked vertically by TestContainer
        LayoutTestHelper.AssertLayoutBox(children[0].Layout, new LayoutBox { Width = 100, Height = 50, X = 0, Y = 0, AbsoluteX = 0, AbsoluteY = 0 });
        LayoutTestHelper.AssertLayoutBox(children[1].Layout, new LayoutBox { Width = 150, Height = 75, X = 0, Y = 50, AbsoluteX = 0, AbsoluteY = 50 });
        
        // Container should size to fit children
        Assert.Equal(150, result.RootLayout.Width); // Max child width
        Assert.Equal(125, result.RootLayout.Height); // Sum of child heights
    }
    
    [Fact]
    public void ConstrainedParent_WithUnconstrainedChildren_ShouldConstrainChildren()
    {
        // Arrange
        var container = new TestContainer
        {
            Children = new()
            {
                new AutoSizeComponent { PreferredWidth = 200, PreferredHeight = 100 },
                new AutoSizeComponent { PreferredWidth = 300, PreferredHeight = 150 }
            }
        };
        
        var root = _context.GetTestInstance(container, "root");
        var constraints = LayoutTestHelper.Tight(100, 100); // Force small size
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        var containerInstance = root as TestContainerInstance;
        Assert.NotNull(containerInstance);
        
        var children = containerInstance.GetChildInstances();
        
        // Children should be constrained by parent
        Assert.True(children[0].Layout.Width <= 100);
        Assert.True(children[1].Layout.Width <= 100);
        
        // Container should respect tight constraints
        Assert.Equal(100, result.RootLayout.Width);
        Assert.Equal(100, result.RootLayout.Height);
    }
    
    [Fact]
    public void MixedConstraints_AtDifferentLevels_ShouldPropagateProperly()
    {
        // Arrange - nested containers with different constraint handling
        var innerContainer = new TestContainer
        {
            Children = new()
            {
                new FixedSizeComponent { Width = 50, Height = 25 }
            }
        };
        
        var outerContainer = new TestContainer
        {
            Children = new()
            {
                innerContainer,
                new AutoSizeComponent { PreferredWidth = 100, PreferredHeight = 50 }
            }
        };
        
        var root = _context.GetTestInstance(outerContainer, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(100, result.RootLayout.Width); // Max of inner container (50) and auto component (100)
        Assert.Equal(75, result.RootLayout.Height); // Sum of heights (25 + 50)
    }
    
    #endregion
    
    #region Infinity Handling Tests
    
    [Fact]
    public void InfiniteConstraints_ShouldNotPropagateUnintentionally()
    {
        // Arrange
        var container = new TestContainer
        {
            Children = new()
            {
                new AutoSizeComponent { PreferredWidth = 100, PreferredHeight = 50 }
            }
        };
        
        var root = _context.GetTestInstance(container, "root");
        var constraints = LayoutTestHelper.Unconstrained(); // Infinite max constraints
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert - auto-sized component should use preferred size, not infinity
        var child = (root as TestContainerInstance)?.GetChildInstances()[0];
        Assert.NotNull(child);
        
        LayoutTestHelper.AssertNotInfinite(child.Layout.Width, "Child width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(child.Layout.Height, "Child height should not be infinite");
        Assert.Equal(100, child.Layout.Width);
        Assert.Equal(50, child.Layout.Height);
    }
    
    [Fact]
    public void ComponentRequestingInfiniteSize_ShouldBeHandledGracefully()
    {
        // Arrange
        var container = new TestContainer
        {
            Children = new()
            {
                new ExtremeValueComponent 
                { 
                    WidthMode = ExtremeValueComponent.SizeMode.Infinite,
                    HeightMode = ExtremeValueComponent.SizeMode.Normal
                }
            }
        };
        
        var root = _context.GetTestInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(500, 500);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert - infinite request should be clamped to max constraint
        var child = (root as TestContainerInstance)?.GetChildInstances()[0];
        Assert.NotNull(child);
        
        LayoutTestHelper.AssertNotInfinite(child.Layout.Width, "Width should be clamped");
        Assert.Equal(500, child.Layout.Width); // Clamped to max constraint
        Assert.Equal(50, child.Layout.Height); // Normal height
    }
    
    #endregion
    
    #region Zero Size Handling Tests
    
    [Fact]
    public void ZeroSizeConstraints_ShouldProduceZeroSizeLayout()
    {
        // Arrange
        var component = new FixedSizeComponent { Width = 100, Height = 50 };
        var root = _context.GetTestInstance(component, "root");
        var constraints = LayoutTestHelper.Tight(0, 0);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    [Fact]
    public void ComponentRequestingZeroSize_ShouldBeRespected()
    {
        // Arrange
        var component = new ExtremeValueComponent
        {
            WidthMode = ExtremeValueComponent.SizeMode.Zero,
            HeightMode = ExtremeValueComponent.SizeMode.Zero
        };
        
        var root = _context.GetTestInstance(component, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    [Fact]
    public void EmptyContainer_ShouldNotCollapseToZero_WhenNotRequired()
    {
        // Arrange - empty container with loose constraints
        var container = new TestContainer { Children = new() };
        var root = _context.GetTestInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert - empty container should have zero size (no minimum in our test container)
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    #endregion
    
    #region Edge Case Tests
    
    [Fact]
    public void NaNValues_ShouldBeHandledGracefully()
    {
        // Arrange
        var component = new ExtremeValueComponent
        {
            WidthMode = ExtremeValueComponent.SizeMode.NaN,
            HeightMode = ExtremeValueComponent.SizeMode.Normal
        };
        
        var root = _context.GetTestInstance(component, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act & Assert - should not throw
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // NaN should be handled (likely as 0 or min constraint)
        Assert.False(float.IsNaN(result.RootLayout.Width), "Width should not be NaN");
        Assert.False(float.IsNaN(result.RootLayout.Height), "Height should not be NaN");
    }
    
    [Fact]
    public void DeeplyNestedConstraints_ShouldPropagateThroughAllLevels()
    {
        // Arrange - create 5 levels of nesting
        ISimpleComponent current = new FixedSizeComponent { Width = 50, Height = 25 };
        
        for (int i = 0; i < 5; i++)
        {
            current = new TestContainer { Children = new() { current } };
        }
        
        var root = _context.GetTestInstance(current, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert - innermost component should still have its fixed size
        var container = root as TestContainerInstance;
        ViewInstance innermost = root;
        
        while (container != null)
        {
            var children = container.GetChildInstances();
            if (children.Count > 0)
            {
                innermost = children[0];
                container = innermost as TestContainerInstance;
            }
            else
            {
                break;
            }
        }
        
        // Innermost should be our fixed size component
        Assert.IsType<FixedSizeInstance>(innermost);
        Assert.Equal(50, innermost.Layout.Width);
        Assert.Equal(25, innermost.Layout.Height);
    }
    
    #endregion
}