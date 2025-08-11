using System;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class ProgressBarComponentTests
{
    [Fact]
    public void ProgressBar_CreatesSuccessfully()
    {
        // Act
        var progressBar = new ProgressBar(0.5f);

        // Assert
        Assert.NotNull(progressBar);
    }

    [Fact]
    public void ProgressBar_CreatesWithAllParameters()
    {
        // Act
        var progressBar = new ProgressBar(
            0.75f,
            minValue: 0f,
            maxValue: 100f,
            width: 30,
            style: ProgressBarStyle.Line,
            filledChar: '=',
            emptyChar: '-',
            filledColor: Color.Green,
            emptyColor: Color.Black,
            showPercentage: false,
            label: ""
        );

        // Assert
        Assert.NotNull(progressBar);
    }

    [Theory]
    [InlineData(-0.5f)]
    [InlineData(0f)]
    [InlineData(0.25f)]
    [InlineData(0.5f)]
    [InlineData(0.75f)]
    [InlineData(1f)]
    [InlineData(1.5f)]
    public void ProgressBar_HandlesVariousProgressValues(float progress)
    {
        // Act
        var progressBar = new ProgressBar(progress);

        // Assert
        Assert.NotNull(progressBar);
    }

    [Theory]
    [InlineData(ProgressBarStyle.Solid, '█', '░')]
    [InlineData(ProgressBarStyle.Line, '━', '─')]
    [InlineData(ProgressBarStyle.Dots, '●', '○')]
    [InlineData(ProgressBarStyle.Blocks, '▓', '░')]
    [InlineData(ProgressBarStyle.Arrows, '→', '─')]
    public void GetStyleChars_ReturnsCorrectCharsForStyle(ProgressBarStyle style, char expectedFilled, char expectedEmpty)
    {
        // Act
        var (filled, empty) = ProgressBar.GetStyleChars(style);

        // Assert
        Assert.Equal(expectedFilled, filled);
        Assert.Equal(expectedEmpty, empty);
    }

    [Fact]
    public void ProgressBarInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var progressBar = new ProgressBar(0.5f);

        // Act
        var instance = manager.GetOrCreateInstance(progressBar, "progress1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<ProgressBarInstance>(instance);
    }

    [Fact]
    public void ProgressBar_ShowcaseExamples()
    {
        // Test examples from UIComponentsShowcase

        // Default style
        var defaultBar = new ProgressBar(0.7f);
        Assert.NotNull(defaultBar);

        // Line style
        var lineBar = new ProgressBar(0.45f, style: ProgressBarStyle.Line, filledColor: Color.Green);
        Assert.NotNull(lineBar);

        // Dots style
        var dotsBar = new ProgressBar(0.9f, style: ProgressBarStyle.Dots, filledColor: Color.Yellow, width: 30);
        Assert.NotNull(dotsBar);

        // Custom characters
        var customBar = new ProgressBar(0.33f, filledChar: '▓', emptyChar: '░');
        Assert.NotNull(customBar);
    }

    [Fact]
    public void ProgressBar_DifferentStylesInShowcase()
    {
        // Test all styles shown in examples
        var styles = new[]
        {
            ProgressBarStyle.Solid,
            ProgressBarStyle.Line,
            ProgressBarStyle.Dots,
            ProgressBarStyle.Blocks,
            ProgressBarStyle.Arrows
        };

        foreach (var style in styles)
        {
            var progressBar = new ProgressBar(0.5f, style: style);
            Assert.NotNull(progressBar);

            var (filled, empty) = ProgressBar.GetStyleChars(style);
            Assert.NotEqual(filled, empty); // Characters should be different
        }
    }
}