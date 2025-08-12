using System.Drawing;

namespace Andy.TUI.Theming;

public interface ITheme
{
    string Name { get; }
    string Description { get; }

    ColorScheme Default { get; }
    ColorScheme Primary { get; }
    ColorScheme Secondary { get; }
    ColorScheme Success { get; }
    ColorScheme Warning { get; }
    ColorScheme Error { get; }
    ColorScheme Info { get; }
    ColorScheme Disabled { get; }

    ColorScheme GetColorScheme(string key);
    bool TryGetColorScheme(string key, out ColorScheme colorScheme);

    BorderStyle DefaultBorder { get; }
    BorderStyle FocusedBorder { get; }

    Spacing DefaultSpacing { get; }
    Spacing CompactSpacing { get; }
    Spacing RelaxedSpacing { get; }

    Typography DefaultTypography { get; }
    Typography HeadingTypography { get; }
    Typography CodeTypography { get; }
}

public record ColorScheme(
    Color Foreground,
    Color Background,
    Color? BorderColor = null,
    Color? AccentColor = null);

public record BorderStyle(
    BorderType Type,
    Color Color,
    int Width = 1);

public enum BorderType
{
    None,
    Single,
    Double,
    Rounded,
    Heavy,
    Dashed
}

public record Spacing(
    int Top,
    int Right,
    int Bottom,
    int Left)
{
    public static Spacing All(int value) => new(value, value, value, value);
    public static Spacing Horizontal(int value) => new(0, value, 0, value);
    public static Spacing Vertical(int value) => new(value, 0, value, 0);
}

public record Typography(
    bool Bold = false,
    bool Italic = false,
    bool Underline = false,
    bool Strikethrough = false,
    bool Dim = false,
    bool Blink = false);