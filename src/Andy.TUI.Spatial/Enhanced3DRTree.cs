using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Spatial;

/// <summary>
/// Enhanced R-Tree implementation with 3D support (x, y, z-index) and optimizations for terminal UI.
/// </summary>
/// <typeparam name="T">The type of elements stored in the tree.</typeparam>
public class Enhanced3DRTree<T> : I3DSpatialIndex<T> where T : class
{
    private RTreeNode<T>? _root;
    private readonly Dictionary<T, RTreeEntry<T>> _elementToEntry = new();
    private readonly OcclusionCalculator _occlusionCalculator = new();

    // Stats
    private int _elementCount;
    private int _nodeCount;

    public int Count => _elementCount;

    public sealed class SpatialInvariantViolationException : Exception
    {
        public SpatialInvariantViolationException(string message) : base(message) { }
    }

    public void Insert(Rectangle bounds, int zIndex, T element)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        Guard(bounds.Width >= 0 && bounds.Height >= 0, $"Cannot insert element with negative size: {bounds}");

        var entry = new RTreeEntry<T>(bounds, zIndex, element);

        // Update mapping
        _elementToEntry[element] = entry;

        if (_root == null)
        {
            _root = new RTreeNode<T>(isLeaf: true);
            _nodeCount = 1;
        }

        // Insert into tree
        var leafNode = ChooseLeaf(_root, bounds);
        leafNode.AddEntry(entry);

        // Handle split if necessary
        if (leafNode.NeedsSplit)
        {
            var newNode = leafNode.Split();
            AdjustTree(leafNode, newNode);
        }
        else
        {
            AdjustTree(leafNode, null);
        }

        _elementCount++;

