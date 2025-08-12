using Andy.TUI.Theming;

namespace Andy.TUI.Theming.Tests;

public class SpacingTests
{
    [Fact]
    public void Spacing_ConstructorSetsProperties()
    {
        var spacing = new Spacing(1, 2, 3, 4);

        Assert.Equal(1, spacing.Top);
        Assert.Equal(2, spacing.Right);
        Assert.Equal(3, spacing.Bottom);
        Assert.Equal(4, spacing.Left);
    }

    [Fact]
    public void Spacing_All_CreatesSameValueForAllSides()
    {
        var spacing = Spacing.All(5);

        Assert.Equal(5, spacing.Top);
        Assert.Equal(5, spacing.Right);
        Assert.Equal(5, spacing.Bottom);
        Assert.Equal(5, spacing.Left);
    }

    [Fact]
    public void Spacing_Horizontal_SetsLeftAndRight()
    {
        var spacing = Spacing.Horizontal(3);

        Assert.Equal(0, spacing.Top);
        Assert.Equal(3, spacing.Right);
        Assert.Equal(0, spacing.Bottom);
        Assert.Equal(3, spacing.Left);
    }

    [Fact]
    public void Spacing_Vertical_SetsTopAndBottom()
    {
        var spacing = Spacing.Vertical(2);

        Assert.Equal(2, spacing.Top);
        Assert.Equal(0, spacing.Right);
        Assert.Equal(2, spacing.Bottom);
        Assert.Equal(0, spacing.Left);
    }

    [Fact]
    public void Spacing_RecordEquality()
    {
        var spacing1 = new Spacing(1, 2, 3, 4);
        var spacing2 = new Spacing(1, 2, 3, 4);
        var spacing3 = new Spacing(1, 2, 3, 5);

        Assert.Equal(spacing1, spacing2);
        Assert.NotEqual(spacing1, spacing3);

        var allSpacing1 = Spacing.All(2);
        var allSpacing2 = Spacing.All(2);

        Assert.Equal(allSpacing1, allSpacing2);
    }
}