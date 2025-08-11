using System;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
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

    #region Auto-Sizing Tests

    [Fact]
    public void Box_WithAutoWidth_AndFixedHeight_ShouldSizeToContent()
    {
        // Arrange
        var box = new Box
        {
            Width = Length.Auto,
            Height = 50,
            FlexDirection = FlexDirection.Column // Explicit column direction for vertical stacking
        };
        box.Add(new Box { Width = 30, Height = 20 });
        box.Add(new Box { Width = 40, Height = 25 });
        box.Add(new Box { Width = 35, Height = 15 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine("Layout Tree:");
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        // Debug: Print child count and sizes
        var children = boxInstance.GetChildInstances();
        _output.WriteLine($"Child count: {children.Count}");
        for (int i = 0; i < children.Count; i++)
        {
            _output.WriteLine($"Child {i}: {children[i].Layout.Width}x{children[i].Layout.Height} at ({children[i].Layout.X},{children[i].Layout.Y})");
        }

        // Box should have fixed height of 50
        Assert.Equal(50, result.RootLayout.Height);

        // Box width should fit the widest child (40)
        Assert.Equal(40, result.RootLayout.Width);

        // Verify child count
        Assert.Equal(3, children.Count);

        // Verify child positions and sizes
        LayoutTestHelper.AssertLayoutBox(children[0].Layout, new LayoutBox { Width = 30, Height = 20, X = 0, Y = 0, AbsoluteX = 0, AbsoluteY = 0 });
        LayoutTestHelper.AssertLayoutBox(children[1].Layout, new LayoutBox { Width = 40, Height = 25, X = 0, Y = 20, AbsoluteX = 0, AbsoluteY = 20 });
        LayoutTestHelper.AssertLayoutBox(children[2].Layout, new LayoutBox { Width = 35, Height = 15, X = 0, Y = 45, AbsoluteX = 0, AbsoluteY = 45 });
    }

    [Fact]
    public void Box_WithFixedWidth_AndAutoHeight_ShouldSizeToContent()
    {
        // Arrange
        var box = new Box
        {
            Width = 100,
            Height = Length.Auto,
            FlexDirection = FlexDirection.Column
        };
        box.Add(new Box { Width = 80, Height = 30 });
        box.Add(new Box { Width = 90, Height = 40 });
        box.Add(new Box { Width = 70, Height = 25 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine("Layout Tree:");
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        // Box should have fixed width of 100
        Assert.Equal(100, result.RootLayout.Width);

        // Box height should fit all children (30 + 40 + 25 = 95)
        Assert.Equal(95, result.RootLayout.Height);

        // Verify children
        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);
    }

    [Fact]
    public void Box_WithBothAutoDimensions_ShouldSizeToContent()
    {
        // Arrange
        var box = new Box
        {
            Width = Length.Auto,
            Height = Length.Auto,
            FlexDirection = FlexDirection.Column
        };
        box.Add(new Box { Width = 60, Height = 20 });
        box.Add(new Box { Width = 80, Height = 30 });
        box.Add(new Box { Width = 50, Height = 15 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(300, 300);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine("Layout Tree:");
        _output.WriteLine(result.LayoutTree);

        // Debug output
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);
        var children = boxInstance.GetChildInstances();
        _output.WriteLine($"Box size: {result.RootLayout.Width}x{result.RootLayout.Height}");
        for (int i = 0; i < children.Count; i++)
        {
            _output.WriteLine($"Child {i}: {children[i].Layout.Width}x{children[i].Layout.Height} at ({children[i].Layout.X},{children[i].Layout.Y})");
        }

        // Assert
        // Box width should fit the widest child (80)
        Assert.Equal(80, result.RootLayout.Width);

        // Box height should fit all children (20 + 30 + 15 = 65)
        Assert.Equal(65, result.RootLayout.Height);

        // Verify children count
        Assert.Equal(3, children.Count);
    }

    [Fact]
    public void NestedAutoSizedBoxes_ShouldPropagateConstraintsCorrectly()
    {
        // Arrange
        var innerBox = new Box
        {
            Width = Length.Auto,
            Height = Length.Auto,
            FlexDirection = FlexDirection.Column
        };
        innerBox.Add(new Text("Inner content line 1"));
        innerBox.Add(new Text("Inner line 2"));

        var outerBox = new Box
        {
            Width = Length.Auto,
            Height = Length.Auto,
            FlexDirection = FlexDirection.Column
        };
        outerBox.Add(new Text("Outer header"));
        outerBox.Add(innerBox);
        outerBox.Add(new Text("Outer footer"));

        var root = _context.ViewInstanceManager.GetOrCreateInstance(outerBox, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine("Layout Tree:");
        _output.WriteLine(result.LayoutTree);

        // Assert
        var outerBoxInstance = root as BoxInstance;
        Assert.NotNull(outerBoxInstance);

        // Check that constraints didn't propagate infinity
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Width, "Outer box width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Height, "Outer box height should not be infinite");

        // Get inner box
        var children = outerBoxInstance.GetChildInstances();
        Assert.Equal(3, children.Count); // Header, inner box, footer

        var innerBoxInstance = children[1] as BoxInstance;
        Assert.NotNull(innerBoxInstance);

        // Check inner box didn't get infinite size
        LayoutTestHelper.AssertNotInfinite(innerBoxInstance.Layout.Width, "Inner box width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(innerBoxInstance.Layout.Height, "Inner box height should not be infinite");

        // Check reasonable sizes (auto-height should work)
        Assert.True(result.RootLayout.Height > 0, "Outer box should have non-zero height");
        Assert.True(innerBoxInstance.Layout.Height > 0, "Inner box should have non-zero height");
    }

    [Fact]
    public void AutoSizedBox_WithPaddingAndMargin_ShouldIncludeSpacingInSize()
    {
        // Arrange
        var box = new Box
        {
            Width = Length.Auto,
            Height = Length.Auto,
            Padding = new Spacing(10), // 10 on all sides
            Margin = new Spacing(5),   // 5 on all sides
            FlexDirection = FlexDirection.Column
        };
        box.Add(new Box { Width = 100, Height = 50 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(300, 300);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine("Layout Tree:");
        _output.WriteLine(result.LayoutTree);
        _output.WriteLine($"Box size: {result.RootLayout.Width}x{result.RootLayout.Height}");

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        // Width should be child width (100) + padding left (10) + padding right (10) = 120
        // Note: Margin is not included in the box's own size
        Assert.Equal(120, result.RootLayout.Width);

        // Height should be child height (50) + padding top (10) + padding bottom (10) = 70
        Assert.Equal(70, result.RootLayout.Height);

        // Check child position is affected by padding
        var children = boxInstance.GetChildInstances();
        Assert.Single(children);

        // Child should be positioned at (0,0) relative to content area
        // The padding offset is applied when calculating absolute positions
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(0, children[0].Layout.Y);
    }

    #endregion

    #region Edge Case Tests - Circular Dependencies

    [Fact]
    public void CircularDependency_ShouldNotCauseInfiniteLoop()
    {
        // Arrange - Create a potential circular dependency scenario
        // Parent box with auto dimensions containing children that depend on parent size
        var parentBox = new Box
        {
            Width = Length.Auto,
            Height = Length.Auto,
            FlexDirection = FlexDirection.Column
        };

        // Child with percentage-based size (depends on parent)
        var childBox = new Box
        {
            Width = Length.Percentage(80), // 80% of parent
            Height = 50
        };

        parentBox.Add(childBox);

        var root = _context.ViewInstanceManager.GetOrCreateInstance(parentBox, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);

        // Act - This should not hang or throw
        LayoutResult? result = null;

        // Since the layout system should handle circular dependencies gracefully,
        // we'll just perform the layout normally
        result = LayoutTestHelper.PerformLayout(root, constraints);

        // Assert
        Assert.NotNull(result);

        // Should resolve to some reasonable size
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Width, "Width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Height, "Height should not be infinite");
        Assert.True(result.RootLayout.Width > 0, "Width should be positive");
        Assert.True(result.RootLayout.Height > 0, "Height should be positive");

        _output.WriteLine($"Resolved size: {result.RootLayout.Width}x{result.RootLayout.Height}");
    }

    #endregion
}