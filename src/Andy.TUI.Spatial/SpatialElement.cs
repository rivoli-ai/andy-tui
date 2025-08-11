using System;
using System.Collections.Generic;

namespace Andy.TUI.Spatial;

/// <summary>
/// Represents an element in the spatial index with position, size, and z-index information.
/// </summary>
/// <typeparam name="T">The type of the element being indexed.</typeparam>
public class SpatialElement<T> : IEquatable<SpatialElement<T>>
{
    /// <summary>
    /// Gets the spatial bounds of the element.
    /// </summary>
    public Rectangle Bounds { get; set; }

    /// <summary>
    /// Gets the absolute z-index of the element for rendering order.
    /// </summary>
    public int ZIndex { get; set; }

    /// <summary>
    /// Gets the element being indexed.
    /// </summary>
    public T Element { get; }

    /// <summary>
    /// Gets or sets whether this element is completely occluded by others.
    /// Used for rendering optimization.
    /// </summary>
    public bool IsFullyOccluded { get; set; }

    /// <summary>
    /// Gets the set of elements that occlude this one.
    /// </summary>
    public HashSet<SpatialElement<T>> OccludedBy { get; } = new();

    /// <summary>
    /// Gets the set of elements that this one occludes.
    /// </summary>
    public HashSet<SpatialElement<T>> Occludes { get; } = new();

    /// <summary>
    /// Gets a unique identifier for this spatial element.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    public SpatialElement(Rectangle bounds, int zIndex, T element)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Bounds = bounds;
        ZIndex = zIndex;
    }

    /// <summary>
    /// Checks if this element completely occludes another element.
    /// </summary>
    public bool CompletelyOccludes(SpatialElement<T> other)
    {
        return ZIndex > other.ZIndex && Bounds.Contains(other.Bounds);
    }

    /// <summary>
    /// Checks if this element partially occludes another element.
    /// </summary>
    public bool PartiallyOccludes(SpatialElement<T> other)
    {
        return ZIndex > other.ZIndex &&
               Bounds.IntersectsWith(other.Bounds) &&
               !Bounds.Contains(other.Bounds);
    }

    /// <summary>
    /// Gets the visible region of this element considering occlusion.
    /// Returns null if completely occluded.
    /// </summary>
    public Rectangle? GetVisibleRegion()
    {
        if (IsFullyOccluded)
            return null;

        // TODO: Implement partial occlusion calculation
        // For now, return full bounds if not fully occluded
        return Bounds;
    }

    public bool Equals(SpatialElement<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SpatialElement<T>);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"SpatialElement[{Element}, Bounds={Bounds}, Z={ZIndex}, Occluded={IsFullyOccluded}]";
    }
}