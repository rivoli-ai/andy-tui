using System.Collections.Generic;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Spatial;

/// <summary>
/// Represents a 2D rectangle for spatial indexing operations.
/// </summary>
public readonly struct Rectangle
{
    public int X { get; init; }
    public int Y { get; init; }  
    public int Width { get; init; }
    public int Height { get; init; }
    
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public bool IsEmpty => Width == 0 || Height == 0;
    
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    /// <summary>
    /// Checks if this rectangle intersects with another rectangle.
    /// </summary>
    public bool IntersectsWith(Rectangle other)
    {
        return X < other.Right && Right > other.X && 
               Y < other.Bottom && Bottom > other.Y;
    }
    
    /// <summary>
    /// Checks if this rectangle completely contains another rectangle.
    /// </summary>
    public bool Contains(Rectangle other)
    {
        return X <= other.X && Y <= other.Y && 
               Right >= other.Right && Bottom >= other.Bottom;
    }
    
    /// <summary>
    /// Checks if this rectangle contains the specified point.
    /// </summary>
    public bool Contains(int x, int y)
    {
        return x >= X && x < Right && y >= Y && y < Bottom;
    }
    
    /// <summary>
    /// Returns the union of two rectangles (smallest rectangle containing both).
    /// </summary>
    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        int minX = Math.Min(a.X, b.X);
        int minY = Math.Min(a.Y, b.Y);
        int maxX = Math.Max(a.Right, b.Right);
        int maxY = Math.Max(a.Bottom, b.Bottom);
        
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }
    
    /// <summary>
    /// Calculates the area of this rectangle.
    /// </summary>
    public int Area => Width * Height;
    
    /// <summary>
    /// Returns the intersection of two rectangles, or empty if they don't intersect.
    /// </summary>
    public static Rectangle Intersect(Rectangle a, Rectangle b)
    {
        int left = Math.Max(a.X, b.X);
        int top = Math.Max(a.Y, b.Y);
        int right = Math.Min(a.Right, b.Right);
        int bottom = Math.Min(a.Bottom, b.Bottom);
        
        if (right <= left || bottom <= top)
            return new Rectangle(0, 0, 0, 0); // Empty rectangle
            
        return new Rectangle(left, top, right - left, bottom - top);
    }
    
    public override string ToString() => $"({X},{Y},{Width}x{Height})";
    
    public override bool Equals(object? obj) => obj is Rectangle r && Equals(r);
    
    public bool Equals(Rectangle other) =>
        X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    
    public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);
    public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);
}

/// <summary>
/// Interface for spatial indexing data structures that efficiently manage
/// 2D rectangular regions for UI element positioning and overlap detection.
/// </summary>
public interface ISpatialIndex<T>
{
    /// <summary>
    /// Inserts an element with its bounding rectangle into the spatial index.
    /// </summary>
    void Insert(Rectangle bounds, T element);
    
    /// <summary>
    /// Removes a specific element with its bounding rectangle from the spatial index.
    /// </summary>
    bool Remove(Rectangle bounds, T element);
    
    /// <summary>
    /// Updates an element's position from old bounds to new bounds.
    /// Equivalent to Remove(oldBounds, element) + Insert(newBounds, element) but more efficient.
    /// </summary>
    void Update(Rectangle oldBounds, Rectangle newBounds, T element);
    
    /// <summary>
    /// Queries all elements that intersect with the specified rectangular region.
    /// </summary>
    IEnumerable<T> Query(Rectangle region);
    
    /// <summary>
    /// Queries all elements that contain the specified point.
    /// </summary>
    IEnumerable<T> QueryPoint(int x, int y);
    
    /// <summary>
    /// Queries all elements that intersect with the specified rectangular region.
    /// Alias for Query() for clarity in overlap detection scenarios.
    /// </summary>
    IEnumerable<T> QueryIntersecting(Rectangle region) => Query(region);
    
    /// <summary>
    /// Removes all elements from the spatial index.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Gets the total number of elements in the spatial index.
    /// </summary>
    int Count { get; }
    
    /// <summary>
    /// Rebuilds the spatial index for optimal performance.
    /// Call this after bulk insertions/deletions.
    /// </summary>
    void Rebuild();
}