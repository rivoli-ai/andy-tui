using System;
using Xunit;
using Andy.TUI.Layout;

namespace Andy.TUI.Layout.Tests;

public class SpacingTests
{
    [Fact]
    public void Constructor_DefaultInitializesAllToAuto()
    {
        var spacing = new Spacing();

        Assert.Equal(0, spacing.Top.Value);
        Assert.Equal(0, spacing.Right.Value);
        Assert.Equal(0, spacing.Bottom.Value);
        Assert.Equal(0, spacing.Left.Value);
        Assert.Equal(LengthUnit.Auto, spacing.Top.Unit);
    }

    [Fact]
    public void Constructor_WithSingleValue_AppliesToAllSides()
    {
        var spacing = new Spacing(10);

        Assert.Equal(10, spacing.Top.Value);
        Assert.Equal(10, spacing.Right.Value);
        Assert.Equal(10, spacing.Bottom.Value);
        Assert.Equal(10, spacing.Left.Value);
    }

    [Fact]
    public void Constructor_WithSingleLength_AppliesToAllSides()
    {
        var length = Length.Percentage(15);
        var spacing = new Spacing(length);

        Assert.Equal(15, spacing.Top.Value);
        Assert.Equal(15, spacing.Right.Value);
        Assert.Equal(15, spacing.Bottom.Value);
        Assert.Equal(15, spacing.Left.Value);
        Assert.Equal(LengthUnit.Percentage, spacing.Top.Unit);
        Assert.Equal(LengthUnit.Percentage, spacing.Right.Unit);
    }

    [Fact]
    public void Constructor_WithVerticalHorizontal_AppliesCorrectly()
    {
        var spacing = new Spacing(10, 20);

        Assert.Equal(10, spacing.Top.Value);
        Assert.Equal(20, spacing.Right.Value);
        Assert.Equal(10, spacing.Bottom.Value);
        Assert.Equal(20, spacing.Left.Value);
    }

    [Fact]
    public void Constructor_WithVerticalHorizontalLengths_AppliesCorrectly()
    {
        var vertical = Length.Percentage(5);
        var horizontal = Length.Pixels(10);
        var spacing = new Spacing(vertical, horizontal);

        Assert.Equal(5, spacing.Top.Value);
        Assert.Equal(LengthUnit.Percentage, spacing.Top.Unit);
        Assert.Equal(10, spacing.Right.Value);
        Assert.Equal(LengthUnit.Pixels, spacing.Right.Unit);
        Assert.Equal(5, spacing.Bottom.Value);
        Assert.Equal(10, spacing.Left.Value);
    }

    [Fact]
    public void Constructor_WithFourValues_AppliesCorrectly()
    {
        var spacing = new Spacing(1, 2, 3, 4);

        Assert.Equal(1, spacing.Top.Value);
        Assert.Equal(2, spacing.Right.Value);
        Assert.Equal(3, spacing.Bottom.Value);
        Assert.Equal(4, spacing.Left.Value);
    }

    [Fact]
    public void Constructor_WithFourLengths_AppliesCorrectly()
    {
        var spacing = new Spacing(
            Length.Pixels(10),
            Length.Percentage(20),
            Length.Pixels(30),
            Length.Percentage(40)
        );

        Assert.Equal(10, spacing.Top.Value);
        Assert.Equal(LengthUnit.Pixels, spacing.Top.Unit);
        Assert.Equal(20, spacing.Right.Value);
        Assert.Equal(LengthUnit.Percentage, spacing.Right.Unit);
        Assert.Equal(30, spacing.Bottom.Value);
        Assert.Equal(LengthUnit.Pixels, spacing.Bottom.Unit);
        Assert.Equal(40, spacing.Left.Value);
        Assert.Equal(LengthUnit.Percentage, spacing.Left.Unit);
    }



    [Fact]
    public void Zero_ReturnsZeroSpacing()
    {
        var spacing = Spacing.Zero;

        Assert.Equal(0, spacing.Top.Value);
        Assert.Equal(0, spacing.Right.Value);
        Assert.Equal(0, spacing.Bottom.Value);
        Assert.Equal(0, spacing.Left.Value);
    }

