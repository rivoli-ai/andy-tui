namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Specifies how text should wrap when it exceeds available width.
/// </summary>
public enum TextWrap
{
    /// <summary>
    /// No wrapping - text will be truncated if it exceeds width.
    /// </summary>
    NoWrap,

    /// <summary>
    /// Wrap at word boundaries when possible.
    /// </summary>
    Word,

    /// <summary>
    /// Wrap at any character.
    /// </summary>
    Character
}

/// <summary>
/// Specifies how text should be truncated when it exceeds available space.
/// </summary>
public enum TruncationMode
{
    /// <summary>
    /// No truncation indicator.
    /// </summary>
    None,

    /// <summary>
    /// Show ellipsis (...) at the end.
    /// </summary>
    Ellipsis,

    /// <summary>
    /// Truncate at the start of the text.
    /// </summary>
    Head,

    /// <summary>
    /// Truncate in the middle of the text.
    /// </summary>
    Middle,

    /// <summary>
    /// Truncate at the end of the text (default).
    /// </summary>
    Tail
}