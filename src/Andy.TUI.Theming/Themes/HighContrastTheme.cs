using System.Drawing;

namespace Andy.TUI.Theming;

public class HighContrastTheme : ThemeBase
{
    public override string Name => "HighContrast";
    public override string Description => "Maximum contrast theme for improved accessibility";

    public override ColorScheme Default => new(
        Foreground: Color.White,
        Background: Color.Black,
        BorderColor: Color.White);

    public override ColorScheme Primary => new(
        Foreground: Color.Black,
        Background: Color.Cyan,
        BorderColor: Color.White,
        AccentColor: Color.FromArgb(0, 255, 255));

    public override ColorScheme Secondary => new(
        Foreground: Color.Black,
        Background: Color.Yellow,
        BorderColor: Color.White,
        AccentColor: Color.FromArgb(255, 255, 0));

    public override ColorScheme Success => new(
        Foreground: Color.Black,
        Background: Color.Lime,
        BorderColor: Color.White,
        AccentColor: Color.FromArgb(0, 255, 0));

    public override ColorScheme Warning => new(
        Foreground: Color.Black,
        Background: Color.Yellow,
        BorderColor: Color.White,
        AccentColor: Color.FromArgb(255, 255, 0));

    public override ColorScheme Error => new(
        Foreground: Color.White,
        Background: Color.Red,
        BorderColor: Color.White,
        AccentColor: Color.FromArgb(255, 0, 0));

    public override ColorScheme Info => new(
        Foreground: Color.Black,
        Background: Color.Cyan,
        BorderColor: Color.White,
        AccentColor: Color.FromArgb(0, 255, 255));

    public override ColorScheme Disabled => new(
        Foreground: Color.Gray,
        Background: Color.FromArgb(32, 32, 32),
        BorderColor: Color.Gray);

    public override BorderStyle DefaultBorder => new(
        Type: BorderType.Double,
        Color: Color.White,
        Width: 1);

    public override BorderStyle FocusedBorder => new(
        Type: BorderType.Heavy,
        Color: Color.Yellow,
        Width: 2);

    public override Typography DefaultTypography => new(Bold: true);

    public override Typography HeadingTypography => new(Bold: true, Underline: true);

    protected override void RegisterDefaultColorSchemes()
    {
        base.RegisterDefaultColorSchemes();

        RegisterColorScheme("inverted", new(
            Foreground: Color.Black,
            Background: Color.White,
            BorderColor: Color.Black));

        RegisterColorScheme("accent", new(
            Foreground: Color.Black,
            Background: Color.Magenta,
            BorderColor: Color.White));

        RegisterColorScheme("highlight", new(
            Foreground: Color.Black,
            Background: Color.Yellow,
            BorderColor: Color.White));

        RegisterColorScheme("link", new(
            Foreground: Color.Cyan,
            Background: Color.Black,
            BorderColor: Color.Cyan));

        RegisterColorScheme("focus", new(
            Foreground: Color.Black,
            Background: Color.Yellow,
            BorderColor: Color.Yellow));
    }
}