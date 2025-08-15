using System.Drawing;
using Andy.TUI.Theming;

namespace Andy.TUI.Theming.Tests;

public class StandardThemeTests
{
    [Fact]
    public void LightTheme_HasCorrectProperties()
    {
        var theme = new LightTheme();

        Assert.Equal("Light", theme.Name);
        Assert.Contains("light", theme.Description.ToLower());

        Assert.Equal(Color.FromArgb(33, 33, 33), theme.Default.Foreground);
        Assert.Equal(Color.FromArgb(255, 255, 255), theme.Default.Background);

        Assert.Equal(Color.White, theme.Primary.Foreground);
        Assert.Equal(Color.FromArgb(0, 123, 255), theme.Primary.Background);

        Assert.Equal(BorderType.Single, theme.DefaultBorder.Type);
        Assert.Equal(BorderType.Single, theme.FocusedBorder.Type);
        Assert.Equal(2, theme.FocusedBorder.Width);
    }

    [Fact]
    public void LightTheme_HasAdditionalColorSchemes()
    {
        var theme = new LightTheme();

        Assert.True(theme.TryGetColorScheme("muted", out var muted));
        Assert.NotNull(muted);

        Assert.True(theme.TryGetColorScheme("accent", out var accent));
        Assert.NotNull(accent);

        Assert.True(theme.TryGetColorScheme("highlight", out var highlight));
        Assert.NotNull(highlight);
    }

    [Fact]
    public void DarkTheme_HasCorrectProperties()
    {
        var theme = new DarkTheme();

        Assert.Equal("Dark", theme.Name);
        Assert.Contains("dark", theme.Description.ToLower());

        Assert.Equal(Color.FromArgb(230, 230, 230), theme.Default.Foreground);
        Assert.Equal(Color.FromArgb(30, 30, 30), theme.Default.Background);

        Assert.Equal(Color.White, theme.Primary.Foreground);
        Assert.Equal(Color.FromArgb(0, 95, 204), theme.Primary.Background);

        Assert.Equal(BorderType.Single, theme.DefaultBorder.Type);
        Assert.Equal(Color.FromArgb(60, 60, 60), theme.DefaultBorder.Color);
    }

    [Fact]
    public void DarkTheme_HasAdditionalColorSchemes()
    {
        var theme = new DarkTheme();

        Assert.True(theme.TryGetColorScheme("surface", out var surface));
        Assert.NotNull(surface);

        Assert.True(theme.TryGetColorScheme("code", out var code));
        Assert.NotNull(code);
        Assert.Equal(Color.FromArgb(152, 195, 121), code.Foreground);
    }

    [Fact]
    public void HighContrastTheme_HasCorrectProperties()
    {
        var theme = new HighContrastTheme();

        Assert.Equal("HighContrast", theme.Name);
        Assert.Contains("accessibility", theme.Description.ToLower());

        Assert.Equal(Color.White, theme.Default.Foreground);
        Assert.Equal(Color.Black, theme.Default.Background);
        Assert.Equal(Color.White, theme.Default.BorderColor);

        Assert.Equal(Color.Black, theme.Primary.Foreground);
        Assert.Equal(Color.Cyan, theme.Primary.Background);

        Assert.Equal(BorderType.Double, theme.DefaultBorder.Type);
        Assert.Equal(BorderType.Heavy, theme.FocusedBorder.Type);
        Assert.Equal(Color.Yellow, theme.FocusedBorder.Color);
    }

    [Fact]
    public void HighContrastTheme_HasCustomTypography()
    {
        var theme = new HighContrastTheme();

        Assert.True(theme.DefaultTypography.Bold);
        Assert.True(theme.HeadingTypography.Bold);
        Assert.True(theme.HeadingTypography.Underline);
    }

    [Fact]
    public void HighContrastTheme_HasAdditionalColorSchemes()
    {
        var theme = new HighContrastTheme();

        Assert.True(theme.TryGetColorScheme("inverted", out var inverted));
        Assert.Equal(Color.Black, inverted.Foreground);
        Assert.Equal(Color.White, inverted.Background);

        Assert.True(theme.TryGetColorScheme("focus", out var focus));
        Assert.Equal(Color.Yellow, focus.Background);

        Assert.True(theme.TryGetColorScheme("link", out var link));
        Assert.Equal(Color.Cyan, link.Foreground);
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public void AllThemes_HaveRequiredColorSchemes(string themeName)
    {
        var theme = ThemeManager.Instance.GetTheme(themeName.ToLower());

        var requiredSchemes = new[] { "default", "primary", "secondary", "success", "warning", "error", "info", "disabled" };

        foreach (var schemeName in requiredSchemes)
        {
            Assert.True(theme.TryGetColorScheme(schemeName, out var scheme),
                $"Theme {themeName} missing required color scheme: {schemeName}");
            Assert.NotNull(scheme);
        }
    }

    [Theory]
    [InlineData("Light")]
    [InlineData("Dark")]
    [InlineData("HighContrast")]
    public void AllThemes_HaveValidSpacing(string themeName)
    {
        var theme = ThemeManager.Instance.GetTheme(themeName.ToLower());

        Assert.NotNull(theme.DefaultSpacing);
        Assert.NotNull(theme.CompactSpacing);
        Assert.NotNull(theme.RelaxedSpacing);

        Assert.True(theme.CompactSpacing.Top <= theme.DefaultSpacing.Top);
        Assert.True(theme.DefaultSpacing.Top <= theme.RelaxedSpacing.Top);
    }
}