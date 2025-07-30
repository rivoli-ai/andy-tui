using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Terminal.Rendering;

/// <summary>
/// Tracks dirty regions for efficient terminal updates.
/// </summary>
public class DirtyRegionTracker
{
    private readonly List<Rectangle> _dirtyRegions = new();
    
    /// <summary>
    /// Marks a region as dirty (needs redrawing).
    /// </summary>
    public void MarkDirty(Rectangle region)
    {
        if (region.Width <= 0 || region.Height <= 0)
            return;
        
        // Try to merge with existing regions
        var merged = false;
        for (int i = 0; i < _dirtyRegions.Count; i++)
        {
            var existing = _dirtyRegions[i];
            if (existing.IntersectsWith(region) || existing.Adjacent(region))
            {
                _dirtyRegions[i] = existing.Union(region);
                merged = true;
                break;
            }
        }
        
        if (!merged)
        {
            _dirtyRegions.Add(region);
        }
        
        // Merge overlapping regions
        MergeOverlappingRegions();
    }
    
    /// <summary>
    /// Gets all dirty regions that need to be redrawn.
    /// </summary>
    public IReadOnlyList<Rectangle> GetDirtyRegions()
    {
        return _dirtyRegions.AsReadOnly();
    }
    
    /// <summary>
    /// Clears all dirty regions.
    /// </summary>
    public void Clear()
    {
        _dirtyRegions.Clear();
    }
    
    /// <summary>
    /// Marks the entire screen as dirty.
    /// </summary>
    public void MarkAllDirty(int width, int height)
    {
        _dirtyRegions.Clear();
        _dirtyRegions.Add(new Rectangle(0, 0, width, height));
    }
    
    private void MergeOverlappingRegions()
    {
        bool merged;
        do
        {
            merged = false;
            for (int i = 0; i < _dirtyRegions.Count - 1; i++)
            {
                for (int j = i + 1; j < _dirtyRegions.Count; j++)
                {
                    if (_dirtyRegions[i].IntersectsWith(_dirtyRegions[j]) || 
                        _dirtyRegions[i].Adjacent(_dirtyRegions[j]))
                    {
                        _dirtyRegions[i] = _dirtyRegions[i].Union(_dirtyRegions[j]);
                        _dirtyRegions.RemoveAt(j);
                        merged = true;
                        break;
                    }
                }
                if (merged) break;
            }
        } while (merged);
    }
}

/// <summary>
/// Represents a rectangular region.
/// </summary>
public readonly struct Rectangle : IEquatable<Rectangle>
{
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    
    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;
    
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    /// <summary>
    /// Checks if this rectangle intersects with another.
    /// </summary>
    public bool IntersectsWith(Rectangle other)
    {
        return Left < other.Right && Right > other.Left &&
               Top < other.Bottom && Bottom > other.Top;
    }
    
    /// <summary>
    /// Checks if this rectangle is adjacent to another (touching but not overlapping).
    /// </summary>
    public bool Adjacent(Rectangle other)
    {
        // Horizontally adjacent
        if ((Right == other.Left || Left == other.Right) &&
            Top < other.Bottom && Bottom > other.Top)
            return true;
        
        // Vertically adjacent
        if ((Bottom == other.Top || Top == other.Bottom) &&
            Left < other.Right && Right > other.Left)
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Returns the union of two rectangles (smallest rectangle containing both).
    /// </summary>
    public Rectangle Union(Rectangle other)
    {
        var left = Math.Min(Left, other.Left);
        var top = Math.Min(Top, other.Top);
        var right = Math.Max(Right, other.Right);
        var bottom = Math.Max(Bottom, other.Bottom);
        
        return new Rectangle(left, top, right - left, bottom - top);
    }
    
    /// <summary>
    /// Returns the intersection of two rectangles.
    /// </summary>
    public Rectangle Intersect(Rectangle other)
    {
        var left = Math.Max(Left, other.Left);
        var top = Math.Max(Top, other.Top);
        var right = Math.Min(Right, other.Right);
        var bottom = Math.Min(Bottom, other.Bottom);
        
        if (right > left && bottom > top)
            return new Rectangle(left, top, right - left, bottom - top);
        
        return Empty;
    }
    
    /// <summary>
    /// Checks if this rectangle contains a point.
    /// </summary>
    public bool Contains(int x, int y)
    {
        return x >= Left && x < Right && y >= Top && y < Bottom;
    }
    
    /// <summary>
    /// Checks if this rectangle contains another rectangle.
    /// </summary>
    public bool Contains(Rectangle other)
    {
        return Left <= other.Left && Right >= other.Right &&
               Top <= other.Top && Bottom >= other.Bottom;
    }
    
    public bool IsEmpty => Width <= 0 || Height <= 0;
    
    public static Rectangle Empty => new(0, 0, 0, 0);
    
    public override bool Equals(object? obj)
    {
        return obj is Rectangle other && Equals(other);
    }
    
    public bool Equals(Rectangle other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }
    
    public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !left.Equals(right);
    }
    
    public override string ToString()
    {
        return $"Rectangle({X}, {Y}, {Width}, {Height})";
    }
}