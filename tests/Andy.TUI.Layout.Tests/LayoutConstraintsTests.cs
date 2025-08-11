using System;
using Xunit;
using Andy.TUI.Layout;

namespace Andy.TUI.Layout.Tests;

public class LayoutConstraintsTests
{
    [Fact]
    public void Constructor_WithValues_InitializesCorrectly()
    {
        var constraints = new LayoutConstraints(10, 100, 20, 200);

        Assert.Equal(10, constraints.MinWidth);
        Assert.Equal(100, constraints.MaxWidth);
        Assert.Equal(20, constraints.MinHeight);
        Assert.Equal(200, constraints.MaxHeight);
    }

    [Fact]
    public void Constructor_WithInvalidValues_CorrectsThem()
    {
        // Max less than min should be corrected
        var constraints = new LayoutConstraints(100, 50, 80, 40);

        Assert.Equal(100, constraints.MinWidth);
        Assert.Equal(100, constraints.MaxWidth); // Corrected to equal min
        Assert.Equal(80, constraints.MinHeight);
        Assert.Equal(80, constraints.MaxHeight); // Corrected to equal min
    }

    [Fact]
    public void Constructor_WithNegativeMin_ClampsToZero()
    {
        var constraints = new LayoutConstraints(-10, 100, -20, 200);

        Assert.Equal(0, constraints.MinWidth);
        Assert.Equal(100, constraints.MaxWidth);
        Assert.Equal(0, constraints.MinHeight);
        Assert.Equal(200, constraints.MaxHeight);
    }

    [Fact]
    public void Tight_CreatesTightConstraints()
    {
        var constraints = LayoutConstraints.Tight(100, 50);

        Assert.Equal(100, constraints.MinWidth);
        Assert.Equal(100, constraints.MaxWidth);
        Assert.Equal(50, constraints.MinHeight);
        Assert.Equal(50, constraints.MaxHeight);
    }

    [Fact]
    public void Loose_CreatesLooseConstraints()
    {
        var constraints = LayoutConstraints.Loose(100, 50);

        Assert.Equal(0, constraints.MinWidth);
        Assert.Equal(100, constraints.MaxWidth);
        Assert.Equal(0, constraints.MinHeight);
        Assert.Equal(50, constraints.MaxHeight);
    }

    [Fact]
    public void Unconstrained_CreatesUnconstrainedConstraints()
    {
        var constraints = LayoutConstraints.Unconstrained;

        Assert.Equal(0, constraints.MinWidth);
        Assert.Equal(float.PositiveInfinity, constraints.MaxWidth);
        Assert.Equal(0, constraints.MinHeight);
        Assert.Equal(float.PositiveInfinity, constraints.MaxHeight);
    }

    [Fact]
    public void ConstrainWidth_ClampsWidthToMinMax()
    {
        var constraints = new LayoutConstraints(10, 100, 20, 80);

        Assert.Equal(10, constraints.ConstrainWidth(5)); // Below min
        Assert.Equal(100, constraints.ConstrainWidth(150)); // Above max
        Assert.Equal(50, constraints.ConstrainWidth(50)); // Within range
    }

    [Fact]
    public void ConstrainHeight_ClampsHeightToMinMax()
    {
        var constraints = new LayoutConstraints(10, 100, 20, 80);

        Assert.Equal(20, constraints.ConstrainHeight(10)); // Below min
        Assert.Equal(80, constraints.ConstrainHeight(100)); // Above max
        Assert.Equal(50, constraints.ConstrainHeight(50)); // Within range
    }
}