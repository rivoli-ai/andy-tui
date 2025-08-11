using System;
using Xunit;
using Andy.TUI.Layout;

namespace Andy.TUI.Layout.Tests;

public class LengthTests
{
    [Fact]
    public void Pixels_CreatesPixelLength()
    {
        var length = Length.Pixels(42);

        Assert.Equal(42, length.Value);
        Assert.Equal(LengthUnit.Pixels, length.Unit);
        Assert.True(length.IsPixels);
    }

    [Fact]
    public void Percentage_CreatesPercentageLength()
    {
        var length = Length.Percentage(50);

        Assert.Equal(50, length.Value);
        Assert.Equal(LengthUnit.Percentage, length.Unit);
        Assert.True(length.IsPercentage);
    }

    [Fact]
    public void Auto_ReturnsAutoLength()
    {
        var length = Length.Auto;

        Assert.Equal(0, length.Value);
        Assert.Equal(LengthUnit.Auto, length.Unit);
        Assert.True(length.IsAuto);
    }


    [Fact]
    public void IsAuto_OnlyTrueForAutoUnit()
    {
        Assert.True(Length.Auto.IsAuto);
        Assert.False(Length.Pixels(0).IsAuto);
        Assert.False(Length.Percentage(0).IsAuto);
    }

    [Fact]
    public void IsPercentage_OnlyTrueForPercentageUnit()
    {
        Assert.True(Length.Percentage(50).IsPercentage);
        Assert.False(Length.Auto.IsPercentage);
        Assert.False(Length.Pixels(50).IsPercentage);
    }

    [Fact]
    public void IsPixels_OnlyTrueForPixelUnit()
    {
        Assert.True(Length.Pixels(100).IsPixels);
        Assert.False(Length.Auto.IsPixels);
        Assert.False(Length.Percentage(100).IsPixels);
    }

    [Fact]
    public void ToPixels_WithPixelUnit_ReturnsValue()
    {
        var length = Length.Pixels(42);

        Assert.Equal(42, length.ToPixels(100));
    }

    [Fact]
    public void ToPixels_WithPercentageUnit_CalculatesCorrectly()
    {
        var length = Length.Percentage(25);

        Assert.Equal(50, length.ToPixels(200)); // 25% of 200
    }

    [Fact]
    public void ToPixels_WithAutoUnit_ReturnsZero()
    {
        var length = Length.Auto;

        Assert.Equal(0, length.ToPixels(100));
    }


    [Fact]
    public void ImplicitConversion_FromFloat_CreatesPixelLength()
    {
        Length length = 42.5f;

        Assert.Equal(42.5f, length.Value);
        Assert.Equal(LengthUnit.Pixels, length.Unit);
    }

    [Fact]
    public void ImplicitConversion_FromInt_CreatesPixelLength()
    {
        Length length = 100;

        Assert.Equal(100, length.Value);
        Assert.Equal(LengthUnit.Pixels, length.Unit);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        var length1 = Length.Percentage(42);
        var length2 = Length.Percentage(42);

        Assert.True(length1.Equals(length2));
        Assert.True(length1.Equals((object)length2));
        Assert.True(length1 == length2);
        Assert.False(length1 != length2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
    {
        var length1 = Length.Percentage(42);
        var length2 = Length.Percentage(43);

        Assert.False(length1.Equals(length2));
        Assert.False(length1 == length2);
        Assert.True(length1 != length2);
    }

    [Fact]
    public void Equals_WithDifferentUnits_ReturnsFalse()
    {
        var length1 = Length.Pixels(42);
        var length2 = Length.Percentage(42);

        Assert.False(length1.Equals(length2));
        Assert.False(length1 == length2);
        Assert.True(length1 != length2);
    }

    [Fact]
    public void GetHashCode_SameValuesProduceSameHash()
    {
        var length1 = Length.Percentage(42);
        var length2 = Length.Percentage(42);

        Assert.Equal(length1.GetHashCode(), length2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        Assert.Equal("42px", Length.Pixels(42).ToString());
        Assert.Equal("50%", Length.Percentage(50).ToString());
        Assert.Equal("auto", Length.Auto.ToString());
    }

    [Fact]
    public void Parse_ValidPixelString_ReturnsLength()
    {
        var length = Length.Parse("42");

        Assert.Equal(42, length.Value);
        Assert.Equal(LengthUnit.Pixels, length.Unit);
    }

    [Fact]
    public void Parse_ValidPercentString_ReturnsLength()
    {
        var length = Length.Parse("75%");

        Assert.Equal(75, length.Value);
        Assert.Equal(LengthUnit.Percentage, length.Unit);
    }

    [Fact]
    public void Parse_AutoString_ReturnsAutoLength()
    {
        var length = Length.Parse("auto");

        Assert.True(length.IsAuto);
    }


    [Fact]
    public void Parse_NumericString_ReturnsPixelLength()
    {
        var length = Length.Parse("100");

        Assert.Equal(100, length.Value);
        Assert.Equal(LengthUnit.Pixels, length.Unit);
    }

}