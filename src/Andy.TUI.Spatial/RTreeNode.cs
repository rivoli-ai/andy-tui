using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Spatial;

/// <summary>
/// Represents a node in the R-Tree spatial index.
/// </summary>
/// <typeparam name="T">The type of elements stored in the tree.</typeparam>
internal class RTreeNode<T>
{
    private const int MinChildren = 2;
    private const int MaxChildren = 4; // Keep small for terminal UI use case

    /// <summary>
    /// Gets the bounding rectangle that contains all children.
    /// </summary>
    public Rectangle MBR { get; private set; } // Minimum Bounding Rectangle

    /// <summary>
    /// Gets whether this is a leaf node.
    /// </summary>
    public bool IsLeaf { get; }

    /// <summary>
    /// Gets the child nodes (for internal nodes).
    /// </summary>
    public List<RTreeNode<T>> Children { get; } = new();

    /// <summary>
    /// Gets the entries (for leaf nodes).
    /// </summary>
    public List<RTreeEntry<T>> Entries { get; } = new();

    /// <summary>
    /// Gets the parent node.
    /// </summary>
    public RTreeNode<T>? Parent { get; set; }

    /// <summary>
    /// Gets the level of this node (0 for leaves).
    /// </summary>
    public int Level { get; }

    public RTreeNode(bool isLeaf, int level = 0)
    {
        IsLeaf = isLeaf;
        Level = level;
        MBR = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);
    }

    /// <summary>
    /// Adds a child node (for internal nodes).
    /// </summary>
    public void AddChild(RTreeNode<T> child)
    {
        if (IsLeaf)
            throw new InvalidOperationException("Cannot add child nodes to a leaf node.");

        Children.Add(child);
        child.Parent = this;
        UpdateMBR();
    }

    /// <summary>
    /// Adds an entry (for leaf nodes).
    /// </summary>
    public void AddEntry(RTreeEntry<T> entry)
    {
        if (!IsLeaf)
            throw new InvalidOperationException("Cannot add entries to an internal node.");

        Entries.Add(entry);
        UpdateMBR();
    }

    /// <summary>
    /// Removes a child node.
    /// </summary>
    public bool RemoveChild(RTreeNode<T> child)
    {
        var removed = Children.Remove(child);
        if (removed)
        {
            child.Parent = null;
            UpdateMBR();
        }
        return removed;
    }

    /// <summary>
    /// Removes an entry.
    /// </summary>
    public bool RemoveEntry(RTreeEntry<T> entry)
    {
        var removed = Entries.Remove(entry);
        if (removed)
            UpdateMBR();
        return removed;
    }

    /// <summary>
    /// Checks if the node needs splitting.
    /// </summary>
    public bool NeedsSplit => IsLeaf ? Entries.Count > MaxChildren : Children.Count > MaxChildren;

    /// <summary>
    /// Checks if the node is underfull after deletion.
    /// </summary>
    public bool IsUnderfull => IsLeaf ? Entries.Count < MinChildren : Children.Count < MinChildren;

    /// <summary>
    /// Updates the minimum bounding rectangle to contain all children/entries.
    /// </summary>
    public void UpdateMBR()
    {
        if (IsLeaf)
        {
            if (Entries.Count == 0)
            {
                MBR = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);
                return;
            }

            MBR = Entries[0].Bounds;
            for (int i = 1; i < Entries.Count; i++)
            {
                MBR = Rectangle.Union(MBR, Entries[i].Bounds);
            }
        }
        else
        {
            if (Children.Count == 0)
            {
                MBR = new Rectangle(int.MaxValue, int.MaxValue, 0, 0);
                return;
            }

            MBR = Children[0].MBR;
            for (int i = 1; i < Children.Count; i++)
            {
                MBR = Rectangle.Union(MBR, Children[i].MBR);
            }
        }

        // Propagate MBR update to parent
        Parent?.UpdateMBR();
    }

    /// <summary>
    /// Finds all entries that intersect with the given rectangle.
    /// </summary>
    public void Query(Rectangle searchRect, List<RTreeEntry<T>> results)
    {
        if (!MBR.IntersectsWith(searchRect))
            return;

        if (IsLeaf)
        {
            foreach (var entry in Entries)
            {
                if (entry.Bounds.IntersectsWith(searchRect))
                    results.Add(entry);
            }
        }
        else
        {
            foreach (var child in Children)
            {
                child.Query(searchRect, results);
            }
        }
    }

    /// <summary>
    /// Finds all entries at the given point.
    /// </summary>
    public void QueryPoint(int x, int y, List<RTreeEntry<T>> results)
    {
        if (!MBR.Contains(x, y))
            return;

        if (IsLeaf)
        {
            foreach (var entry in Entries)
            {
                if (entry.Bounds.Contains(x, y))
                    results.Add(entry);
            }
        }
        else
        {
            foreach (var child in Children)
            {
                child.QueryPoint(x, y, results);
            }
        }
    }

    /// <summary>
    /// Chooses the best child node for inserting a new rectangle.
    /// </summary>
    public RTreeNode<T> ChooseChild(Rectangle bounds)
    {
        if (IsLeaf)
            throw new InvalidOperationException("Cannot choose child from a leaf node.");

        // If the new bounds does not intersect any child MBRs, choose the child
        // with the smallest area (helps reduce overall tree growth when far away).
        var intersectingChildren = Children.Where(c => c.MBR.IntersectsWith(bounds)).ToList();
        if (intersectingChildren.Count == 0)
        {
            return Children.OrderBy(c => c.MBR.Area).First();
        }

        // Otherwise, follow classic R-Tree heuristic: minimal area enlargement,
        // breaking ties by choosing the smallest area.
        RTreeNode<T>? bestChild = null;
        int minEnlargement = int.MaxValue;
        int minArea = int.MaxValue;

        foreach (var child in Children)
        {
            var enlarged = Rectangle.Union(child.MBR, bounds);
            var enlargement = enlarged.Area - child.MBR.Area;

            if (enlargement < minEnlargement ||
                (enlargement == minEnlargement && child.MBR.Area < minArea))
            {
                minEnlargement = enlargement;
                minArea = child.MBR.Area;
                bestChild = child;
            }
        }

        return bestChild ?? Children[0];
    }

    /// <summary>
    /// Splits this node into two nodes using the quadratic split algorithm.
    /// </summary>
    public RTreeNode<T> Split()
    {
        var newNode = new RTreeNode<T>(IsLeaf, Level);

        if (IsLeaf)
        {
            // Split entries
            var allEntries = new List<RTreeEntry<T>>(Entries);
            Entries.Clear();

            // Find two entries with maximum separation
            var (e1, e2) = PickSeeds(allEntries);
            AddEntry(e1);
            newNode.AddEntry(e2);
            allEntries.Remove(e1);
            allEntries.Remove(e2);

            // Distribute remaining entries
            DistributeEntries(allEntries, this, newNode);
        }
        else
        {
            // Split child nodes
            var allChildren = new List<RTreeNode<T>>(Children);
            Children.Clear();

            // Find two children with maximum separation
            var (c1, c2) = PickSeeds(allChildren);
            AddChild(c1);
            newNode.AddChild(c2);
            allChildren.Remove(c1);
            allChildren.Remove(c2);

            // Distribute remaining children
            DistributeChildren(allChildren, this, newNode);
        }

        return newNode;
    }

    private (TItem, TItem) PickSeeds<TItem>(List<TItem> items) where TItem : class
    {
        TItem? seed1 = null, seed2 = null;
        int maxWaste = int.MinValue;

        for (int i = 0; i < items.Count; i++)
        {
            for (int j = i + 1; j < items.Count; j++)
            {
                var rect1 = GetBounds(items[i]);
                var rect2 = GetBounds(items[j]);
                var combined = Rectangle.Union(rect1, rect2);
                var waste = combined.Area - rect1.Area - rect2.Area;

                if (waste > maxWaste)
                {
                    maxWaste = waste;
                    seed1 = items[i];
                    seed2 = items[j];
                }
            }
        }

        return (seed1!, seed2!);
    }

    private Rectangle GetBounds(object item)
    {
        return item switch
        {
            RTreeEntry<T> entry => entry.Bounds,
            RTreeNode<T> node => node.MBR,
            _ => throw new ArgumentException("Invalid item type")
        };
    }

    private void DistributeEntries(List<RTreeEntry<T>> entries, RTreeNode<T> node1, RTreeNode<T> node2)
    {
        while (entries.Count > 0)
        {
            if (node1.Entries.Count + entries.Count <= MinChildren)
            {
                // Add all remaining to node1
                entries.ForEach(node1.AddEntry);
                break;
            }
            if (node2.Entries.Count + entries.Count <= MinChildren)
            {
                // Add all remaining to node2
                entries.ForEach(node2.AddEntry);
                break;
            }

            // Choose next entry to assign
            var entry = entries[0];
            entries.RemoveAt(0);

            var enlargement1 = Rectangle.Union(node1.MBR, entry.Bounds).Area - node1.MBR.Area;
            var enlargement2 = Rectangle.Union(node2.MBR, entry.Bounds).Area - node2.MBR.Area;

            if (enlargement1 < enlargement2)
                node1.AddEntry(entry);
            else
                node2.AddEntry(entry);
        }
    }

    private void DistributeChildren(List<RTreeNode<T>> children, RTreeNode<T> node1, RTreeNode<T> node2)
    {
        while (children.Count > 0)
        {
            if (node1.Children.Count + children.Count <= MinChildren)
            {
                children.ForEach(node1.AddChild);
                break;
            }
            if (node2.Children.Count + children.Count <= MinChildren)
            {
                children.ForEach(node2.AddChild);
                break;
            }

            var child = children[0];
            children.RemoveAt(0);

            var enlargement1 = Rectangle.Union(node1.MBR, child.MBR).Area - node1.MBR.Area;
            var enlargement2 = Rectangle.Union(node2.MBR, child.MBR).Area - node2.MBR.Area;

            if (enlargement1 < enlargement2)
                node1.AddChild(child);
            else
                node2.AddChild(child);
        }
    }
}

/// <summary>
/// Represents an entry in a leaf node of the R-Tree.
/// </summary>
internal class RTreeEntry<T>
{
    public Rectangle Bounds { get; }
    public int ZIndex { get; }
    public T Element { get; }

    public RTreeEntry(Rectangle bounds, int zIndex, T element)
    {
        Bounds = bounds;
        ZIndex = zIndex;
        Element = element;
    }
}