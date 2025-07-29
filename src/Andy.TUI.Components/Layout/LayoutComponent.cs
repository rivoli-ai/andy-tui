using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Components.Layout;

/// <summary>
/// Base class for layout components that manage child arrangement.
/// </summary>
public abstract class LayoutComponent : ComponentBase
{
    private Size _availableSize = Size.Unlimited;
    private Rectangle _bounds = Rectangle.Empty;
    
    /// <summary>
    /// Gets or sets the margin around this component.
    /// </summary>
    public Spacing Margin { get; set; } = Spacing.None;
    
    /// <summary>
    /// Gets or sets the padding inside this component.
    /// </summary>
    public Spacing Padding { get; set; } = Spacing.None;
    
    /// <summary>
    /// Gets or sets the horizontal alignment.
    /// </summary>
    public Alignment HorizontalAlignment { get; set; } = Alignment.Stretch;
    
    /// <summary>
    /// Gets or sets the vertical alignment.
    /// </summary>
    public Alignment VerticalAlignment { get; set; } = Alignment.Stretch;
    
    /// <summary>
    /// Gets or sets the minimum width.
    /// </summary>
    public int? MinWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum width.
    /// </summary>
    public int? MaxWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum height.
    /// </summary>
    public int? MinHeight { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum height.
    /// </summary>
    public int? MaxHeight { get; set; }
    
    /// <summary>
    /// Gets or sets the explicit width.
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// Gets or sets the explicit height.
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// Gets the current bounds of this component.
    /// </summary>
    public Rectangle Bounds => _bounds;
    
    /// <summary>
    /// Gets the content bounds (bounds minus padding).
    /// </summary>
    public Rectangle ContentBounds => _bounds.Inset(Padding);
    
    /// <summary>
    /// Measures the desired size of this component.
    /// </summary>
    /// <param name="availableSize">The available size for layout.</param>
    /// <returns>The desired size.</returns>
    public Size Measure(Size availableSize)
    {
        _availableSize = availableSize;
        
        // Apply size constraints
        var constrainedSize = new Size(
            Math.Min(availableSize.Width, MaxWidth ?? int.MaxValue),
            Math.Min(availableSize.Height, MaxHeight ?? int.MaxValue));
        
        // Account for margin
        var sizeForContent = new Size(
            Math.Max(0, constrainedSize.Width - Margin.Horizontal),
            Math.Max(0, constrainedSize.Height - Margin.Vertical));
        
        // Measure content
        var contentSize = MeasureCore(sizeForContent);
        
        // Apply minimum constraints
        var finalWidth = Math.Max(contentSize.Width, MinWidth ?? 0);
        var finalHeight = Math.Max(contentSize.Height, MinHeight ?? 0);
        
        // Apply explicit size if set
        if (Width.HasValue)
            finalWidth = Width.Value;
        if (Height.HasValue)
            finalHeight = Height.Value;
        
        // Add margin back
        return new Size(
            finalWidth + Margin.Horizontal,
            finalHeight + Margin.Vertical);
    }
    
    /// <summary>
    /// Arranges this component within the given bounds.
    /// </summary>
    /// <param name="bounds">The bounds to arrange within.</param>
    public void Arrange(Rectangle bounds)
    {
        _bounds = bounds;
        
        // Apply margin
        var contentBounds = bounds.Inset(Margin);
        
        // Apply alignment
        var desiredSize = Measure(_availableSize);
        var actualWidth = contentBounds.Width;
        var actualHeight = contentBounds.Height;
        
        if (HorizontalAlignment != Alignment.Stretch)
        {
            actualWidth = Math.Min(actualWidth, desiredSize.Width - Margin.Horizontal);
        }
        
        if (VerticalAlignment != Alignment.Stretch)
        {
            actualHeight = Math.Min(actualHeight, desiredSize.Height - Margin.Vertical);
        }
        
        var x = contentBounds.X;
        var y = contentBounds.Y;
        
        // Apply horizontal alignment
        switch (HorizontalAlignment)
        {
            case Alignment.Center:
                x += (contentBounds.Width - actualWidth) / 2;
                break;
            case Alignment.End:
                x += contentBounds.Width - actualWidth;
                break;
        }
        
        // Apply vertical alignment
        switch (VerticalAlignment)
        {
            case Alignment.Center:
                y += (contentBounds.Height - actualHeight) / 2;
                break;
            case Alignment.End:
                y += contentBounds.Height - actualHeight;
                break;
        }
        
        var finalBounds = new Rectangle(x, y, actualWidth, actualHeight);
        ArrangeCore(finalBounds);
    }
    
    /// <summary>
    /// When overridden in a derived class, measures the content size.
    /// </summary>
    /// <param name="availableSize">The available size for content.</param>
    /// <returns>The desired content size.</returns>
    protected abstract Size MeasureCore(Size availableSize);
    
    /// <summary>
    /// When overridden in a derived class, arranges the content.
    /// </summary>
    /// <param name="bounds">The bounds for content arrangement.</param>
    protected abstract void ArrangeCore(Rectangle bounds);
    
    /// <summary>
    /// Creates a virtual node with layout attributes.
    /// </summary>
    /// <param name="tagName">The element tag name.</param>
    /// <param name="children">The child nodes.</param>
    /// <returns>The virtual node with layout attributes.</returns>
    protected VirtualNode CreateLayoutNode(string tagName, params VirtualNode[] children)
    {
        var attributes = new Dictionary<string, object?>
        {
            ["x"] = _bounds.X,
            ["y"] = _bounds.Y,
            ["width"] = _bounds.Width,
            ["height"] = _bounds.Height
        };
        
        if (Margin != Spacing.None)
        {
            attributes["margin-top"] = Margin.Top;
            attributes["margin-right"] = Margin.Right;
            attributes["margin-bottom"] = Margin.Bottom;
            attributes["margin-left"] = Margin.Left;
        }
        
        if (Padding != Spacing.None)
        {
            attributes["padding-top"] = Padding.Top;
            attributes["padding-right"] = Padding.Right;
            attributes["padding-bottom"] = Padding.Bottom;
            attributes["padding-left"] = Padding.Left;
        }
        
        return new ElementNode(tagName, attributes, children);
    }
}