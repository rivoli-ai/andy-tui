namespace Andy.TUI.Declarative.Layout;

/// <summary>
/// Defines how an individual item aligns itself along the cross axis, overriding the container's align-items.
/// </summary>
public enum AlignSelf
{
    /// <summary>
    /// Use the container's align-items value (default).
    /// </summary>
    Auto,
    
    /// <summary>
    /// Align at the start of the cross axis.
    /// </summary>
    FlexStart,
    
    /// <summary>
    /// Align at the end of the cross axis.
    /// </summary>
    FlexEnd,
    
    /// <summary>
    /// Center along the cross axis.
    /// </summary>
    Center,
    
    /// <summary>
    /// Stretch to fill the container.
    /// </summary>
    Stretch,
    
    /// <summary>
    /// Align along baseline.
    /// </summary>
    Baseline
}