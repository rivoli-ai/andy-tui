using System.Drawing;

namespace Andy.TUI.Theming;

public class DarkTheme : ThemeBase
{
    public override string Name => "Dark";
    public override string Description => "A modern dark theme optimized for low-light environments";

    public override ColorScheme Default => new(
        Foreground: Color.FromArgb(230, 230, 230),
        Background: Color.FromArgb(30, 30, 30),
        BorderColor: Color.FromArgb(60, 60, 60));

    public override ColorScheme Primary => new(
        Foreground: Color.White,
        Background: Color.FromArgb(0, 95, 204),
        BorderColor: Color.FromArgb(0, 75, 160),
        AccentColor: Color.FromArgb(51, 133, 255));

    public override ColorScheme Secondary => new(
        Foreground: Color.FromArgb(230, 230, 230),
        Background: Color.FromArgb(68, 68, 68),
        BorderColor: Color.FromArgb(85, 85, 85),
        AccentColor: Color.FromArgb(102, 102, 102));

    public override ColorScheme Success => new(
        Foreground: Color.White,
        Background: Color.FromArgb(34, 139, 34),
        BorderColor: Color.FromArgb(28, 116, 28),
        AccentColor: Color.FromArgb(50, 205, 50));

    public override ColorScheme Warning => new(
        Foreground: Color.Black,
        Background: Color.FromArgb(255, 165, 0),
        BorderColor: Color.FromArgb(255, 140, 0),
        AccentColor: Color.FromArgb(255, 193, 37));

    public override ColorScheme Error => new(
        Foreground: Color.White,
        Background: Color.FromArgb(178, 34, 34),
        BorderColor: Color.FromArgb(139, 0, 0),
        AccentColor: Color.FromArgb(255, 69, 69));

    public override ColorScheme Info => new(
        Foreground: Color.White,
        Background: Color.FromArgb(0, 139, 139),
        BorderColor: Color.FromArgb(0, 100, 100),
        AccentColor: Color.FromArgb(64, 224, 208));

    public override ColorScheme Disabled => new(
        Foreground: Color.FromArgb(100, 100, 100),
        Background: Color.FromArgb(45, 45, 45),
        BorderColor: Color.FromArgb(55, 55, 55));

    public override BorderStyle DefaultBorder => new(
        Type: BorderType.Single,
        Color: Color.FromArgb(60, 60, 60));

    public override BorderStyle FocusedBorder => new(
        Type: BorderType.Single,
        Color: Color.FromArgb(51, 133, 255),
        Width: 2);

    protected override void RegisterDefaultColorSchemes()
    {
        base.RegisterDefaultColorSchemes();

        RegisterColorScheme("muted", new(
            Foreground: Color.FromArgb(150, 150, 150),
            Background: Color.FromArgb(40, 40, 40)));

        RegisterColorScheme("accent", new(
            Foreground: Color.FromArgb(51, 133, 255),
            Background: Color.FromArgb(10, 30, 60)));

        RegisterColorScheme("highlight", new(
            Foreground: Color.FromArgb(255, 255, 255),
            Background: Color.FromArgb(60, 60, 80)));

        RegisterColorScheme("surface", new(
            Foreground: Color.FromArgb(230, 230, 230),
            Background: Color.FromArgb(40, 40, 40),
            BorderColor: Color.FromArgb(70, 70, 70)));

        RegisterColorScheme("code", new(
            Foreground: Color.FromArgb(152, 195, 121),
            Background: Color.FromArgb(35, 35, 35),
            BorderColor: Color.FromArgb(55, 55, 55)));
    }
}