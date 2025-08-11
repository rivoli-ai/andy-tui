namespace Andy.TUI.Layout;

/// <summary>
/// Defines how items are aligned along the cross axis of a flex container.
/// </summary>
public enum AlignItems
{
    /// <summary>
    /// Items are aligned at the start of the cross axis.
    /// </summary>
    FlexStart,

    /// <summary>
    /// Items are aligned at the end of the cross axis.
    /// </summary>
    FlexEnd,

    /// <summary>
    /// Items are centered along the cross axis.
    /// </summary>
    Center,

    /// <summary>
    /// Items are stretched to fill the container (default).
    /// </summary>
    Stretch,

    /// <summary>
    /// Items are aligned along their baselines.
    /// </summary>
    Baseline
}