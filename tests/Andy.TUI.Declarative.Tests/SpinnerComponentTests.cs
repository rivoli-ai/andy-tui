using System;
using System.Linq;
using System.Threading;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class SpinnerComponentTests
{
    [Fact]
    public void Spinner_CreatesSuccessfully()
    {
        // Act
        var spinner = new Spinner();

        // Assert
        Assert.NotNull(spinner);
    }

    [Fact]
    public void Spinner_CreatesWithAllParameters()
    {
        // Act
        var spinner = new Spinner(
            SpinnerStyle.Line,
            customFrames: null,
            color: Color.Green,
            label: "Loading...",
            labelFirst: true,
            frameDelay: 100
        );

        // Assert
        Assert.NotNull(spinner);
    }

    [Theory]
    [InlineData(SpinnerStyle.Dots, 10)]
    [InlineData(SpinnerStyle.Line, 4)]
    [InlineData(SpinnerStyle.Star, 6)]
    [InlineData(SpinnerStyle.Box, 4)]
    [InlineData(SpinnerStyle.Bounce, 4)]
    [InlineData(SpinnerStyle.Pulse, 6)]
    [InlineData(SpinnerStyle.Arrow, 8)]
    [InlineData(SpinnerStyle.Circle, 4)]
    public void GetFrames_ReturnsCorrectFrameCount(SpinnerStyle style, int expectedFrameCount)
    {
        // Act
        var frames = Spinner.GetFrames(style);

        // Assert
        Assert.Equal(expectedFrameCount, frames.Length);
        Assert.All(frames, frame => Assert.NotNull(frame));
        Assert.All(frames, frame => Assert.NotEmpty(frame));
    }

    [Fact]
    public void GetFrames_AllFramesHaveSameLength()
    {
        // Arrange
        var styles = Enum.GetValues<SpinnerStyle>().Where(s => s != SpinnerStyle.Custom);

        foreach (var style in styles)
        {
            // Act
            var frames = Spinner.GetFrames(style);

            // Assert
            if (frames.Length > 0)
            {
                var firstLength = frames[0].Length;
                Assert.All(frames, frame => Assert.Equal(firstLength, frame.Length));
            }
        }
    }

    [Fact]
    public void SpinnerInstance_CreatesSuccessfully()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var spinner = new Spinner(SpinnerStyle.Dots);

        // Act
        var instance = manager.GetOrCreateInstance(spinner, "spinner1");

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<SpinnerInstance>(instance);
    }

    [Fact]
    public void Spinner_ShowcaseExamples()
    {
        // Test examples from UIComponentsShowcase
        
        // Dots spinner with label
        var dotsSpinner = new Spinner(SpinnerStyle.Dots, color: Color.Cyan, label: "Loading");
        Assert.NotNull(dotsSpinner);

        // Line spinner
        var lineSpinner = new Spinner(SpinnerStyle.Line, color: Color.Green);
        Assert.NotNull(lineSpinner);

        // Arrow spinner
        var arrowSpinner = new Spinner(SpinnerStyle.Arrow, color: Color.Yellow);
        Assert.NotNull(arrowSpinner);

        // Box spinner with label first
        var boxSpinner = new Spinner(SpinnerStyle.Box, color: Color.Red, label: "Saving", labelFirst: true);
        Assert.NotNull(boxSpinner);

        // Pulse spinner
        var pulseSpinner = new Spinner(SpinnerStyle.Pulse, color: Color.Magenta);
        Assert.NotNull(pulseSpinner);
    }

    [Theory]
    [InlineData(SpinnerStyle.Dots)]
    [InlineData(SpinnerStyle.Line)]
    [InlineData(SpinnerStyle.Box)]
    [InlineData(SpinnerStyle.Pulse)]
    [InlineData(SpinnerStyle.Arrow)]
    public void Spinner_AllStylesHaveFrames(SpinnerStyle style)
    {
        // Act
        var frames = Spinner.GetFrames(style);

        // Assert
        Assert.NotEmpty(frames);
        Assert.All(frames, frame => Assert.False(string.IsNullOrWhiteSpace(frame)));
    }

    [Fact]
    public void Spinner_CustomFrameDelayWorks()
    {
        // Test different frame delays
        var fastSpinner = new Spinner(frameDelay: 50);
        Assert.NotNull(fastSpinner);

        var slowSpinner = new Spinner(frameDelay: 200);
        Assert.NotNull(slowSpinner);
    }

    [Fact]
    public void Spinner_CustomFramesWork()
    {
        // Arrange
        var customFrames = new[] { "1", "2", "3", "4" };

        // Act
        var spinner = new Spinner(SpinnerStyle.Custom, customFrames: customFrames);

        // Assert
        Assert.NotNull(spinner);
    }
}