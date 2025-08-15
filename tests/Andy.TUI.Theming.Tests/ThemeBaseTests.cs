using System.Drawing;
using Andy.TUI.Theming;

namespace Andy.TUI.Theming.Tests;

public class ThemeBaseTests
{
    private class TestTheme : ThemeBase
    {
        public override string Name => "TestTheme";
        public override string Description => "A test theme";

        public override ColorScheme Default => new(Color.Black, Color.White);
        public override ColorScheme Primary => new(Color.White, Color.Blue);
        public override ColorScheme Secondary => new(Color.White, Color.Gray);
        public override ColorScheme Success => new(Color.White, Color.Green);
        public override ColorScheme Warning => new(Color.Black, Color.Yellow);
        public override ColorScheme Error => new(Color.White, Color.Red);
        public override ColorScheme Info => new(Color.White, Color.Cyan);
        public override ColorScheme Disabled => new(Color.Gray, Color.LightGray);

        public override BorderStyle DefaultBorder => new(BorderType.Single, Color.Black);
        public override BorderStyle FocusedBorder => new(BorderType.Double, Color.Blue);
    }

    [Fact]
    public void Constructor_RegistersDefaultColorSchemes()
    {
        var theme = new TestTheme();

        Assert.True(theme.TryGetColorScheme("default", out var defaultScheme));
        Assert.Equal(theme.Default, defaultScheme);

        Assert.True(theme.TryGetColorScheme("primary", out var primaryScheme));
        Assert.Equal(theme.Primary, primaryScheme);

        Assert.True(theme.TryGetColorScheme("error", out var errorScheme));
        Assert.Equal(theme.Error, errorScheme);
    }

    [Fact]
    public void GetColorScheme_ReturnsCorrectScheme()
    {
        var theme = new TestTheme();

        var scheme = theme.GetColorScheme("primary");

        Assert.Equal(Color.White, scheme.Foreground);
        Assert.Equal(Color.Blue, scheme.Background);
    }

    [Fact]
    public void GetColorScheme_IsCaseInsensitive()
    {
        var theme = new TestTheme();

        var scheme1 = theme.GetColorScheme("PRIMARY");
        var scheme2 = theme.GetColorScheme("primary");
        var scheme3 = theme.GetColorScheme("PrImArY");

        Assert.Equal(scheme1, scheme2);
        Assert.Equal(scheme2, scheme3);
    }

    [Fact]
    public void GetColorScheme_ThrowsForUnknownKey()
    {
        var theme = new TestTheme();

        var exception = Assert.Throws<KeyNotFoundException>(() => theme.GetColorScheme("unknown"));
        Assert.Contains("unknown", exception.Message);
        Assert.Contains("TestTheme", exception.Message);
    }

    [Fact]
    public void TryGetColorScheme_ReturnsTrueForKnownKey()
    {
        var theme = new TestTheme();

        var result = theme.TryGetColorScheme("success", out var scheme);

        Assert.True(result);
        Assert.NotNull(scheme);
        Assert.Equal(Color.Green, scheme.Background);
    }

    [Fact]
    public void TryGetColorScheme_ReturnsFalseForUnknownKey()
    {
        var theme = new TestTheme();

        var result = theme.TryGetColorScheme("unknown", out var scheme);

        Assert.False(result);
    }

    [Fact]
    public void DefaultSpacing_ReturnsExpectedValues()
    {
        var theme = new TestTheme();

        Assert.Equal(Spacing.All(1), theme.DefaultSpacing);
        Assert.Equal(Spacing.All(0), theme.CompactSpacing);
        Assert.Equal(Spacing.All(2), theme.RelaxedSpacing);
    }

    [Fact]
    public void DefaultTypography_ReturnsExpectedValues()
    {
        var theme = new TestTheme();

        Assert.False(theme.DefaultTypography.Bold);
        Assert.True(theme.HeadingTypography.Bold);
        Assert.False(theme.CodeTypography.Italic);
    }
}