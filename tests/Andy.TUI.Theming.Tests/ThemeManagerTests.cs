using System.Drawing;
using Andy.TUI.Theming;

namespace Andy.TUI.Theming.Tests;

public class ThemeManagerTests
{
    [Fact]
    public void Instance_ReturnsSameInstance()
    {
        var instance1 = ThemeManager.Instance;
        var instance2 = ThemeManager.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Constructor_RegistersBuiltInThemes()
    {
        var manager = ThemeManager.Instance;

        Assert.True(manager.TryGetTheme("light", out var lightTheme));
        Assert.NotNull(lightTheme);
        Assert.Equal("Light", lightTheme!.Name);

        Assert.True(manager.TryGetTheme("dark", out var darkTheme));
        Assert.NotNull(darkTheme);
        Assert.Equal("Dark", darkTheme!.Name);

        Assert.True(manager.TryGetTheme("highcontrast", out var hcTheme));
        Assert.NotNull(hcTheme);
        Assert.Equal("HighContrast", hcTheme!.Name);
    }

    [Fact]
    public void CurrentTheme_DefaultsToLight()
    {
        var manager = ThemeManager.Instance;

        Assert.NotNull(manager.CurrentTheme);
        Assert.Equal("Light", manager.CurrentTheme.Name);
    }

    [Fact]
    public void SetTheme_ChangesCurrentTheme()
    {
        var manager = ThemeManager.Instance;

        manager.SetTheme("dark");

        Assert.Equal("Dark", manager.CurrentTheme.Name);

        manager.SetTheme("light");
        Assert.Equal("Light", manager.CurrentTheme.Name);
    }

    [Fact]
    public void SetTheme_RaisesThemeChangedEvent()
    {
        var manager = ThemeManager.Instance;
        ITheme? changedTheme = null;

        manager.ThemeChanged += (sender, args) =>
        {
            changedTheme = args.NewTheme;
        };

        manager.SetTheme("dark");

        Assert.NotNull(changedTheme);
        Assert.Equal("Dark", changedTheme!.Name);
    }

    [Fact]
    public void RegisterTheme_AddsNewTheme()
    {
        var manager = ThemeManager.Instance;
        var customTheme = new CustomTestTheme();

        manager.RegisterTheme(customTheme);

        Assert.True(manager.TryGetTheme("customtest", out var retrievedTheme));
        Assert.Same(customTheme, retrievedTheme);
    }

    [Fact]
    public void RegisterTheme_ThrowsForNull()
    {
        var manager = ThemeManager.Instance;

        Assert.Throws<ArgumentNullException>(() => manager.RegisterTheme(null!));
    }

    [Fact]
    public void GetTheme_ThrowsForUnknownTheme()
    {
        var manager = ThemeManager.Instance;

        var exception = Assert.Throws<KeyNotFoundException>(() => manager.GetTheme("nonexistent"));
        Assert.Contains("nonexistent", exception.Message);
    }

    [Fact]
    public void TrySetTheme_ReturnsTrueForKnownTheme()
    {
        var manager = ThemeManager.Instance;

        var result = manager.TrySetTheme("dark");

        Assert.True(result);
        Assert.Equal("Dark", manager.CurrentTheme.Name);
    }

    [Fact]
    public void TrySetTheme_ReturnsFalseForUnknownTheme()
    {
        var manager = ThemeManager.Instance;
        var originalTheme = manager.CurrentTheme;

        var result = manager.TrySetTheme("nonexistent");

        Assert.False(result);
        Assert.Same(originalTheme, manager.CurrentTheme);
    }

    [Fact]
    public void GetAllThemes_ReturnsAllRegisteredThemes()
    {
        var manager = ThemeManager.Instance;

        var themes = manager.GetAllThemes().ToList();

        Assert.Contains(themes, t => t.Name == "Light");
        Assert.Contains(themes, t => t.Name == "Dark");
        Assert.Contains(themes, t => t.Name == "HighContrast");
        Assert.True(themes.Count >= 3);
    }

    private class CustomTestTheme : ThemeBase
    {
        public override string Name => "CustomTest";
        public override string Description => "A custom test theme";

        public override ColorScheme Default => new(Color.Black, Color.White);
        public override ColorScheme Primary => new(Color.White, Color.Purple);
        public override ColorScheme Secondary => new(Color.White, Color.Gray);
        public override ColorScheme Success => new(Color.White, Color.Green);
        public override ColorScheme Warning => new(Color.Black, Color.Orange);
        public override ColorScheme Error => new(Color.White, Color.Red);
        public override ColorScheme Info => new(Color.White, Color.Teal);
        public override ColorScheme Disabled => new(Color.DarkGray, Color.LightGray);

        public override BorderStyle DefaultBorder => new(BorderType.Rounded, Color.Purple);
        public override BorderStyle FocusedBorder => new(BorderType.Heavy, Color.Purple);
    }
}