namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Represents a fragment node that groups multiple nodes without adding a wrapper element.
/// </summary>
public sealed class FragmentNode : VirtualNode
{
    private readonly List<VirtualNode> _children;
    
    public override VirtualNodeType Type => VirtualNodeType.Fragment;
    
    public override IReadOnlyList<VirtualNode> Children => _children.AsReadOnly();
    
    /// <summary>
    /// Initializes a new instance of the FragmentNode class.
    /// </summary>
    /// <param name="children">The child nodes.</param>
    public FragmentNode(params VirtualNode[] children)
    {
        _children = new List<VirtualNode>(children ?? Array.Empty<VirtualNode>());
    }
    
    /// <summary>
    /// Initializes a new instance of the FragmentNode class.
    /// </summary>
    /// <param name="children">The child nodes.</param>
    public FragmentNode(IEnumerable<VirtualNode> children)
    {
        _children = new List<VirtualNode>(children ?? Array.Empty<VirtualNode>());
    }
    
    /// <summary>
    /// Adds a child node to this fragment.
    /// </summary>
    public void AddChild(VirtualNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        _children.Add(child);
    }
    
    /// <summary>
    /// Removes a child node from this fragment.
    /// </summary>
    public bool RemoveChild(VirtualNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        return _children.Remove(child);
    }
    
    public override bool Equals(VirtualNode? other)
    {
        if (other is not FragmentNode fragment)
            return false;
            
        if (Key != fragment.Key)
            return false;
            
        if (Children.Count != fragment.Children.Count)
            return false;
            
        for (int i = 0; i < Children.Count; i++)
        {
            if (!Children[i].Equals(fragment.Children[i]))
                return false;
        }
        
        return true;
    }
    
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        hash.Add(Key);
        hash.Add(Children.Count);
        return hash.ToHashCode();
    }
    
    public override VirtualNode Clone()
    {
        var clonedChildren = _children.Select(c => c.Clone()).ToArray();
        return new FragmentNode(clonedChildren) { Key = Key };
    }
    
    public override void Accept(IVirtualNodeVisitor visitor)
    {
        visitor.VisitFragment(this);
    }
    
    public override string ToString()
    {
        return $"Fragment: {Children.Count} children";
    }
}