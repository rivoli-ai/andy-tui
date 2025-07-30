using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// A virtual DOM node that clips its children to a specific rectangular area.
/// </summary>
public class ClippingNode : VirtualNode
{
    public override VirtualNodeType Type => VirtualNodeType.Clipping;
    /// <summary>
    /// The x-coordinate of the clipping rectangle.
    /// </summary>
    public int X { get; }
    
    /// <summary>
    /// The y-coordinate of the clipping rectangle.
    /// </summary>
    public int Y { get; }
    
    /// <summary>
    /// The width of the clipping rectangle.
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// The height of the clipping rectangle.
    /// </summary>
    public int Height { get; }
    
    /// <summary>
    /// The children to be clipped.
    /// </summary>
    public override IReadOnlyList<VirtualNode> Children { get; }
    
    /// <summary>
    /// Creates a new clipping node.
    /// </summary>
    public ClippingNode(int x, int y, int width, int height, params VirtualNode[] children)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Children = children?.ToList() ?? new List<VirtualNode>();
    }
    
    public override string ToString() => $"ClippingNode({X},{Y},{Width}x{Height})";
    
    public override bool Equals(VirtualNode? other)
    {
        return other is ClippingNode clip &&
               X == clip.X &&
               Y == clip.Y &&
               Width == clip.Width &&
               Height == clip.Height &&
               Children.SequenceEqual(clip.Children);
    }
    
    public override int GetHashCode()
    {
        var hash = HashCode.Combine(X, Y, Width, Height);
        foreach (var child in Children)
        {
            hash = HashCode.Combine(hash, child.GetHashCode());
        }
        return hash;
    }
    
    public override VirtualNode Clone()
    {
        return new ClippingNode(X, Y, Width, Height, Children.Select(c => c.Clone()).ToArray());
    }
    
    public override void Accept(IVirtualNodeVisitor visitor)
    {
        visitor.VisitClipping(this);
    }
}