    [Fact]
    public void OnlyTop_CreatesSpacingWithOnlyTop()
    {
        var spacing = Spacing.OnlyTop(10);

        Assert.Equal(10, spacing.Top.Value);
        Assert.Equal(0, spacing.Right.Value);
        Assert.Equal(0, spacing.Bottom.Value);
        Assert.Equal(0, spacing.Left.Value);
    }

    [Fact]
    public void OnlyRight_CreatesSpacingWithOnlyRight()
    {
        var spacing = Spacing.OnlyRight(15);

        Assert.Equal(0, spacing.Top.Value);
        Assert.Equal(15, spacing.Right.Value);
        Assert.Equal(0, spacing.Bottom.Value);
        Assert.Equal(0, spacing.Left.Value);
    }

    [Fact]
    public void Horizontal_CreatesSpacingWithLeftAndRight()
    {
        var spacing = Spacing.Horizontal(20);

        Assert.Equal(0, spacing.Top.Value);
        Assert.Equal(20, spacing.Right.Value);
        Assert.Equal(0, spacing.Bottom.Value);
        Assert.Equal(20, spacing.Left.Value);
    }

    [Fact]
    public void Vertical_CreatesSpacingWithTopAndBottom()
    {
        var spacing = Spacing.Vertical(25);

        Assert.Equal(25, spacing.Top.Value);
        Assert.Equal(0, spacing.Right.Value);
        Assert.Equal(25, spacing.Bottom.Value);
        Assert.Equal(0, spacing.Left.Value);
    }

    [Fact]
    public void GetHorizontalTotal_CalculatesSum()
    {
        var spacing = new Spacing(10, 20, 30, 40);

        Assert.Equal(60, spacing.GetHorizontalTotal(100)); // 20 + 40
    }

    [Fact]
    public void GetVerticalTotal_CalculatesSum()
    {
        var spacing = new Spacing(10, 20, 30, 40);

        Assert.Equal(40, spacing.GetVerticalTotal(100)); // 10 + 30
    }

    [Fact]
    public void ImplicitConversion_FromInt_CreatesUniformSpacing()
    {
        Spacing spacing = 42;

        Assert.Equal(42, spacing.Top.Value);
        Assert.Equal(42, spacing.Right.Value);
        Assert.Equal(42, spacing.Bottom.Value);
        Assert.Equal(42, spacing.Left.Value);
    }

    [Fact]
    public void ImplicitConversion_FromFloat_CreatesUniformSpacing()
    {
        Spacing spacing = 42.5f;

        Assert.Equal(42.5f, spacing.Top.Value);
        Assert.Equal(42.5f, spacing.Right.Value);
        Assert.Equal(42.5f, spacing.Bottom.Value);
        Assert.Equal(42.5f, spacing.Left.Value);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var spacing1 = new Spacing(10, 20, 30, 40);
        var spacing2 = new Spacing(10, 20, 30, 40);

        Assert.True(spacing1.Equals(spacing2));
        Assert.True(spacing1.Equals((object)spacing2));
        Assert.True(spacing1 == spacing2);
        Assert.False(spacing1 != spacing2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var spacing1 = new Spacing(10, 20, 30, 40);
        var spacing2 = new Spacing(10, 20, 30, 41);

        Assert.False(spacing1.Equals(spacing2));
        Assert.False(spacing1 == spacing2);
        Assert.True(spacing1 != spacing2);
    }

    [Fact]
    public void GetHashCode_SameValuesProduceSameHash()
    {
        var spacing1 = new Spacing(10, 20, 30, 40);
        var spacing2 = new Spacing(10, 20, 30, 40);

        Assert.Equal(spacing1.GetHashCode(), spacing2.GetHashCode());
    }

    [Fact]
    public void ToString_UniformSpacing_ReturnsSingleValue()
    {
        var spacing = new Spacing(10);

        Assert.Equal("10px", spacing.ToString());
    }

    [Fact]
    public void ToString_VerticalHorizontal_ReturnsTwoValues()
    {
        var spacing = new Spacing(10, 20);

        Assert.Equal("10px 20px", spacing.ToString());
    }

    [Fact]
    public void ToString_AllDifferent_ReturnsFourValues()
    {
        var spacing = new Spacing(10, 20, 30, 40);

        Assert.Equal("10px 20px 30px 40px", spacing.ToString());
    }

}