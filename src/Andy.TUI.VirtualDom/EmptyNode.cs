using System;
using System.Collections.Generic;

namespace Andy.TUI.VirtualDom;

public class EmptyNode : VirtualNode
{
    private static readonly EmptyNode _instance = new();
    private static readonly IReadOnlyList<VirtualNode> _emptyChildren = Array.Empty<VirtualNode>();
    public static EmptyNode Instance => _instance;
    public override VirtualNodeType Type => VirtualNodeType.Empty;
    public override IReadOnlyList<VirtualNode> Children => _emptyChildren;
    public EmptyNode() { }
    public override bool Equals(VirtualNode? other) => other is EmptyNode;
    public override int GetHashCode() => 0;
    public override VirtualNode Clone() => this;
    public override void Accept(IVirtualNodeVisitor visitor) => visitor.VisitEmpty(this);
}


