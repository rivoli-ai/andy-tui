using System;

namespace Andy.TUI.Layout;

/// <summary>
/// Represents constraints passed down during layout calculation.
/// </summary>
public readonly struct LayoutConstraints
{
    /// <summary>
    /// Gets the minimum width constraint.
    /// </summary>
    public float MinWidth { get; }
    
    /// <summary>
    /// Gets the maximum width constraint.
    /// </summary>
    public float MaxWidth { get; }
    
    /// <summary>
    /// Gets the minimum height constraint.
    /// </summary>
    public float MinHeight { get; }
    
    /// <summary>
    /// Gets the maximum height constraint.
    /// </summary>
    public float MaxHeight { get; }
    
    /// <summary>
    /// Creates unconstrained layout constraints.
    /// </summary>
    public static LayoutConstraints Unconstrained => new(0, float.PositiveInfinity, 0, float.PositiveInfinity);
    
    /// <summary>
    /// Creates layout constraints with the specified bounds.
    /// </summary>
    public LayoutConstraints(float minWidth, float maxWidth, float minHeight, float maxHeight)
    {
        MinWidth = Math.Max(0, minWidth);
        MaxWidth = Math.Max(minWidth, maxWidth);
        MinHeight = Math.Max(0, minHeight);
        MaxHeight = Math.Max(minHeight, maxHeight);
    }
    
    /// <summary>
    /// Creates tight constraints that force a specific size.
    /// </summary>
    public static LayoutConstraints Tight(float width, float height)
    {
        return new LayoutConstraints(width, width, height, height);
    }
    
    /// <summary>
    /// Creates loose constraints with a maximum size.
    /// </summary>
    public static LayoutConstraints Loose(float maxWidth, float maxHeight)
    {
        return new LayoutConstraints(0, maxWidth, 0, maxHeight);
    }
    
    /// <summary>
    /// Returns new constraints with the specified minimum width.
    /// </summary>
    public LayoutConstraints WithMinWidth(float minWidth)
    {
        return new LayoutConstraints(minWidth, MaxWidth, MinHeight, MaxHeight);
    }
    
    /// <summary>
    /// Returns new constraints with the specified maximum width.
    /// </summary>
    public LayoutConstraints WithMaxWidth(float maxWidth)
    {
        return new LayoutConstraints(MinWidth, maxWidth, MinHeight, MaxHeight);
    }
    
    /// <summary>
    /// Returns new constraints with the specified minimum height.
    /// </summary>
    public LayoutConstraints WithMinHeight(float minHeight)
    {
        return new LayoutConstraints(MinWidth, MaxWidth, minHeight, MaxHeight);
    }
    
    /// <summary>
    /// Returns new constraints with the specified maximum height.
    /// </summary>
    public LayoutConstraints WithMaxHeight(float maxHeight)
    {
        return new LayoutConstraints(MinWidth, MaxWidth, MinHeight, maxHeight);
    }
    
    /// <summary>
    /// Constrains a width value to fit within these constraints.
    /// </summary>
    public float ConstrainWidth(float width)
    {
        return Math.Max(MinWidth, Math.Min(MaxWidth, width));
    }
    
    /// <summary>
    /// Constrains a height value to fit within these constraints.
    /// </summary>
    public float ConstrainHeight(float height)
    {
        return Math.Max(MinHeight, Math.Min(MaxHeight, height));
    }
    
    /// <summary>
    /// Returns constraints with padding subtracted from the available space.
    /// </summary>
    public LayoutConstraints Deflate(Spacing padding, float parentWidth, float parentHeight)
    {
        var horizontalPadding = padding.GetHorizontalTotal(parentWidth);
        var verticalPadding = padding.GetVerticalTotal(parentHeight);
        
        return new LayoutConstraints(
            Math.Max(0, MinWidth - horizontalPadding),
            Math.Max(0, MaxWidth - horizontalPadding),
            Math.Max(0, MinHeight - verticalPadding),
            Math.Max(0, MaxHeight - verticalPadding)
        );
    }
    
    /// <summary>
    /// Checks if these constraints allow no flexibility (tight constraints).
    /// </summary>
    public bool IsTight => MinWidth == MaxWidth && MinHeight == MaxHeight;
    
    /// <summary>
    /// Checks if these constraints are unbounded.
    /// </summary>
    public bool IsUnbounded => float.IsPositiveInfinity(MaxWidth) || float.IsPositiveInfinity(MaxHeight);
    
    public override string ToString()
    {
        return $"Constraints(W: {MinWidth}-{MaxWidth}, H: {MinHeight}-{MaxHeight})";
    }
}