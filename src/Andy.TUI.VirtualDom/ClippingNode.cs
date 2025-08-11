using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.VirtualDom;

public class ClippingNode : VirtualNode
{
    public override VirtualNodeType Type => VirtualNodeType.Clipping;
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public override IReadOnlyList<VirtualNode> Children { get; }

    public ClippingNode(int x, int y, int width, int height, params VirtualNode[] children)
    {
        X = x; Y = y; Width = width; Height = height;
        Children = children?.ToList() ?? new List<VirtualNode>();
    }

    public override bool Equals(VirtualNode? other)
    {
        return other is ClippingNode clip && X == clip.X && Y == clip.Y && Width == clip.Width && Height == clip.Height && Children.SequenceEqual(clip.Children);
    }

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(X, Y, Width, Height);
        foreach (var child in Children) hash = HashCode.Combine(hash, child.GetHashCode());
        return hash;
    }

    public override VirtualNode Clone() => new ClippingNode(X, Y, Width, Height, Children.Select(c => c.Clone()).ToArray());
    public override void Accept(IVirtualNodeVisitor visitor) => visitor.VisitClipping(this);
}


