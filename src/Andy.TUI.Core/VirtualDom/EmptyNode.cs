using System;

namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Represents an empty virtual DOM node that renders nothing.
/// </summary>
public class EmptyNode : VirtualNode
{
    private static readonly EmptyNode _instance = new();
    private static readonly IReadOnlyList<VirtualNode> _emptyChildren = Array.Empty<VirtualNode>();
    
    /// <summary>
    /// Gets the singleton instance of EmptyNode.
    /// </summary>
    public static EmptyNode Instance => _instance;
    
    public override VirtualNodeType Type => VirtualNodeType.Fragment; // Treat as empty fragment
    
    public override IReadOnlyList<VirtualNode> Children => _emptyChildren;
    
    /// <summary>
    /// Creates a new empty node.
    /// </summary>
    public EmptyNode()
    {
    }
    
    public override bool Equals(VirtualNode? other) => other is EmptyNode;
    
    public override int GetHashCode() => 0;
    
    public override VirtualNode Clone() => this; // EmptyNode is immutable
    
    public override void Accept(IVirtualNodeVisitor visitor)
    {
        // Empty node doesn't need to be visited
        visitor.VisitFragment(new FragmentNode(Array.Empty<VirtualNode>()));
    }
    
    public override string ToString() => "EmptyNode";
}