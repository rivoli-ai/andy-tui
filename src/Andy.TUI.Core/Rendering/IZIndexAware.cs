namespace Andy.TUI.Core.Rendering;

/// <summary>
/// Interface for components that participate in hierarchical z-index resolution.
/// </summary>
public interface IZIndexAware
{
    /// <summary>
    /// Gets or sets the z-index relative to the component's parent.
    /// </summary>
    int RelativeZIndex { get; set; }

    /// <summary>
    /// Gets the computed absolute z-index after resolution.
    /// This value is calculated during rendering based on the component hierarchy.
    /// </summary>
    int AbsoluteZIndex { get; }

    /// <summary>
    /// Updates the absolute z-index based on the current context.
    /// </summary>
    /// <param name="context">The z-index context for resolution.</param>
    void UpdateAbsoluteZIndex(ZIndexContext context);

    /// <summary>
    /// Gets whether this component creates a new stacking context.
    /// Components that create stacking contexts reset the z-index baseline for their children.
    /// </summary>
    bool CreatesStackingContext { get; }
}