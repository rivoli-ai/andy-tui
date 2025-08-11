namespace Andy.TUI.Layout;

/// <summary>
/// Defines whether flex items should wrap to new lines.
/// </summary>
public enum FlexWrap
{
    /// <summary>
    /// Items are laid out in a single line (default).
    /// </summary>
    NoWrap,

    /// <summary>
    /// Items wrap to new lines as needed.
    /// </summary>
    Wrap,

    /// <summary>
    /// Items wrap to new lines in reverse order.
    /// </summary>
    WrapReverse
}