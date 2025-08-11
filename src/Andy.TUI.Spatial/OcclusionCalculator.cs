using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Spatial;

/// <summary>
/// Calculates occlusion and visibility for spatial elements based on their z-index and bounds.
/// </summary>
public class OcclusionCalculator
{
    /// <summary>
    /// Calculates which elements are visible given their z-order and overlaps.
    /// </summary>
    public IEnumerable<SpatialElement<T>> CalculateVisible<T>(
        IEnumerable<SpatialElement<T>> elements, 
        Rectangle viewport) where T : class
    {
        // Sort by z-index (highest first)
        var sortedElements = elements.OrderByDescending(e => e.ZIndex).ToList();
        
        // Track covered regions at each z-level
        var occlusionMap = new List<(Rectangle bounds, int zIndex)>();
        
        foreach (var element in sortedElements)
        {
            // Check if element is within viewport
            if (!element.Bounds.IntersectsWith(viewport))
                continue;
                
            // Check if element is fully occluded by higher z-index elements
            var isFullyOccluded = IsFullyOccluded(element.Bounds, element.ZIndex, occlusionMap);
            element.IsFullyOccluded = isFullyOccluded;
            
            if (!isFullyOccluded)
            {
                // Element is at least partially visible
                yield return element;
                
                // Add to occlusion map for lower elements
                occlusionMap.Add((element.Bounds, element.ZIndex));
            }
        }
    }
    
    /// <summary>
    /// Checks if a rectangle at a given z-index is fully occluded by higher elements.
    /// </summary>
    private bool IsFullyOccluded(Rectangle bounds, int zIndex, List<(Rectangle bounds, int zIndex)> occlusionMap)
    {
        // Get all occluders with higher z-index
        var occluders = occlusionMap
            .Where(o => o.zIndex > zIndex)
            .Select(o => o.bounds)
            .ToList();
            
        if (occluders.Count == 0)
            return false;
            
        // Check if bounds is fully covered by occluders
        return IsFullyCovered(bounds, occluders);
    }
    
    /// <summary>
    /// Checks if a rectangle is fully covered by a set of occluding rectangles.
    /// </summary>
    private bool IsFullyCovered(Rectangle target, List<Rectangle> occluders)
    {
        // Simple algorithm: check if every point in target is covered
        // For terminal UI, we can check character-by-character
        
        // First, quick check if any single occluder fully contains target
        if (occluders.Any(o => o.Contains(target)))
            return true;
            
        // More complex check: divide target into cells and check coverage
        var uncoveredRegions = new List<Rectangle> { target };
        
        foreach (var occluder in occluders)
        {
            var newUncovered = new List<Rectangle>();
            
            foreach (var region in uncoveredRegions)
            {
                if (!occluder.IntersectsWith(region))
                {
                    // Region remains uncovered
                    newUncovered.Add(region);
                }
                else if (!occluder.Contains(region))
                {
                    // Partially covered - split into uncovered parts
                    newUncovered.AddRange(SubtractRectangle(region, occluder));
                }
                // Else: region is fully covered, don't add to newUncovered
            }
            
            uncoveredRegions = newUncovered;
            
            // Early exit if everything is covered
            if (uncoveredRegions.Count == 0)
                return true;
        }
        
        return uncoveredRegions.Count == 0;
    }
    
    /// <summary>
    /// Subtracts an occluder from a region, returning the uncovered parts.
    /// </summary>
    private IEnumerable<Rectangle> SubtractRectangle(Rectangle region, Rectangle occluder)
    {
        var intersection = Rectangle.Intersect(region, occluder);
        
        if (intersection.IsEmpty)
        {
            yield return region;
            yield break;
        }
        
        // Top part (above intersection)
        if (intersection.Y > region.Y)
        {
            yield return new Rectangle(
                region.X, 
                region.Y, 
                region.Width, 
                intersection.Y - region.Y
            );
        }
        
        // Bottom part (below intersection)
        var intersectionBottom = intersection.Y + intersection.Height;
        var regionBottom = region.Y + region.Height;
        if (intersectionBottom < regionBottom)
        {
            yield return new Rectangle(
                region.X, 
                intersectionBottom, 
                region.Width, 
                regionBottom - intersectionBottom
            );
        }
        
        // Left part (left of intersection)
        if (intersection.X > region.X)
        {
            yield return new Rectangle(
                region.X, 
                intersection.Y, 
                intersection.X - region.X, 
                intersection.Height
            );
        }
        
        // Right part (right of intersection)
        var intersectionRight = intersection.X + intersection.Width;
        var regionRight = region.X + region.Width;
        if (intersectionRight < regionRight)
        {
            yield return new Rectangle(
                intersectionRight, 
                intersection.Y, 
                regionRight - intersectionRight, 
                intersection.Height
            );
        }
    }
    
