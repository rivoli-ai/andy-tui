namespace Andy.TUI.Terminal;

/// <summary>
/// Represents text styling options.
/// </summary>
public readonly struct Style : IEquatable<Style>
{
    /// <summary>
    /// Gets the foreground color.
    /// </summary>
    public Color Foreground { get; init; }

    /// <summary>
    /// Gets the background color.
    /// </summary>
    public Color Background { get; init; }

    /// <summary>
    /// Gets whether the text is bold.
    /// </summary>
    public bool Bold { get; init; }

    /// <summary>
    /// Gets whether the text is italic.
    /// </summary>
    public bool Italic { get; init; }

    /// <summary>
    /// Gets whether the text is underlined.
    /// </summary>
    public bool Underline { get; init; }

    /// <summary>
    /// Gets whether the text has strikethrough.
    /// </summary>
    public bool Strikethrough { get; init; }

    /// <summary>
    /// Gets whether the text is dim/faint.
    /// </summary>
    public bool Dim { get; init; }

    /// <summary>
    /// Gets whether the colors are inverted.
    /// </summary>
    public bool Inverse { get; init; }

    /// <summary>
    /// Gets whether the text should blink.
    /// </summary>
    public bool Blink { get; init; }

    /// <summary>
    /// Gets the default style with no formatting.
    /// </summary>
    public static Style Default { get; } = new Style();

    /// <summary>
    /// Creates a style with the specified foreground color.
    /// </summary>
    public static Style WithForeground(Color color) => new Style { Foreground = color };

    /// <summary>
    /// Creates a style with the specified background color.
    /// </summary>
    public static Style WithBackground(Color color) => new Style { Background = color };

    /// <summary>
    /// Creates a bold style.
    /// </summary>
    public static Style WithBold() => new Style { Bold = true };

    /// <summary>
    /// Creates an italic style.
    /// </summary>
    public static Style WithItalic() => new Style { Italic = true };

    /// <summary>
    /// Creates an underlined style.
    /// </summary>
    public static Style WithUnderline() => new Style { Underline = true };

    /// <summary>
    /// Combines this style with another, with the other style taking precedence.
    /// </summary>
    public Style Merge(Style other)
    {
        return new Style
        {
            Foreground = other.Foreground.Type != ColorType.None ? other.Foreground : Foreground,
            Background = other.Background.Type != ColorType.None ? other.Background : Background,
            Bold = other.Bold || Bold,
            Italic = other.Italic || Italic,
            Underline = other.Underline || Underline,
            Strikethrough = other.Strikethrough || Strikethrough,
            Dim = other.Dim || Dim,
            Inverse = other.Inverse || Inverse,
            Blink = other.Blink || Blink
        };
    }

    /// <summary>
    /// Creates a new style with the specified foreground color.
    /// </summary>
    public Style WithForegroundColor(Color color) => this with { Foreground = color };

    /// <summary>
    /// Creates a new style with the specified background color.
    /// </summary>
    public Style WithBackgroundColor(Color color) => this with { Background = color };

    /// <summary>
    /// Creates a new style with bold enabled or disabled.
    /// </summary>
    public Style WithBold(bool bold = true) => this with { Bold = bold };

    /// <summary>
    /// Creates a new style with italic enabled or disabled.
    /// </summary>
    public Style WithItalic(bool italic = true) => this with { Italic = italic };

    /// <summary>
    /// Creates a new style with underline enabled or disabled.
    /// </summary>
    public Style WithUnderline(bool underline = true) => this with { Underline = underline };

    /// <summary>
    /// Creates a new style with strikethrough enabled or disabled.
    /// </summary>
    public Style WithStrikethrough(bool strikethrough = true) => this with { Strikethrough = strikethrough };

    /// <summary>
    /// Creates a new style with dim enabled or disabled.
    /// </summary>
    public Style WithDim(bool dim = true) => this with { Dim = dim };

    /// <summary>
    /// Creates a new style with inverse enabled or disabled.
    /// </summary>
    public Style WithInverse(bool inverse = true) => this with { Inverse = inverse };

    /// <summary>
    /// Creates a new style with blink enabled or disabled.
    /// </summary>
    public Style WithBlink(bool blink = true) => this with { Blink = blink };

    public bool Equals(Style other)
    {
        return Foreground.Equals(other.Foreground) &&
               Background.Equals(other.Background) &&
               Bold == other.Bold &&
               Italic == other.Italic &&
               Underline == other.Underline &&
               Strikethrough == other.Strikethrough &&
               Dim == other.Dim &&
               Inverse == other.Inverse &&
               Blink == other.Blink;
    }

    public override bool Equals(object? obj) => obj is Style other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Foreground);
        hash.Add(Background);
        hash.Add(Bold);
        hash.Add(Italic);
        hash.Add(Underline);
        hash.Add(Strikethrough);
        hash.Add(Dim);
        hash.Add(Inverse);
        hash.Add(Blink);
        return hash.ToHashCode();
    }

    public static bool operator ==(Style left, Style right) => left.Equals(right);
    public static bool operator !=(Style left, Style right) => !left.Equals(right);

    public override string ToString()
    {
        var attributes = new List<string>();

        if (Foreground.Type != ColorType.None)
            attributes.Add($"fg={Foreground}");
        if (Background.Type != ColorType.None)
            attributes.Add($"bg={Background}");
        if (Bold)
            attributes.Add("bold");
        if (Italic)
            attributes.Add("italic");
        if (Underline)
            attributes.Add("underline");
        if (Strikethrough)
            attributes.Add("strikethrough");
        if (Dim)
            attributes.Add("dim");
        if (Inverse)
            attributes.Add("inverse");
        if (Blink)
            attributes.Add("blink");

        return attributes.Count > 0 ? $"Style({string.Join(", ", attributes)})" : "Style()";
    }
}