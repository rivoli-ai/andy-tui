namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Represents an element node in the virtual DOM.
/// </summary>
public sealed class ElementNode : VirtualNode
{
    private readonly List<VirtualNode> _children;
    
    /// <summary>
    /// Gets the element tag name.
    /// </summary>
    public string TagName { get; }
    
    public override VirtualNodeType Type => VirtualNodeType.Element;
    
    public override IReadOnlyList<VirtualNode> Children => _children.AsReadOnly();
    
    /// <summary>
    /// Initializes a new instance of the ElementNode class.
    /// </summary>
    /// <param name="tagName">The element tag name.</param>
    /// <param name="props">The element properties.</param>
    /// <param name="children">The child nodes.</param>
    public ElementNode(string tagName, Dictionary<string, object?>? props = null, params VirtualNode[] children)
    {
        TagName = tagName ?? throw new ArgumentNullException(nameof(tagName));
        _children = new List<VirtualNode>(children ?? Array.Empty<VirtualNode>());
        
        if (props != null)
        {
            Props = new Dictionary<string, object?>(props);
        }
    }
    
    /// <summary>
    /// Adds a child node to this element.
    /// </summary>
    public void AddChild(VirtualNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
    }
    
    /// <summary>
    /// Removes a child node from this element.
    /// </summary>
    public bool RemoveChild(VirtualNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        return _children.Remove(child);
    }
    
    /// <summary>
    /// Replaces a child node at the specified index.
    /// </summary>
    public void ReplaceChild(int index, VirtualNode newChild)
    {
        ArgumentNullException.ThrowIfNull(newChild);
        if (index < 0 || index >= _children.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
            
        _children[index] = newChild;
    }
    
    public override bool Equals(VirtualNode? other)
    {
        if (other is not ElementNode element)
            return false;
            
        if (TagName != element.TagName || Key != element.Key)
            return false;
            
        if (Props.Count != element.Props.Count)
            return false;
            
        foreach (var kvp in Props)
        {
            if (!element.Props.TryGetValue(kvp.Key, out var otherValue) ||
                !Equals(kvp.Value, otherValue))
            {
                return false;
            }
        }
        
        if (Children.Count != element.Children.Count)
            return false;
            
        for (int i = 0; i < Children.Count; i++)
        {
            if (!Children[i].Equals(element.Children[i]))
                return false;
        }
        
        return true;
    }
    
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        hash.Add(TagName);
        hash.Add(Key);
        hash.Add(Props.Count);
        hash.Add(Children.Count);
        return hash.ToHashCode();
    }
    
    public override VirtualNode Clone()
    {
        var clonedProps = new Dictionary<string, object?>(Props);
        var clonedChildren = _children.Select(c => c.Clone()).ToArray();
        return new ElementNode(TagName, clonedProps, clonedChildren) { Key = Key };
    }
    
    public override void Accept(IVirtualNodeVisitor visitor)
    {
        visitor.VisitElement(this);
    }
    
    public override string ToString()
    {
        return $"<{TagName} children={Children.Count}>";
    }
}