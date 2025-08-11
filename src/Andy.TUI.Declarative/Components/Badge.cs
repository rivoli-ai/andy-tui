using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Badge style variations.
/// </summary>
public enum BadgeStyle
{
    Default,    // Simple text badge
    Rounded,    // Rounded corners
    Square,     // Square corners
    Pill,       // Pill-shaped
    Dot,        // Small dot indicator
    Count,      // Number count style
    Icon        // Icon-based badge
}

/// <summary>
/// Badge variant for semantic coloring.
/// </summary>
public enum BadgeVariant
{
    Default,
    Success,
    Warning,
    Error,
    Info,
    Primary,
    Secondary
}

/// <summary>
/// A badge component for status indicators and labels.
/// </summary>
public class Badge : ISimpleComponent
{
    private readonly string _content;
    private readonly BadgeStyle _style;
    private readonly BadgeVariant _variant;
    private readonly Color? _customColor;
    private readonly Color? _customBackgroundColor;
    private readonly bool _bold;
    private readonly string _prefix;
    private readonly string _suffix;

    public Badge(
        string content,
        BadgeStyle style = BadgeStyle.Default,
        BadgeVariant variant = BadgeVariant.Default,
        Color? customColor = null,
        Color? customBackgroundColor = null,
        bool bold = true,
        string prefix = "",
        string suffix = "")
    {
        _content = content ?? "";
        _style = style;
        _variant = variant;
        _customColor = customColor;
        _customBackgroundColor = customBackgroundColor;
        _bold = bold;
        _prefix = prefix ?? "";
        _suffix = suffix ?? "";
    }

    // Internal accessors for view instance
    internal string GetContent() => _content;
    internal BadgeStyle GetStyle() => _style;
    internal BadgeVariant GetVariant() => _variant;
    internal Color? GetCustomColor() => _customColor;
    internal Color? GetCustomBackgroundColor() => _customBackgroundColor;
    internal bool GetBold() => _bold;
    internal string GetPrefix() => _prefix;
    internal string GetSuffix() => _suffix;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Badge declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // Helper method to get colors based on variant
    internal static (Color foreground, Color background) GetVariantColors(BadgeVariant variant)
    {
        return variant switch
        {
            BadgeVariant.Success => (Color.Black, Color.Green),
            BadgeVariant.Warning => (Color.Black, Color.Yellow),
            BadgeVariant.Error => (Color.White, Color.Red),
            BadgeVariant.Info => (Color.White, Color.Blue),
            BadgeVariant.Primary => (Color.White, Color.Cyan),
            BadgeVariant.Secondary => (Color.White, Color.Magenta),
            _ => (Color.White, Color.DarkGray)
        };
    }

    // Helper method to get badge borders based on style
    internal static (string left, string right) GetStyleBorders(BadgeStyle style)
    {
        return style switch
        {
            BadgeStyle.Rounded => ("(", ")"),
            BadgeStyle.Square => ("[", "]"),
            BadgeStyle.Pill => ("◖", "◗"),
            BadgeStyle.Dot => ("●", ""),
            BadgeStyle.Count => ("<", ">"),
            BadgeStyle.Icon => ("", ""),
            _ => ("", "")
        };
    }
}