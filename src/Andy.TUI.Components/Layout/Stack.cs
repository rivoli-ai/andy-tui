using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Components.Layout;

/// <summary>
/// A layout component that arranges children in a horizontal or vertical stack.
/// </summary>
public class Stack : LayoutComponent
{
    private readonly List<VirtualNode> _children = new();
    private readonly List<Size> _childSizes = new();
    
    /// <summary>
    /// Gets or sets the stack orientation.
    /// </summary>
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    
    /// <summary>
    /// Gets or sets the spacing between children.
    /// </summary>
    public int Spacing { get; set; } = 0;
    
    /// <summary>
    /// Gets or sets the alignment of children perpendicular to the stack direction.
    /// </summary>
    public Alignment CrossAxisAlignment { get; set; } = Alignment.Stretch;
    
    /// <summary>
    /// Gets or sets the alignment of children along the stack direction.
    /// </summary>
    public Alignment MainAxisAlignment { get; set; } = Alignment.Start;
    
    /// <summary>
    /// Gets or sets whether children should be reversed.
    /// </summary>
    public bool Reverse { get; set; } = false;
    
    /// <summary>
    /// Gets the children of this stack.
    /// </summary>
    public IReadOnlyList<VirtualNode> Children => _children;
    
    /// <summary>
    /// Adds a child to the stack.
    /// </summary>
    public void AddChild(VirtualNode child)
    {
        _children.Add(child);
        RequestRender();
    }
    
    /// <summary>
    /// Removes a child from the stack.
    /// </summary>
    public bool RemoveChild(VirtualNode child)
    {
        var result = _children.Remove(child);
        if (result)
        {
            RequestRender();
        }
        return result;
    }
    
    /// <summary>
    /// Clears all children from the stack.
    /// </summary>
    public void ClearChildren()
    {
        _children.Clear();
        RequestRender();
    }
    
    /// <summary>
    /// Sets the children of the stack.
    /// </summary>
    public void SetChildren(IEnumerable<VirtualNode> children)
    {
        _children.Clear();
        _children.AddRange(children);
        RequestRender();
    }
    
    protected override Size MeasureCore(Size availableSize)
    {
        _childSizes.Clear();
        
        var totalMainAxis = 0;
        var maxCrossAxis = 0;
        var childCount = 0;
        
        var isHorizontal = Orientation == Orientation.Horizontal;
        
        foreach (var child in GetOrderedChildren())
        {
            var childAvailableSize = isHorizontal
                ? new Size(int.MaxValue, availableSize.Height - Padding.Vertical)
                : new Size(availableSize.Width - Padding.Horizontal, int.MaxValue);
            
            var childSize = MeasureChild(child, childAvailableSize);
            _childSizes.Add(childSize);
            
            if (isHorizontal)
            {
                totalMainAxis += childSize.Width;
                maxCrossAxis = Math.Max(maxCrossAxis, childSize.Height);
            }
            else
            {
                totalMainAxis += childSize.Height;
                maxCrossAxis = Math.Max(maxCrossAxis, childSize.Width);
            }
            
            childCount++;
        }
        
        // Add spacing
        if (childCount > 1)
        {
            totalMainAxis += Spacing * (childCount - 1);
        }
        
        var width = isHorizontal ? totalMainAxis : maxCrossAxis;
        var height = isHorizontal ? maxCrossAxis : totalMainAxis;
        
        return new Size(
            width + Padding.Horizontal,
            height + Padding.Vertical);
    }
    
    protected override void ArrangeCore(Rectangle bounds)
    {
        var contentBounds = bounds.Inset(Padding);
        var isHorizontal = Orientation == Orientation.Horizontal;
        var children = GetOrderedChildren().ToList();
        
        if (children.Count == 0)
            return;
        
        // Calculate total size of children
        var totalMainAxis = 0;
        var totalSpacing = Spacing * (children.Count - 1);
        
        for (int i = 0; i < children.Count; i++)
        {
            totalMainAxis += isHorizontal ? _childSizes[i].Width : _childSizes[i].Height;
        }
        
        // Calculate starting position based on main axis alignment
        var mainAxisStart = isHorizontal ? contentBounds.X : contentBounds.Y;
        var mainAxisSize = isHorizontal ? contentBounds.Width : contentBounds.Height;
        var extraSpace = Math.Max(0, mainAxisSize - totalMainAxis - totalSpacing);
        
        switch (MainAxisAlignment)
        {
            case Alignment.Center:
                mainAxisStart += extraSpace / 2;
                break;
            case Alignment.End:
                mainAxisStart += extraSpace;
                break;
            case Alignment.Stretch:
                // Distribute extra space among children
                if (children.Count > 0)
                {
                    var extraPerChild = extraSpace / children.Count;
                    for (int i = 0; i < _childSizes.Count; i++)
                    {
                        if (isHorizontal)
                            _childSizes[i] = new Size(_childSizes[i].Width + extraPerChild, _childSizes[i].Height);
                        else
                            _childSizes[i] = new Size(_childSizes[i].Width, _childSizes[i].Height + extraPerChild);
                    }
                }
                break;
        }
        
        // Arrange children
        var currentMainAxis = mainAxisStart;
        
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var childSize = _childSizes[i];
            
            Rectangle childBounds;
            
            if (isHorizontal)
            {
                var y = CalculateCrossAxisPosition(contentBounds.Y, contentBounds.Height, childSize.Height);
                var width = CrossAxisAlignment == Alignment.Stretch ? contentBounds.Width : childSize.Width;
                childBounds = new Rectangle(currentMainAxis, y, childSize.Width, 
                    CrossAxisAlignment == Alignment.Stretch ? contentBounds.Height : childSize.Height);
                currentMainAxis += childSize.Width + Spacing;
            }
            else
            {
                var x = CalculateCrossAxisPosition(contentBounds.X, contentBounds.Width, childSize.Width);
                var width = CrossAxisAlignment == Alignment.Stretch ? contentBounds.Width : childSize.Width;
                childBounds = new Rectangle(x, currentMainAxis, width, childSize.Height);
                currentMainAxis += childSize.Height + Spacing;
            }
            
            ArrangeChild(child, childBounds);
        }
    }
    
    protected override VirtualNode OnRender()
    {
        var children = GetOrderedChildren().ToArray();
        return CreateLayoutNode("stack", children);
    }
    
    private IEnumerable<VirtualNode> GetOrderedChildren()
    {
        return Reverse ? _children.AsEnumerable().Reverse() : _children;
    }
    
    private Size MeasureChild(VirtualNode child, Size availableSize)
    {
        if (child is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            return layoutComponent.Measure(availableSize);
        }
        
        // For non-layout nodes, estimate size
        if (child is TextNode textNode)
        {
            var lines = textNode.Content.Split('\n');
            return new Size(lines.Length > 0 ? lines.Max(l => l.Length) : 0, lines.Length);
        }
        
        // Default size for unknown nodes
        return new Size(1, 1);
    }
    
    private void ArrangeChild(VirtualNode child, Rectangle bounds)
    {
        if (child is ComponentNode componentNode && componentNode.ComponentInstance is LayoutComponent layoutComponent)
        {
            layoutComponent.Arrange(bounds);
        }
    }
    
    private int CalculateCrossAxisPosition(int start, int size, int childSize)
    {
        return CrossAxisAlignment switch
        {
            Alignment.Center => start + (size - childSize) / 2,
            Alignment.End => start + size - childSize,
            _ => start
        };
    }
}