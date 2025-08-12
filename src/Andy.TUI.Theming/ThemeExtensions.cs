using Andy.TUI.VirtualDom;

namespace Andy.TUI.Theming;

public static class ThemeExtensions
{
    public static T WithTheme<T>(this T element, ITheme theme, string? colorSchemeKey = null) where T : VirtualNode
    {
        var applicator = new ThemeApplicator(theme);
        applicator.ApplyToElement(element, colorSchemeKey);
        return element;
    }

    public static T WithTheme<T>(this T element, string? colorSchemeKey = null) where T : VirtualNode
    {
        return element.WithTheme(ThemeManager.Instance.CurrentTheme, colorSchemeKey);
    }

    public static T WithColorScheme<T>(this T element, string colorSchemeKey) where T : VirtualNode
    {
        return element.WithTheme(colorSchemeKey);
    }

    public static T WithPrimaryTheme<T>(this T element) where T : VirtualNode
    {
        return element.WithTheme("primary");
    }

    public static T WithSecondaryTheme<T>(this T element) where T : VirtualNode
    {
        return element.WithTheme("secondary");
    }

    public static T WithSuccessTheme<T>(this T element) where T : VirtualNode
    {
        return element.WithTheme("success");
    }

    public static T WithWarningTheme<T>(this T element) where T : VirtualNode
    {
        return element.WithTheme("warning");
    }

    public static T WithErrorTheme<T>(this T element) where T : VirtualNode
    {
        return element.WithTheme("error");
    }

    public static T WithInfoTheme<T>(this T element) where T : VirtualNode
    {
        return element.WithTheme("info");
    }

    public static T WithDisabledTheme<T>(this T element) where T : VirtualNode
    {
        var applicator = new ThemeApplicator(ThemeManager.Instance.CurrentTheme);
        applicator.ApplyDisabledStyle(element);
        return element;
    }

    public static T WithFocusedTheme<T>(this T element) where T : VirtualNode
    {
        var applicator = new ThemeApplicator(ThemeManager.Instance.CurrentTheme);
        applicator.ApplyFocusedStyle(element);
        return element;
    }

    public static T WithTypography<T>(this T element, Typography typography) where T : VirtualNode
    {
        var applicator = new ThemeApplicator(ThemeManager.Instance.CurrentTheme);
        applicator.ApplyTypography(element, typography);
        return element;
    }

    public static T WithHeadingTypography<T>(this T element) where T : VirtualNode
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        var applicator = new ThemeApplicator(theme);
        applicator.ApplyTypography(element, theme.HeadingTypography);
        return element;
    }

    public static T WithCodeTypography<T>(this T element) where T : VirtualNode
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        var applicator = new ThemeApplicator(theme);
        applicator.ApplyTypography(element, theme.CodeTypography);
        return element;
    }
}