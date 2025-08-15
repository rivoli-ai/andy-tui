using Andy.TUI.Terminal;
using Andy.TUI.VirtualDom;
using SysColor = System.Drawing.Color;

namespace Andy.TUI.Theming;

public class ThemeApplicator
{
    private readonly ITheme _theme;

    public ThemeApplicator(ITheme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
    }

    public void ApplyToElement(VirtualNode element, string? colorSchemeKey = null)
    {
        var colorScheme = GetColorScheme(colorSchemeKey);

        if (colorScheme.Foreground != SysColor.Empty)
            element.Props["foregroundColor"] = colorScheme.Foreground;

        if (colorScheme.Background != SysColor.Empty)
            element.Props["backgroundColor"] = colorScheme.Background;

        if (colorScheme.BorderColor.HasValue && element is IHasBorder borderedElement)
        {
            ApplyBorderStyle(borderedElement, _theme.DefaultBorder, colorScheme.BorderColor.Value);
        }
    }

    public void ApplyTypography(VirtualNode element, Typography? typography = null)
    {
        var typo = typography ?? _theme.DefaultTypography;

        if (typo.Bold)
            element.AddModifier(TextModifier.Bold);

        if (typo.Italic)
            element.AddModifier(TextModifier.Italic);

        if (typo.Underline)
            element.AddModifier(TextModifier.Underline);

        if (typo.Strikethrough)
            element.AddModifier(TextModifier.Strikethrough);

        if (typo.Dim)
            element.AddModifier(TextModifier.Dim);

        if (typo.Blink)
            element.AddModifier(TextModifier.Blink);
    }

    public void ApplySpacing(IHasSpacing element, Spacing? spacing = null)
    {
        var space = spacing ?? _theme.DefaultSpacing;

        element.PaddingTop = space.Top;
        element.PaddingRight = space.Right;
        element.PaddingBottom = space.Bottom;
        element.PaddingLeft = space.Left;
    }

    public void ApplyBorderStyle(IHasBorder element, BorderStyle? style = null, SysColor? overrideColor = null)
    {
        var borderStyle = style ?? _theme.DefaultBorder;

        element.BorderType = ConvertBorderType(borderStyle.Type);
        element.BorderColor = overrideColor ?? borderStyle.Color;
        element.BorderWidth = borderStyle.Width;
    }

    public void ApplyFocusedStyle(VirtualNode element)
    {
        ApplyToElement(element, "primary");

        if (element is IHasBorder borderedElement)
        {
            ApplyBorderStyle(borderedElement, _theme.FocusedBorder);
        }
    }

    public void ApplyDisabledStyle(VirtualNode element)
    {
        ApplyToElement(element, "disabled");
    }

    private ColorScheme GetColorScheme(string? key)
    {
        if (string.IsNullOrEmpty(key))
            return _theme.Default;

        return _theme.TryGetColorScheme(key, out var scheme)
            ? scheme
            : _theme.Default;
    }

    private string ConvertBorderType(Theming.BorderType themeType)
    {
        return themeType switch
        {
            Theming.BorderType.None => "none",
            Theming.BorderType.Single => "single",
            Theming.BorderType.Double => "double",
            Theming.BorderType.Rounded => "rounded",
            Theming.BorderType.Heavy => "heavy",
            Theming.BorderType.Dashed => "dashed",
            _ => "single"
        };
    }
}

public interface IHasBorder
{
    string BorderType { get; set; }
    SysColor BorderColor { get; set; }
    int BorderWidth { get; set; }
}

public interface IHasSpacing
{
    int PaddingTop { get; set; }
    int PaddingRight { get; set; }
    int PaddingBottom { get; set; }
    int PaddingLeft { get; set; }
}

public enum TextModifier
{
    Bold,
    Italic,
    Underline,
    Strikethrough,
    Dim,
    Blink
}

public static class VirtualNodeExtensions
{
    private const string MODIFIERS_KEY = "textModifiers";

    public static void AddModifier(this VirtualNode element, TextModifier modifier)
    {
        if (!element.Props.TryGetValue(MODIFIERS_KEY, out var value) || value is not HashSet<TextModifier> modifiers)
        {
            modifiers = new HashSet<TextModifier>();
            element.Props[MODIFIERS_KEY] = modifiers;
        }
        modifiers.Add(modifier);
    }

    public static bool HasModifier(this VirtualNode element, TextModifier modifier)
    {
        if (element.Props.TryGetValue(MODIFIERS_KEY, out var value) && value is HashSet<TextModifier> modifiers)
        {
            return modifiers.Contains(modifier);
        }
        return false;
    }

    public static void RemoveModifier(this VirtualNode element, TextModifier modifier)
    {
        if (element.Props.TryGetValue(MODIFIERS_KEY, out var value) && value is HashSet<TextModifier> modifiers)
        {
            modifiers.Remove(modifier);
        }
    }

    public static void ClearModifiers(this VirtualNode element)
    {
        element.Props.Remove(MODIFIERS_KEY);
    }
}