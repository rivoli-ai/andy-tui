using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.VirtualDom;

public sealed class FragmentNode : VirtualNode
{
    private readonly List<VirtualNode> _children;
    public override VirtualNodeType Type => VirtualNodeType.Fragment;
    public override IReadOnlyList<VirtualNode> Children => _children.AsReadOnly();

    public FragmentNode(params VirtualNode[] children) { _children = new List<VirtualNode>(children ?? Array.Empty<VirtualNode>()); }
    public FragmentNode(IEnumerable<VirtualNode> children) { _children = new List<VirtualNode>(children ?? Array.Empty<VirtualNode>()); }

    public void AddChild(VirtualNode child) { ArgumentNullException.ThrowIfNull(child); _children.Add(child); }
    public bool RemoveChild(VirtualNode child) { ArgumentNullException.ThrowIfNull(child); return _children.Remove(child); }

    public override bool Equals(VirtualNode? other)
    {
        if (other is not FragmentNode fragment) return false;
        if (Key != fragment.Key) return false;
        if (Children.Count != fragment.Children.Count) return false;
        for (int i = 0; i < Children.Count; i++) { if (!Children[i].Equals(fragment.Children[i])) return false; }
        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type); hash.Add(Key); hash.Add(Children.Count);
        return hash.ToHashCode();
    }

    public override VirtualNode Clone()
    {
        var clonedChildren = _children.Select(c => c.Clone()).ToArray();
        return new FragmentNode(clonedChildren) { Key = Key };
    }

    public override void Accept(IVirtualNodeVisitor visitor) => visitor.VisitFragment(this);

    public override string ToString()
    {
        return $"Fragment: {Children.Count} children";
    }
}