        ValidateTreeStructure();
    }

    public bool Remove(Rectangle bounds, int zIndex, T element)
    {
        if (!_elementToEntry.TryGetValue(element, out var entry))
            return false;

        // Find and remove from tree
        var removed = RemoveEntry(_root, entry);
        if (removed)
        {
            _elementToEntry.Remove(element);
            _elementCount--;

            // Handle empty root
            if (_root != null && _root.IsLeaf && _root.Entries.Count == 0)
            {
                _root = null;
                _nodeCount = 0;
            }
            ValidateTreeStructure();
        }

        return removed;
    }

    public void Update(Rectangle oldBounds, int oldZ, Rectangle newBounds, int newZ, T element)
    {
        if (!_elementToEntry.TryGetValue(element, out var oldEntry))
            throw new ArgumentException("Element not found in index", nameof(element));

        // Check if only z-index changed
        if (oldBounds.Equals(newBounds) && oldZ != newZ)
        {
            UpdateZIndex(element, oldZ, newZ);
            return;
        }

        // Full update: remove and re-insert
        Remove(oldBounds, oldZ, element);
        Insert(newBounds, newZ, element);
    }

    public void UpdateZIndex(T element, int oldZ, int newZ)
    {
        if (!_elementToEntry.TryGetValue(element, out var entry))
            throw new ArgumentException("Element not found in index", nameof(element));

        // Create new entry with updated z-index
        var newEntry = new RTreeEntry<T>(entry.Bounds, newZ, element);

        // Find leaf containing the entry and update
        UpdateEntryZIndex(_root, entry, newEntry);

        // Update mapping
        _elementToEntry[element] = newEntry;
        ValidateTreeStructure();
    }

    public IEnumerable<T> Query(Rectangle bounds)
    {
        if (_root == null)
            yield break;

        var entries = new List<RTreeEntry<T>>();
        _root.Query(bounds, entries);

        // Return elements sorted by z-index
        foreach (var entry in entries.OrderBy(e => e.ZIndex))
        {
            yield return entry.Element;
        }
    }

    public IEnumerable<T> QueryPoint(int x, int y)
    {
        if (_root == null)
            yield break;

        var entries = new List<RTreeEntry<T>>();
        _root.QueryPoint(x, y, entries);

        // Return elements sorted by z-index (highest first for hit testing)
        foreach (var entry in entries.OrderByDescending(e => e.ZIndex))
        {
            yield return entry.Element;
        }
    }

    public IEnumerable<T> QueryWithZRange(Rectangle bounds, int minZ, int maxZ)
    {
        if (_root == null)
            yield break;

        var entries = new List<RTreeEntry<T>>();
        _root.Query(bounds, entries);

        foreach (var entry in entries.Where(e => e.ZIndex >= minZ && e.ZIndex <= maxZ).OrderBy(e => e.ZIndex))
        {
            yield return entry.Element;
        }
    }

    public IEnumerable<T> QueryVisible(Rectangle viewport)
    {
        if (_root == null)
            yield break;

        // Query all elements in viewport
        var entries = new List<RTreeEntry<T>>();
        _root.Query(viewport, entries);

        // Convert to SpatialElements for occlusion calculation
        var elements = entries.Select(e => new SpatialElement<T>(e.Bounds, e.ZIndex, e.Element)).ToList();

        // Calculate occlusion
        var visibleElements = _occlusionCalculator.CalculateVisible(elements, viewport);

        foreach (var element in visibleElements)
        {
            yield return element.Element;
        }
    }

    public IEnumerable<T> QueryTopmost(int x, int y)
    {
        var elements = QueryPoint(x, y).ToList();
        if (elements.Count > 0)
            yield return elements.First(); // Already sorted by z-index descending
    }

    public IEnumerable<T> FindOccludedBy(T element)
    {
        // Returns elements that are occluded by the provided element (i.e., this element sits above them)
        if (!_elementToEntry.TryGetValue(element, out var entry))
            yield break;

        var candidates = new List<RTreeEntry<T>>();
        _root?.Query(entry.Bounds, candidates);

        foreach (var lower in candidates.Where(e => e.ZIndex < entry.ZIndex))
        {
            // Consider any intersection as occlusion (partial or full)
            if (lower.Bounds.IntersectsWith(entry.Bounds))
                yield return lower.Element;
        }
    }

    public IEnumerable<T> FindOccluding(T element)
    {
        // Returns elements that occlude the provided element (i.e., elements above it)
        if (!_elementToEntry.TryGetValue(element, out var entry))
            yield break;

        var candidates = new List<RTreeEntry<T>>();
        _root?.Query(entry.Bounds, candidates);

        foreach (var higher in candidates.Where(e => e.ZIndex > entry.ZIndex))
        {
            if (higher.Bounds.IntersectsWith(entry.Bounds))
                yield return higher.Element;
        }
    }

    public IEnumerable<T> FindRevealedByMovement(Rectangle oldBounds, Rectangle newBounds, int zIndex)
    {
        // Find elements that were covered by old bounds but not by new bounds
        var revealedArea = oldBounds; // Simplified - should calculate actual revealed region

        var entries = new List<RTreeEntry<T>>();
        _root?.Query(revealedArea, entries);

        foreach (var entry in entries.Where(e => e.ZIndex < zIndex))
        {
            if (entry.Bounds.IntersectsWith(oldBounds) && !entry.Bounds.IntersectsWith(newBounds))
                yield return entry.Element;
        }
    }

    public bool IsCompletelyOccluded(T element)
    {
        if (!_elementToEntry.TryGetValue(element, out var entry))
            return false;

        var candidates = new List<RTreeEntry<T>>();
        _root?.Query(entry.Bounds, candidates);

        // Completely occluded if any higher-z rectangle fully contains it
        return candidates.Any(e => e.ZIndex > entry.ZIndex && e.Bounds.Contains(entry.Bounds));
    }

    public Rectangle? GetVisibleRegion(T element)
    {
        if (!_elementToEntry.TryGetValue(element, out var entry))
            return null;

        // If fully occluded, no visible region
        if (IsCompletelyOccluded(element))
            return null;

        // Compute a simple remaining visible rectangle by subtracting higher-z occluders
        var candidates = new List<RTreeEntry<T>>();
        _root?.Query(entry.Bounds, candidates);

        var occluders = candidates.Where(e => e.ZIndex > entry.ZIndex && e.Bounds.IntersectsWith(entry.Bounds))
                                  .Select(e => e.Bounds)
                                  .ToList();

        if (occluders.Count == 0)
            return entry.Bounds;

        // Start with the full region and iteratively subtract occluders to get uncovered parts
        var uncovered = new List<Rectangle> { entry.Bounds };
        foreach (var occ in occluders)
        {
            var next = new List<Rectangle>();
            foreach (var region in uncovered)
            {
                if (!region.IntersectsWith(occ))
                {
                    next.Add(region);
                }
                else if (!occ.Contains(region))
                {
                    next.AddRange(SubtractRectangle(region, occ));
                }
                // else fully covered, drop region
            }
            uncovered = next;
            if (uncovered.Count == 0)
                break;
        }

        // Return the largest remaining rectangle if any, else null
        return uncovered.Count > 0 ? uncovered.OrderByDescending(r => r.Area).First() : null;
    }

    public void BringToFront(T element)
    {
        if (!_elementToEntry.TryGetValue(element, out var entry))
            return;

        var maxZ = GetMaxZIndex();
        if (entry.ZIndex < maxZ)
        {
            UpdateZIndex(element, entry.ZIndex, maxZ + 1);
        }
    }

    public void SendToBack(T element)
    {
        if (!_elementToEntry.TryGetValue(element, out var entry))
            return;

        var minZ = GetMinZIndex();
        if (entry.ZIndex > minZ)
        {
            UpdateZIndex(element, entry.ZIndex, minZ - 1);
        }
    }

    public void SwapZOrder(T element1, T element2)
    {
        if (!_elementToEntry.TryGetValue(element1, out var entry1) ||
            !_elementToEntry.TryGetValue(element2, out var entry2))
            return;

        var z1 = entry1.ZIndex;
        var z2 = entry2.ZIndex;

        UpdateZIndex(element1, z1, z2);
        UpdateZIndex(element2, z2, z1);
    }

    public int GetMaxZIndex()
    {
        return _elementToEntry.Values.Any() ? _elementToEntry.Values.Max(e => e.ZIndex) : 0;
    }

    public int GetMinZIndex()
    {
        return _elementToEntry.Values.Any() ? _elementToEntry.Values.Min(e => e.ZIndex) : 0;
    }

    public void RecalculateOcclusion()
    {
        // Run a full occlusion pass over all elements to warm any caches/stats
        if (_root == null)
            return;

        var allEntries = _elementToEntry.Values.Select(e => new SpatialElement<T>(e.Bounds, e.ZIndex, e.Element)).ToList();
        // Use a viewport that covers everything present
        var union = ComputeUnionBounds(allEntries.Select(e => e.Bounds));
        _ = _occlusionCalculator.CalculateVisible(allEntries, union).ToList();
    }

    private Rectangle ComputeUnionBounds(IEnumerable<Rectangle> rects)
    {
        var list = rects.ToList();
        if (list.Count == 0)
            return new Rectangle(0, 0, 0, 0);
        var acc = list[0];
        for (int i = 1; i < list.Count; i++)
            acc = Rectangle.Union(acc, list[i]);
        return acc;
    }

    private IEnumerable<Rectangle> SubtractRectangle(Rectangle region, Rectangle occluder)
    {
        var intersection = Rectangle.Intersect(region, occluder);
        if (intersection.IsEmpty)
        {
            yield return region;
            yield break;
        }

        if (intersection.Y > region.Y)
            yield return new Rectangle(region.X, region.Y, region.Width, intersection.Y - region.Y);

        var intersectionBottom = intersection.Y + intersection.Height;
        var regionBottom = region.Y + region.Height;
        if (intersectionBottom < regionBottom)
            yield return new Rectangle(region.X, intersectionBottom, region.Width, regionBottom - intersectionBottom);

        if (intersection.X > region.X)
            yield return new Rectangle(region.X, intersection.Y, intersection.X - region.X, intersection.Height);

        var intersectionRight = intersection.X + intersection.Width;
        var regionRight = region.X + region.Width;
        if (intersectionRight < regionRight)
            yield return new Rectangle(intersectionRight, intersection.Y, regionRight - intersectionRight, intersection.Height);
    }

    public void Rebuild()
    {
        var allEntries = _elementToEntry.Values.ToList();
        Clear();

        foreach (var entry in allEntries)
        {
            Insert(entry.Bounds, entry.ZIndex, entry.Element);
        }
    }

    public SpatialIndexStats GetStatistics()
    {
        return new SpatialIndexStats
        {
            TotalElements = _elementCount,
            TreeDepth = CalculateTreeDepth(_root),
            MinZIndex = GetMinZIndex(),
            MaxZIndex = GetMaxZIndex(),
            UniqueZLevels = _elementToEntry.Values.Select(e => e.ZIndex).Distinct().Count()
        };
    }

    public void Clear()
    {
        _root = null;
        _elementToEntry.Clear();
        _elementCount = 0;
        _nodeCount = 0;
    }

    private static void Guard(bool condition, string message)
    {
        if (!condition) throw new SpatialInvariantViolationException(message);
    }

    private void ValidateTreeStructure()
    {
        if (_root == null) return;
        ValidateNode(_root);
    }

    private void ValidateNode(RTreeNode<T> node)
    {
        Guard(node.MBR.Width >= 0 && node.MBR.Height >= 0, $"Node MBR has negative size at level {node.Level}: {node.MBR}");
        if (node.IsLeaf)
        {
            foreach (var e in node.Entries)
            {
                Guard(e.Bounds.Width >= 0 && e.Bounds.Height >= 0, $"Entry has negative size: {e.Bounds}");
                Guard(node.MBR.Contains(e.Bounds), $"Leaf MBR does not contain entry bounds: NodeMBR={node.MBR}, Entry={e.Bounds}");
            }
        }
        else
        {
            foreach (var c in node.Children)
            {
                Guard(c.Parent == node, "Child parent link inconsistent");
                Guard(node.MBR.Contains(c.MBR), $"Parent MBR does not contain child MBR: Parent={node.MBR}, Child={c.MBR}");
                ValidateNode(c);
            }
        }
    }

    private int CalculateTreeDepth(RTreeNode<T>? node)
    {
        if (node == null)
            return 0;

        if (node.IsLeaf)
            return 1;

        return 1 + node.Children.Max(c => CalculateTreeDepth(c));
    }

    #region Private Helper Methods

    private RTreeNode<T> ChooseLeaf(RTreeNode<T> node, Rectangle bounds)
    {
        if (node.IsLeaf)
            return node;

        return ChooseLeaf(node.ChooseChild(bounds), bounds);
    }

    private void AdjustTree(RTreeNode<T> leafNode, RTreeNode<T>? splitNode)
    {
        var node = leafNode;
        var newNode = splitNode;

        while (node != _root)
        {
            var parent = node.Parent!;

            // Update parent's MBR
            node.UpdateMBR();

            if (newNode != null)
            {
                // Add new node to parent
                parent.AddChild(newNode);

                if (parent.NeedsSplit)
                {
                    newNode = parent.Split();
                    node = parent;
                }
                else
                {
                    newNode = null;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        // Handle root split
        if (node == _root && newNode != null)
        {
            var newRoot = new RTreeNode<T>(isLeaf: false, level: _root.Level + 1);
            newRoot.AddChild(_root);
            newRoot.AddChild(newNode);
            _root = newRoot;
            _nodeCount++;
        }
    }

    private bool RemoveEntry(RTreeNode<T>? node, RTreeEntry<T> entry)
    {
        if (node == null)
            return false;

        if (node.IsLeaf)
        {
            return node.RemoveEntry(entry);
        }

        // Search children
        foreach (var child in node.Children)
        {
            if (child.MBR.IntersectsWith(entry.Bounds))
            {
                if (RemoveEntry(child, entry))
                {
                    // Handle underflow
                    if (child.IsUnderfull && child != _root)
                    {
                        // Reinsert entries
                        node.RemoveChild(child);
                        ReinsertEntries(child);
                    }

                    node.UpdateMBR();
                    return true;
                }
            }
        }

        return false;
    }

    private void ReinsertEntries(RTreeNode<T> node)
    {
        if (node.IsLeaf)
        {
            foreach (var entry in node.Entries)
            {
                Insert(entry.Bounds, entry.ZIndex, entry.Element);
            }
        }
        else
        {
            foreach (var child in node.Children)
            {
                ReinsertEntries(child);
            }
        }
    }

    private bool UpdateEntryZIndex(RTreeNode<T>? node, RTreeEntry<T> oldEntry, RTreeEntry<T> newEntry)
    {
        if (node == null)
            return false;

        if (node.IsLeaf)
        {
            var index = node.Entries.IndexOf(oldEntry);
            if (index >= 0)
            {
                node.Entries[index] = newEntry;
                return true;
            }
            return false;
        }

        // Search children
        foreach (var child in node.Children)
        {
            if (child.MBR.IntersectsWith(oldEntry.Bounds))
            {
                if (UpdateEntryZIndex(child, oldEntry, newEntry))
                    return true;
            }
        }

        return false;
    }

    private IEnumerable<Rectangle> MergeRegions(List<Rectangle> regions)
    {
        if (regions.Count == 0)
            yield break;

        // Simple merge algorithm - can be optimized
        var merged = new List<Rectangle>();

        foreach (var region in regions)
        {
            var wasMerged = false;

            for (int i = 0; i < merged.Count; i++)
            {
                if (merged[i].IntersectsWith(region))
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

        // Second pass to merge any newly overlapping regions
        bool changed;
        do
        {
            changed = false;
            for (int i = 0; i < merged.Count - 1; i++)
            {
                for (int j = i + 1; j < merged.Count; j++)
                {
                    if (merged[i].IntersectsWith(merged[j]))
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

    #endregion
}