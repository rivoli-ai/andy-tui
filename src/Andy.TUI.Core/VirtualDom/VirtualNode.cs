namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Represents a node in the virtual DOM tree.
/// </summary>
public abstract class VirtualNode : IEquatable<VirtualNode>
{
    /// <summary>
    /// Gets the type of the node.
    /// </summary>
    public abstract VirtualNodeType Type { get; }
    
    /// <summary>
    /// Gets or sets the key used for efficient diffing.
    /// </summary>
    public string? Key { get; init; }
    
    /// <summary>
    /// Gets the properties/attributes of the node.
    /// </summary>
    public Dictionary<string, object?> Props { get; init; } = new();
    
    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    public virtual IReadOnlyList<VirtualNode> Children => Array.Empty<VirtualNode>();
    
    /// <summary>
    /// Determines whether this node is equal to another node.
    /// </summary>
    public abstract bool Equals(VirtualNode? other);
    
    public override bool Equals(object? obj) => obj is VirtualNode other && Equals(other);
    
    public abstract override int GetHashCode();
    
    /// <summary>
    /// Creates a deep clone of this node.
    /// </summary>
    public abstract VirtualNode Clone();
    
    /// <summary>
    /// Accepts a visitor for traversing the virtual DOM tree.
    /// </summary>
    public abstract void Accept(IVirtualNodeVisitor visitor);
}

/// <summary>
/// Defines the types of virtual nodes.
/// </summary>
public enum VirtualNodeType
{
    /// <summary>
    /// A text node containing string content.
    /// </summary>
    Text,
    
    /// <summary>
    /// An element node representing a UI element.
    /// </summary>
    Element,
    
    /// <summary>
    /// A component node representing a reusable component.
    /// </summary>
    Component,
    
    /// <summary>
    /// A fragment node that groups multiple nodes without a wrapper.
    /// </summary>
    Fragment,
    
    /// <summary>
    /// A clipping node that constrains children to a rectangular area.
    /// </summary>
    Clipping
}

/// <summary>
/// Visitor interface for traversing virtual DOM nodes.
/// </summary>
public interface IVirtualNodeVisitor
{
    void VisitText(TextNode node);
    void VisitElement(ElementNode node);
    void VisitComponent(ComponentNode node);
    void VisitFragment(FragmentNode node);
    void VisitClipping(ClippingNode node);
}