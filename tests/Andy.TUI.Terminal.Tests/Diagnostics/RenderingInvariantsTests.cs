using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Diagnostics;
using Xunit;

namespace Andy.TUI.Terminal.Tests.Diagnostics;

public class RenderingInvariantsTests
{
    [Fact]
    public void ValidateUniformLineBackgrounds_UniformSegment_DoesNotThrow()
    {
        var buffer = new Buffer(10, 1);
        // Fill "hello" with same background
        var style = Style.Default.WithBackgroundColor(Color.Cyan);
        buffer[0, 0] = new Cell('h', style);
        buffer[1, 0] = new Cell('e', style);
        buffer[2, 0] = new Cell('l', style);
        buffer[3, 0] = new Cell('l', style);
        buffer[4, 0] = new Cell('o', style);
        // Trailing spaces without background are allowed
        for (int x = 5; x < 10; x++)
        {
            buffer[x, 0] = new Cell(' ', Style.Default);
        }

        var options = new RenderingInvariantOptions { Enabled = true, ThrowOnViolation = true };
        RenderingInvariants.ValidateUniformLineBackgrounds(buffer, options);
    }

    [Fact]
    public void ValidateUniformLineBackgrounds_NonUniformSegment_Throws()
    {
        var buffer = new Buffer(10, 1);
        var styleA = Style.Default.WithBackgroundColor(Color.Cyan);
        var styleB = Style.Default.WithBackgroundColor(Color.Black);
        buffer[0, 0] = new Cell('a', styleA);
        buffer[1, 0] = new Cell('b', styleB); // different background in same segment
        buffer[2, 0] = new Cell('c', styleA);

        var options = new RenderingInvariantOptions { Enabled = true, ThrowOnViolation = true };
        Assert.Throws<RenderingInvariantViolationException>(() =>
            RenderingInvariants.ValidateUniformLineBackgrounds(buffer, options));
    }

    [Fact]
    public void ValidateUniformLineBackgrounds_SegmentsSeparatedBySpaces_IndependentlyChecked()
    {
        var buffer = new Buffer(10, 1);
        var styleA = Style.Default.WithBackgroundColor(Color.Cyan);
        var styleB = Style.Default.WithBackgroundColor(Color.Black);
        buffer[0, 0] = new Cell('a', styleA);
        buffer[1, 0] = new Cell('b', styleA);
        buffer[2, 0] = new Cell(' ', Style.Default); // separator (no bg)
        buffer[3, 0] = new Cell('x', styleB);
        buffer[4, 0] = new Cell('y', styleB);

        var options = new RenderingInvariantOptions { Enabled = true, ThrowOnViolation = true };
        RenderingInvariants.ValidateUniformLineBackgrounds(buffer, options);
    }
}
