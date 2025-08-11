using System;
using System.Collections.Generic;

namespace Andy.TUI.VirtualDom;

public abstract class VirtualNode : IEquatable<VirtualNode>
{
    public abstract VirtualNodeType Type { get; }
    public string? Key { get; init; }
    public Dictionary<string, object?> Props { get; init; } = new();
    public virtual IReadOnlyList<VirtualNode> Children => Array.Empty<VirtualNode>();
    public abstract bool Equals(VirtualNode? other);
    public override bool Equals(object? obj) => obj is VirtualNode other && Equals(other);
    public abstract override int GetHashCode();
    public abstract VirtualNode Clone();
    public abstract void Accept(IVirtualNodeVisitor visitor);
}

public enum VirtualNodeType { Text, Element, Component, Fragment, Clipping, Empty }

public interface IVirtualNodeVisitor
{
    void VisitText(TextNode node);
    void VisitElement(ElementNode node);
    void VisitComponent(ComponentNode node);
    void VisitFragment(FragmentNode node);
    void VisitClipping(ClippingNode node);
    void VisitEmpty(EmptyNode node);
}


