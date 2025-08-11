using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Text transformation modes.
/// </summary>
public enum TextTransform
{
    None,
    Uppercase,
    Lowercase,
    Capitalize,     // First letter of each word
    CapitalizeFirst // First letter only
}

/// <summary>
/// A component that applies text transformations to its content.
/// </summary>
public class Transform : ISimpleComponent
{
    private readonly string _text;
    private readonly TextTransform _transform;
    private readonly Style _style;

    public Transform(string text, TextTransform transform = TextTransform.None)
    {
        _text = text ?? "";
        _transform = transform;
        _style = Style.Default;
    }

    private Transform(string text, TextTransform transform, Style style)
    {
        _text = text;
        _transform = transform;
        _style = style;
    }

    public Transform Uppercase() => new(_text, TextTransform.Uppercase, _style);
    public Transform Lowercase() => new(_text, TextTransform.Lowercase, _style);
    public Transform Capitalize() => new(_text, TextTransform.Capitalize, _style);
    public Transform CapitalizeFirst() => new(_text, TextTransform.CapitalizeFirst, _style);

    // Style methods
    public Transform Color(Color color) => new(_text, _transform, _style.WithForegroundColor(color));
    public Transform Bold() => new(_text, _transform, _style.WithBold(true));
    public Transform Italic() => new(_text, _transform, _style.WithItalic(true));
    public Transform Underline() => new(_text, _transform, _style.WithUnderline(true));

    // Internal accessors for view instance
    internal string GetText() => _text;
    internal TextTransform GetTransform() => _transform;
    internal Style GetStyle() => _style;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Transform declarations should not be rendered directly. Use ViewInstanceManager.");
    }

    // Helper method to apply transformation
    public static string ApplyTransform(string text, TextTransform transform)
    {
        return transform switch
        {
            TextTransform.None => text,
            TextTransform.Uppercase => text.ToUpper(),
            TextTransform.Lowercase => text.ToLower(),
            TextTransform.Capitalize => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower()),
            TextTransform.CapitalizeFirst => text.Length > 0
                ? char.ToUpper(text[0]) + text.Substring(1).ToLower()
                : text,
            _ => text
        };
    }
}