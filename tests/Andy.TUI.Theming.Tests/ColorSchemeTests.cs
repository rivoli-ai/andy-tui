using System.Drawing;
using Andy.TUI.Theming;

namespace Andy.TUI.Theming.Tests;

public class ColorSchemeTests
{
    [Fact]
    public void ColorScheme_ConstructorSetsProperties()
    {
        var foreground = Color.Red;
        var background = Color.Blue;
        var border = Color.Green;
        var accent = Color.Yellow;

        var scheme = new ColorScheme(foreground, background, border, accent);

        Assert.Equal(foreground, scheme.Foreground);
        Assert.Equal(background, scheme.Background);
        Assert.Equal(border, scheme.BorderColor);
        Assert.Equal(accent, scheme.AccentColor);
    }

    [Fact]
    public void ColorScheme_OptionalPropertiesCanBeNull()
    {
        var scheme = new ColorScheme(Color.Black, Color.White);

        Assert.Equal(Color.Black, scheme.Foreground);
        Assert.Equal(Color.White, scheme.Background);
        Assert.Null(scheme.BorderColor);
        Assert.Null(scheme.AccentColor);
    }

    [Fact]
    public void ColorScheme_RecordEquality()
    {
        var scheme1 = new ColorScheme(Color.Red, Color.Blue, Color.Green);
        var scheme2 = new ColorScheme(Color.Red, Color.Blue, Color.Green);
        var scheme3 = new ColorScheme(Color.Red, Color.Blue, Color.Yellow);

        Assert.Equal(scheme1, scheme2);
        Assert.NotEqual(scheme1, scheme3);
    }
}