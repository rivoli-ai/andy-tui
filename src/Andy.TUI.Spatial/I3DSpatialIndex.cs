using System.Collections.Generic;

namespace Andy.TUI.Spatial;

/// <summary>
/// Enhanced spatial index interface with z-index support for 3D UI element management.
/// </summary>
/// <typeparam name="T">The type of elements stored in the index.</typeparam>
public interface I3DSpatialIndex<T>
{
    // Core operations with z-index
    void Insert(Rectangle bounds, int zIndex, T element);
    bool Remove(Rectangle bounds, int zIndex, T element);
    void Update(Rectangle oldBounds, int oldZ, Rectangle newBounds, int newZ, T element);
    
    // Z-only update operation (no position change)
    void UpdateZIndex(T element, int oldZ, int newZ);
    
    // 2D spatial queries (ignoring z-index)
    IEnumerable<T> Query(Rectangle region);
    IEnumerable<T> QueryPoint(int x, int y);
    
    // 3D queries with z-index awareness
    IEnumerable<T> QueryWithZRange(Rectangle region, int minZ, int maxZ);
    IEnumerable<T> QueryVisible(Rectangle region); // Only non-occluded elements
    IEnumerable<T> QueryTopmost(int x, int y); // Highest z-index at point
    
    // Occlusion analysis
    IEnumerable<T> FindOccludedBy(T element);
    IEnumerable<T> FindOccluding(T element);
    IEnumerable<T> FindRevealedByMovement(Rectangle oldBounds, Rectangle newBounds, int zIndex);
    bool IsCompletelyOccluded(T element);
    Rectangle? GetVisibleRegion(T element);
    
    // Z-order operations
    void BringToFront(T element);
    void SendToBack(T element);
    void SwapZOrder(T element1, T element2);
    int GetMaxZIndex();
    int GetMinZIndex();
    
    // Performance operations
    void RecalculateOcclusion();
    void Clear();
    void Rebuild();
    int Count { get; }
    
    // Debugging and statistics
    SpatialIndexStats GetStatistics();
}

/// <summary>
/// Statistics about the spatial index for performance monitoring.
/// </summary>
public class SpatialIndexStats
{
    public int TotalElements { get; set; }
    public int FullyOccludedElements { get; set; }
    public int PartiallyOccludedElements { get; set; }
    public int TreeDepth { get; set; }
    public double AverageNodeOccupancy { get; set; }
    public int MinZIndex { get; set; }
    public int MaxZIndex { get; set; }
    public int UniqueZLevels { get; set; }
}