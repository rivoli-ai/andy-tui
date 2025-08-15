using System.Drawing;

namespace Andy.TUI.Theming;

public class LightTheme : ThemeBase
{
    public override string Name => "Light";
    public override string Description => "A clean, light theme with good readability";

    public override ColorScheme Default => new(
        Foreground: Color.FromArgb(33, 33, 33),
        Background: Color.FromArgb(255, 255, 255),
        BorderColor: Color.FromArgb(200, 200, 200));

    public override ColorScheme Primary => new(
        Foreground: Color.White,
        Background: Color.FromArgb(0, 123, 255),
        BorderColor: Color.FromArgb(0, 100, 200),
        AccentColor: Color.FromArgb(0, 86, 179));

    public override ColorScheme Secondary => new(
        Foreground: Color.White,
        Background: Color.FromArgb(108, 117, 125),
        BorderColor: Color.FromArgb(90, 98, 104),
        AccentColor: Color.FromArgb(73, 80, 87));

    public override ColorScheme Success => new(
        Foreground: Color.White,
        Background: Color.FromArgb(40, 167, 69),
        BorderColor: Color.FromArgb(34, 139, 34),
        AccentColor: Color.FromArgb(28, 116, 48));

    public override ColorScheme Warning => new(
        Foreground: Color.FromArgb(33, 33, 33),
        Background: Color.FromArgb(255, 193, 7),
        BorderColor: Color.FromArgb(255, 160, 0),
        AccentColor: Color.FromArgb(255, 140, 0));

    public override ColorScheme Error => new(
        Foreground: Color.White,
        Background: Color.FromArgb(220, 53, 69),
        BorderColor: Color.FromArgb(200, 35, 51),
        AccentColor: Color.FromArgb(189, 33, 48));

    public override ColorScheme Info => new(
        Foreground: Color.White,
        Background: Color.FromArgb(23, 162, 184),
        BorderColor: Color.FromArgb(17, 122, 139),
        AccentColor: Color.FromArgb(19, 132, 150));

    public override ColorScheme Disabled => new(
        Foreground: Color.FromArgb(150, 150, 150),
        Background: Color.FromArgb(240, 240, 240),
        BorderColor: Color.FromArgb(220, 220, 220));

    public override BorderStyle DefaultBorder => new(
        Type: BorderType.Single,
        Color: Color.FromArgb(200, 200, 200));

    public override BorderStyle FocusedBorder => new(
        Type: BorderType.Single,
        Color: Color.FromArgb(0, 123, 255),
        Width: 2);

    protected override void RegisterDefaultColorSchemes()
    {
        base.RegisterDefaultColorSchemes();

        RegisterColorScheme("muted", new(
            Foreground: Color.FromArgb(108, 117, 125),
            Background: Color.FromArgb(248, 249, 250)));

        RegisterColorScheme("accent", new(
            Foreground: Color.FromArgb(0, 123, 255),
            Background: Color.FromArgb(232, 244, 255)));

        RegisterColorScheme("highlight", new(
            Foreground: Color.FromArgb(33, 33, 33),
            Background: Color.FromArgb(255, 243, 205)));
    }
}