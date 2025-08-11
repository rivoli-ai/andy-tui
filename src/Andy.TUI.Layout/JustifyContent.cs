namespace Andy.TUI.Layout;

/// <summary>
/// Defines how content is justified along the main axis of a flex container.
/// </summary>
public enum JustifyContent
{
    /// <summary>
    /// Items are packed toward the start of the main axis (default).
    /// </summary>
    FlexStart,

    /// <summary>
    /// Items are packed toward the end of the main axis.
    /// </summary>
    FlexEnd,

    /// <summary>
    /// Items are centered along the main axis.
    /// </summary>
    Center,

    /// <summary>
    /// Items are evenly distributed with the first item at the start and last item at the end.
    /// </summary>
    SpaceBetween,

    /// <summary>
    /// Items are evenly distributed with equal space around them.
    /// </summary>
    SpaceAround,

    /// <summary>
    /// Items are evenly distributed with equal space between them.
    /// </summary>
    SpaceEvenly
}