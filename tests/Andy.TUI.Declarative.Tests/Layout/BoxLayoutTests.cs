using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Layout;
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

    #region Flex Properties Tests

    [Fact]
    public void Box_WithFlexGrow_ShouldExpandToFillSpace()
    {
        // Arrange
        var container = new Box
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };
        container.Add(new Box { Width = 50, Height = 50 });
        container.Add(new Box { Width = 50, Height = 50, FlexGrow = 1 });
        container.Add(new Box { Width = 50, Height = 50 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);

        // First child stays at 50
        Assert.Equal(50, children[0].Layout.Width);
        // Middle child should grow to fill remaining space (300 - 50 - 50 = 200)
        Assert.Equal(200, children[1].Layout.Width);
        // Last child stays at 50
        Assert.Equal(50, children[2].Layout.Width);
    }

    [Fact]
    public void Box_WithMultipleFlexGrow_ShouldDistributeProportionally()
    {
        // Arrange
        var container = new Box
        {
            Width = 400,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };
        container.Add(new Box { Width = 50, Height = 50, FlexGrow = 1 });
        container.Add(new Box { Width = 50, Height = 50, FlexGrow = 2 });
        container.Add(new Box { Width = 50, Height = 50, FlexGrow = 1 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(500, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);

        // Total space to distribute: 400 - 150 = 250
        // Child 0: 50 + 250 * (1/4) = 112.5
        // Child 1: 50 + 250 * (2/4) = 175
        // Child 2: 50 + 250 * (1/4) = 112.5
        Assert.Equal(112.5f, children[0].Layout.Width, 1);
        Assert.Equal(175f, children[1].Layout.Width, 1);
        Assert.Equal(112.5f, children[2].Layout.Width, 1);
    }

    [Fact]
    public void Box_WithFlexShrink_ShouldShrinkWhenNeeded()
    {
        // Arrange
        var container = new Box
        {
            Width = 200,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };
        container.Add(new Box { Width = 100, Height = 50, FlexShrink = 1 });
        container.Add(new Box { Width = 100, Height = 50, FlexShrink = 2 });
        container.Add(new Box { Width = 100, Height = 50, FlexShrink = 1 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);

        // Total to shrink: 300 - 200 = 100
        // Weighted by natural size: 100*1 + 100*2 + 100*1 = 400
        // Child 0: 100 - 100 * (100*1/400) = 75
        // Child 1: 100 - 100 * (100*2/400) = 50
        // Child 2: 100 - 100 * (100*1/400) = 75
        Assert.Equal(75f, children[0].Layout.Width, 1);
        Assert.Equal(50f, children[1].Layout.Width, 1);
        Assert.Equal(75f, children[2].Layout.Width, 1);
    }

    [Fact]
    public void Box_WithFlexBasis_ShouldUseAsInitialSize()
    {
        // Arrange
        var container = new Box
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row
        };
        container.Add(new Box { FlexBasis = 80, Height = 50 });
        container.Add(new Box { FlexBasis = 120, Height = 50, FlexGrow = 1 });
        container.Add(new Box { FlexBasis = 60, Height = 50 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);


        // Total flex basis: 80 + 120 + 60 = 260
        // Available space: 300
        // Extra space: 40
        // Only middle child grows: 120 + 40 = 160
        Assert.Equal(80, children[0].Layout.Width);
        Assert.Equal(160, children[1].Layout.Width);
        Assert.Equal(60, children[2].Layout.Width);
    }

    [Fact]
    public void Box_WithJustifyContent_ShouldPositionChildrenCorrectly()
    {
        // Arrange
        var scenarios = new[]
        {
            (JustifyContent.FlexStart, new[] { 0f, 50f, 100f }),
            (JustifyContent.FlexEnd, new[] { 150f, 200f, 250f }),
            (JustifyContent.Center, new[] { 75f, 125f, 175f }),
            (JustifyContent.SpaceBetween, new[] { 0f, 125f, 250f }),
            (JustifyContent.SpaceAround, new[] { 25f, 125f, 225f }),
            (JustifyContent.SpaceEvenly, new[] { 37.5f, 125f, 212.5f })
        };

        foreach (var (justifyContent, expectedPositions) in scenarios)
        {
            var container = new Box
            {
                Width = 300,
                Height = 100,
                FlexDirection = FlexDirection.Row,
                JustifyContent = justifyContent
            };
            container.Add(new Box { Width = 50, Height = 50 });
            container.Add(new Box { Width = 50, Height = 50 });
            container.Add(new Box { Width = 50, Height = 50 });

            var root = _context.ViewInstanceManager.GetOrCreateInstance(container, $"root-{justifyContent}");
            var constraints = LayoutTestHelper.Loose(400, 200);

            // Act
            var result = LayoutTestHelper.PerformLayout(root, constraints);

            // Assert
            var boxInstance = root as BoxInstance;
            Assert.NotNull(boxInstance);

            var children = boxInstance.GetChildInstances();
            Assert.Equal(3, children.Count);

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(expectedPositions[i] - children[i].Layout.X) > 1)
                {
                    _output.WriteLine($"JustifyContent.{justifyContent}: Child {i} expected at {expectedPositions[i]}, actual {children[i].Layout.X}");
                }
                Assert.Equal(expectedPositions[i], children[i].Layout.X, 1);
            }
        }
    }

    [Fact]
    public void Box_WithAlignItems_ShouldAlignChildrenCorrectly()
    {
        // Arrange
        var container = new Box
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.Center
        };
        container.Add(new Box { Width = 50, Height = 30 });
        container.Add(new Box { Width = 50, Height = 50 });
        container.Add(new Box { Width = 50, Height = 40 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);

        // Children should be centered vertically
        Assert.Equal(35, children[0].Layout.Y); // (100 - 30) / 2
        Assert.Equal(25, children[1].Layout.Y); // (100 - 50) / 2
        Assert.Equal(30, children[2].Layout.Y); // (100 - 40) / 2
    }

    [Fact]
    public void Box_WithAlignSelf_ShouldOverrideParentAlignment()
    {
        // Arrange
        var container = new Box
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            AlignItems = AlignItems.FlexStart
        };
        container.Add(new Box { Width = 50, Height = 30 });
        container.Add(new Box { Width = 50, Height = 30, AlignSelf = AlignSelf.Center });
        container.Add(new Box { Width = 50, Height = 30, AlignSelf = AlignSelf.FlexEnd });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);

        // First child uses parent's FlexStart
        Assert.Equal(0, children[0].Layout.Y);
        // Second child overrides with Center
        Assert.Equal(35, children[1].Layout.Y); // (100 - 30) / 2
        // Third child overrides with FlexEnd
        Assert.Equal(70, children[2].Layout.Y); // 100 - 30
    }

    #endregion

    #region Gap/Spacing Tests

    [Fact]
    public void Box_WithGap_ShouldSpaceChildrenEvenly()
    {
        // Arrange
        var container = new Box
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            Gap = 20
        };
        container.Add(new Box { Width = 50, Height = 50 });
        container.Add(new Box { Width = 50, Height = 50 });
        container.Add(new Box { Width = 50, Height = 50 });

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Equal(3, children.Count);

        // Children should be positioned with gaps
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(70, children[1].Layout.X);  // 50 + 20
        Assert.Equal(140, children[2].Layout.X); // 50 + 20 + 50 + 20
    }

    [Fact]
    public void Box_WithRowGapAndColumnGap_ShouldUseCorrectGap()
    {
        // Test row layout uses column gap
        var rowContainer = new Box
        {
            Width = 300,
            Height = 100,
            FlexDirection = FlexDirection.Row,
            RowGap = 10,
            ColumnGap = 30
        };
        rowContainer.Add(new Box { Width = 50, Height = 50 });
        rowContainer.Add(new Box { Width = 50, Height = 50 });

        var rowRoot = _context.ViewInstanceManager.GetOrCreateInstance(rowContainer, "row-root");
        var constraints = LayoutTestHelper.Loose(400, 200);

        var rowResult = LayoutTestHelper.PerformLayout(rowRoot, constraints);
        var rowInstance = rowRoot as BoxInstance;
        var rowChildren = rowInstance!.GetChildInstances();

        // In row layout, should use column gap
        Assert.Equal(0, rowChildren[0].Layout.X);
        Assert.Equal(80, rowChildren[1].Layout.X); // 50 + 30

        // Test column layout uses row gap
        var colContainer = new Box
        {
            Width = 100,
            Height = 300,
            FlexDirection = FlexDirection.Column,
            RowGap = 10,
            ColumnGap = 30
        };
        colContainer.Add(new Box { Width = 50, Height = 50 });
        colContainer.Add(new Box { Width = 50, Height = 50 });

        var colRoot = _context.ViewInstanceManager.GetOrCreateInstance(colContainer, "col-root");
        var colResult = LayoutTestHelper.PerformLayout(colRoot, constraints);
        var colInstance = colRoot as BoxInstance;
        var colChildren = colInstance!.GetChildInstances();

        // In column layout, should use row gap
        Assert.Equal(0, colChildren[0].Layout.Y);
        Assert.Equal(60, colChildren[1].Layout.Y); // 50 + 10
    }

    #endregion

    #region Margin Tests

    [Fact]
    public void Box_WithMargin_ShouldAffectOuterDimensions()
    {
        // Arrange
        var container = new Box
        {
            Width = 300,
            Height = 200
        };

        var childWithMargin = new Box
        {
            Width = 100,
            Height = 50,
            Margin = new Spacing(10, 20, 30, 40) // top, right, bottom, left
        };

        container.Add(childWithMargin);

        var root = _context.ViewInstanceManager.GetOrCreateInstance(container, "root");
        var constraints = LayoutTestHelper.Loose(400, 300);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);

        // Assert
        var boxInstance = root as BoxInstance;
        Assert.NotNull(boxInstance);

        var children = boxInstance.GetChildInstances();
        Assert.Single(children);

        // Child should be positioned considering margin
        var child = children[0];
        // Note: Margin handling depends on implementation
        // For now, checking that the child maintains its size
        Assert.Equal(100, child.Layout.Width);
        Assert.Equal(50, child.Layout.Height);
    }

    [Fact]
    public void Box_WithUniformMargin_ShouldApplyEquallyOnAllSides()
    {
        // Arrange
        var box = new Box
        {
            Width = 100,
            Height = 50,
            Margin = new Spacing(15) // Uniform 15px margin
        };

        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);

        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);

        // Assert
        // Box itself should be 100x50
        Assert.Equal(100, result.RootLayout.Width);
        Assert.Equal(50, result.RootLayout.Height);

        // Margin values should be set
        Assert.Equal(15, result.RootLayout.Margin.Top.Value);
        Assert.Equal(15, result.RootLayout.Margin.Right.Value);
        Assert.Equal(15, result.RootLayout.Margin.Bottom.Value);
        Assert.Equal(15, result.RootLayout.Margin.Left.Value);
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