    /// <summary>
    /// Calculates dirty regions when elements change, considering occlusion.
    /// </summary>
    public IEnumerable<Rectangle> CalculateDirtyRegions<T>(
        IEnumerable<SpatialElement<T>> allElements,
        IEnumerable<(T element, Rectangle oldBounds, int oldZ, Rectangle newBounds, int newZ)> changes) 
        where T : class
    {
        var dirtyRegions = new List<Rectangle>();
        var elementsDict = allElements.ToDictionary(e => e.Element);
        
        foreach (var (element, oldBounds, oldZ, newBounds, newZ) in changes)
        {
            // Always mark old position as dirty
            dirtyRegions.Add(oldBounds);
            
            // Always mark new position as dirty
            dirtyRegions.Add(newBounds);
            
            // Find elements that might be affected by z-index changes
            if (oldZ != newZ)
            {
                // Elements between old and new z-indices might need redrawing
                var minZ = Math.Min(oldZ, newZ);
                var maxZ = Math.Max(oldZ, newZ);
                
                foreach (var other in allElements)
                {
                    if (other.Element.Equals(element))
                        continue;
                        
                    if (other.ZIndex >= minZ && other.ZIndex <= maxZ)
                    {
                        // Check if this element overlaps with either old or new bounds
                        if (other.Bounds.IntersectsWith(oldBounds) || 
                            other.Bounds.IntersectsWith(newBounds))
                        {
                            dirtyRegions.Add(other.Bounds);
                        }
                    }
                }
            }
            
            // Find elements that were revealed by movement
            var revealedArea = oldBounds;
            if (!newBounds.Contains(oldBounds))
            {
                // Some area was vacated - check what's underneath
                foreach (var other in allElements.Where(e => e.ZIndex < oldZ))
                {
                    if (other.Bounds.IntersectsWith(revealedArea))
                    {
                        dirtyRegions.Add(Rectangle.Intersect(other.Bounds, revealedArea));
                    }
                }
            }
        }
        
        // Merge overlapping regions
        return MergeRegions(dirtyRegions);
    }
    
    private IEnumerable<Rectangle> MergeRegions(List<Rectangle> regions)
    {
        if (regions.Count == 0)
            yield break;
            
        // Remove duplicates
        regions = regions.Distinct().ToList();
        
        // Simple merge algorithm
        var merged = new List<Rectangle>();
        
        foreach (var region in regions)
        {
            var wasMerged = false;
            
            for (int i = 0; i < merged.Count; i++)
            {
                // Check if regions overlap or are adjacent
                if (AreAdjacent(merged[i], region) || merged[i].IntersectsWith(region))
                {
                    merged[i] = Rectangle.Union(merged[i], region);
                    wasMerged = true;
                    break;
                }
            }
            
            if (!wasMerged)
            {
                merged.Add(region);
            }
        }
        
        // Multiple passes until no more merges
        bool changed;
        do
        {
            changed = false;
            for (int i = 0; i < merged.Count - 1; i++)
            {
                for (int j = i + 1; j < merged.Count; j++)
                {
                    if (AreAdjacent(merged[i], merged[j]) || merged[i].IntersectsWith(merged[j]))
                    {
                        merged[i] = Rectangle.Union(merged[i], merged[j]);
                        merged.RemoveAt(j);
                        changed = true;
                        break;
                    }
                }
                if (changed) break;
            }
        } while (changed);
        
        foreach (var region in merged)
        {
            yield return region;
        }
    }
    
    private bool AreAdjacent(Rectangle r1, Rectangle r2)
    {
        // Check if rectangles share an edge
        var r1Right = r1.X + r1.Width;
        var r1Bottom = r1.Y + r1.Height;
        var r2Right = r2.X + r2.Width;
        var r2Bottom = r2.Y + r2.Height;
        
        // Horizontally adjacent
        if ((r1Right == r2.X || r2Right == r1.X) &&
            !(r1Bottom <= r2.Y || r2Bottom <= r1.Y))
        {
            return true;
        }
        
        // Vertically adjacent
        if ((r1Bottom == r2.Y || r2Bottom == r1.Y) &&
            !(r1Right <= r2.X || r2Right <= r1.X))
        {
            return true;
        }
        
        return false;
    }
}