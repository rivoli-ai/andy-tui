using System;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class GradientComponentTests
{
    [Fact]
    public void Gradient_CreatesSuccessfully()
    {
        // Act
        var gradient = new Gradient("Hello World", Color.Red, Color.Blue);

        // Assert
        Assert.NotNull(gradient);
    }

    [Fact]
    public void Gradient_CreatesWithAllParameters()
    {
        // Act
        var gradient = new Gradient(
            "Gradient Text",
            Color.Green,
            Color.Yellow,
            GradientDirection.Vertical,
            bold: true,
            italic: true,
            underline: true
        );

        // Assert
        Assert.NotNull(gradient);
    }

    [Theory]
    [InlineData(GradientDirection.Horizontal)]
    [InlineData(GradientDirection.Vertical)]
    [InlineData(GradientDirection.Diagonal)]
    public void Gradient_SupportsAllDirections(GradientDirection direction)
    {
        // Act
        var gradient = new Gradient("Test", Color.White, Color.Black, direction);

        // Assert
        Assert.NotNull(gradient);
    }

    [Fact]
    public void InterpolateColor_ReturnsValidColors()
    {
        // Test various interpolation factors
        var factors = new[] { 0f, 0.25f, 0.5f, 0.75f, 1f };
        
        foreach (var factor in factors)
        {
            // Act
            var color = Gradient.InterpolateColor(Color.Red, Color.Blue, factor);

            // Assert
            Assert.NotEqual(Color.None, color);
        }
    }

    [Fact]
    public void GradientInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var gradient = new Gradient("Test", Color.Red, Color.Blue);

        // Act
        var instance = manager.GetOrCreateInstance(gradient, "gradient1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<GradientInstance>(instance);
    }

    [Fact]
    public void Gradient_ShowcaseExamples()
    {
        // Test examples from FinalComponentsShowcase
        
        // Horizontal gradient
        var horizontal = new Gradient("Horizontal Gradient Text", Color.Red, Color.Blue);
        Assert.NotNull(horizontal);

        // Vertical gradient
        var vertical = new Gradient("Vertical\nGradient\nText", Color.Green, Color.Yellow, GradientDirection.Vertical);
        Assert.NotNull(vertical);

        // Diagonal gradient with bold
        var diagonal = new Gradient("Diagonal Gradient Effect", Color.Magenta, Color.Cyan, GradientDirection.Diagonal, bold: true);
        Assert.NotNull(diagonal);

        // Underlined gradient
        var underlined = new Gradient("System running at 75% capacity", Color.Yellow, Color.Green, underline: true);
        Assert.NotNull(underlined);
    }

    [Fact]
    public void Gradient_HandlesEmptyText()
    {
        // Act
        var gradient = new Gradient("", Color.Red, Color.Blue);

        // Assert
        Assert.NotNull(gradient);
    }

    [Fact]
    public void Gradient_HandlesMultilineText()
    {
        // Act
        var gradient = new Gradient("Line 1\nLine 2\nLine 3", Color.Red, Color.Blue);

        // Assert
        Assert.NotNull(gradient);
    }

    [Fact]
    public void Gradient_VariousColorCombinations()
    {
        // Test various color combinations
        var combinations = new[]
        {
            ("RGB gradient", Color.Red, Color.Green),
            ("Cyan to Magenta", Color.Cyan, Color.Magenta),
            ("Black to White", Color.Black, Color.White),
            ("Yellow to Blue", Color.Yellow, Color.Blue)
        };

        foreach (var (text, start, end) in combinations)
        {
            // Act
            var gradient = new Gradient(text, start, end);

            // Assert
            Assert.NotNull(gradient);
        }
    }

    [Fact]
    public void InterpolateColor_ClampsFactorToValidRange()
    {
        // Test that interpolation handles out-of-range factors gracefully
        var color1 = Gradient.InterpolateColor(Color.Red, Color.Blue, -0.5f);
        var color2 = Gradient.InterpolateColor(Color.Red, Color.Blue, 1.5f);

        // Assert - Should not throw and should return valid colors
        Assert.NotEqual(Color.None, color1);
        Assert.NotEqual(Color.None, color2);
    }
}