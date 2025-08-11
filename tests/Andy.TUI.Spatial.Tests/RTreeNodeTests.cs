using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Andy.TUI.Spatial.Tests;

public class RTreeNodeTests
{
    private class TestItem
    {
        public string Name { get; }
        public TestItem(string name) => Name = name;
    }

    [Fact]
    public void Constructor_LeafNode_InitializesCorrectly()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true, level: 0);

        Assert.True(node.IsLeaf);
        Assert.Equal(0, node.Level);
        Assert.Empty(node.Entries);
        Assert.Empty(node.Children);
        Assert.Null(node.Parent);
        Assert.Equal(int.MaxValue, node.MBR.X);
        Assert.Equal(int.MaxValue, node.MBR.Y);
        Assert.Equal(0, node.MBR.Width);
        Assert.Equal(0, node.MBR.Height);
    }

    [Fact]
    public void Constructor_InternalNode_InitializesCorrectly()
    {
        var node = new RTreeNode<TestItem>(isLeaf: false, level: 2);

        Assert.False(node.IsLeaf);
        Assert.Equal(2, node.Level);
        Assert.Empty(node.Entries);
        Assert.Empty(node.Children);
        Assert.Null(node.Parent);
    }

    [Fact]
    public void AddEntry_ToLeafNode_AddsSuccessfully()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);
        var entry = new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test"));

        node.AddEntry(entry);

        Assert.Single(node.Entries);
        Assert.Same(entry, node.Entries[0]);
        Assert.Equal(10, node.MBR.X);
        Assert.Equal(20, node.MBR.Y);
        Assert.Equal(30, node.MBR.Width);
        Assert.Equal(40, node.MBR.Height);
    }

    [Fact]
    public void AddEntry_ToInternalNode_ThrowsException()
    {
        var node = new RTreeNode<TestItem>(isLeaf: false);
        var entry = new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test"));

        var ex = Assert.Throws<InvalidOperationException>(() => node.AddEntry(entry));
        Assert.Equal("Cannot add entries to an internal node.", ex.Message);
    }

    [Fact]
    public void AddChild_ToInternalNode_AddsSuccessfully()
    {
        var parentNode = new RTreeNode<TestItem>(isLeaf: false, level: 2);
        var childNode = new RTreeNode<TestItem>(isLeaf: true, level: 1);

        parentNode.AddChild(childNode);

        Assert.Single(parentNode.Children);
        Assert.Same(childNode, parentNode.Children[0]);
        Assert.Same(parentNode, childNode.Parent);
    }

    [Fact]
    public void AddChild_ToLeafNode_ThrowsException()
    {
        var leafNode = new RTreeNode<TestItem>(isLeaf: true);
        var childNode = new RTreeNode<TestItem>(isLeaf: true);

        var ex = Assert.Throws<InvalidOperationException>(() => leafNode.AddChild(childNode));
        Assert.Equal("Cannot add child nodes to a leaf node.", ex.Message);
    }

    [Fact]
    public void RemoveEntry_ExistingEntry_RemovesSuccessfully()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);
        var entry1 = new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test1"));
        var entry2 = new RTreeEntry<TestItem>(new Rectangle(50, 60, 20, 20), 2, new TestItem("test2"));

        node.AddEntry(entry1);
        node.AddEntry(entry2);

        var removed = node.RemoveEntry(entry1);

        Assert.True(removed);
        Assert.Single(node.Entries);
        Assert.Same(entry2, node.Entries[0]);
        Assert.Equal(50, node.MBR.X);
        Assert.Equal(60, node.MBR.Y);
    }

    [Fact]
    public void RemoveEntry_NonExistingEntry_ReturnsFalse()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);
        var entry1 = new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test1"));
        var entry2 = new RTreeEntry<TestItem>(new Rectangle(50, 60, 20, 20), 2, new TestItem("test2"));

        node.AddEntry(entry1);

        var removed = node.RemoveEntry(entry2);

        Assert.False(removed);
        Assert.Single(node.Entries);
    }

    [Fact]
    public void RemoveChild_ExistingChild_RemovesSuccessfully()
    {
        var parentNode = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        parentNode.AddChild(child1);
        parentNode.AddChild(child2);

        var removed = parentNode.RemoveChild(child1);

        Assert.True(removed);
        Assert.Single(parentNode.Children);
        Assert.Same(child2, parentNode.Children[0]);
        Assert.Null(child1.Parent);
    }

    [Fact]
    public void RemoveChild_NonExistingChild_ReturnsFalse()
    {
        var parentNode = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        parentNode.AddChild(child1);

        var removed = parentNode.RemoveChild(child2);

        Assert.False(removed);
        Assert.Single(parentNode.Children);
    }

    [Fact]
    public void NeedsSplit_LeafNodeOverCapacity_ReturnsTrue()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);

        for (int i = 0; i < 5; i++)
        {
            var entry = new RTreeEntry<TestItem>(new Rectangle(i * 10, i * 10, 10, 10), i, new TestItem($"test{i}"));
            node.AddEntry(entry);
        }

        Assert.True(node.NeedsSplit);
    }

    [Fact]
    public void NeedsSplit_LeafNodeWithinCapacity_ReturnsFalse()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);

        for (int i = 0; i < 3; i++)
        {
            var entry = new RTreeEntry<TestItem>(new Rectangle(i * 10, i * 10, 10, 10), i, new TestItem($"test{i}"));
            node.AddEntry(entry);
        }

        Assert.False(node.NeedsSplit);
    }

    [Fact]
    public void NeedsSplit_InternalNodeOverCapacity_ReturnsTrue()
    {
        var node = new RTreeNode<TestItem>(isLeaf: false);

        for (int i = 0; i < 5; i++)
        {
            node.AddChild(new RTreeNode<TestItem>(isLeaf: true));
        }

        Assert.True(node.NeedsSplit);
    }

    [Fact]
    public void IsUnderfull_LeafNodeBelowMinimum_ReturnsTrue()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);
        var entry = new RTreeEntry<TestItem>(new Rectangle(10, 10, 10, 10), 1, new TestItem("test"));
        node.AddEntry(entry);

        Assert.True(node.IsUnderfull);
    }

    [Fact]
    public void IsUnderfull_LeafNodeAtMinimum_ReturnsFalse()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);

        for (int i = 0; i < 2; i++)
        {
            var entry = new RTreeEntry<TestItem>(new Rectangle(i * 10, i * 10, 10, 10), i, new TestItem($"test{i}"));
            node.AddEntry(entry);
        }

        Assert.False(node.IsUnderfull);
    }

    [Fact]
    public void UpdateMBR_LeafNodeWithEntries_CalculatesCorrectBounds()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);

        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test1")));
        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(50, 60, 20, 20), 2, new TestItem("test2")));

        Assert.Equal(10, node.MBR.X);
        Assert.Equal(20, node.MBR.Y);
        Assert.Equal(60, node.MBR.Width); // 50 + 20 - 10
        Assert.Equal(60, node.MBR.Height); // 60 + 20 - 20
    }

    [Fact]
    public void UpdateMBR_EmptyLeafNode_ResetsToDefault()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);
        var entry = new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test"));

        node.AddEntry(entry);
        node.RemoveEntry(entry);

        Assert.Equal(int.MaxValue, node.MBR.X);
        Assert.Equal(int.MaxValue, node.MBR.Y);
        Assert.Equal(0, node.MBR.Width);
        Assert.Equal(0, node.MBR.Height);
    }

    [Fact]
    public void UpdateMBR_InternalNodeWithChildren_CalculatesCorrectBounds()
    {
        var parentNode = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        child1.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test1")));
        child2.AddEntry(new RTreeEntry<TestItem>(new Rectangle(50, 60, 20, 20), 2, new TestItem("test2")));

        parentNode.AddChild(child1);
        parentNode.AddChild(child2);

        Assert.Equal(10, parentNode.MBR.X);
        Assert.Equal(20, parentNode.MBR.Y);
        Assert.Equal(60, parentNode.MBR.Width);
        Assert.Equal(60, parentNode.MBR.Height);
    }

    [Fact]
    public void UpdateMBR_PropagatesUpToParent()
    {
        var grandparent = new RTreeNode<TestItem>(isLeaf: false, level: 2);
        var parent = new RTreeNode<TestItem>(isLeaf: false, level: 1);
        var child = new RTreeNode<TestItem>(isLeaf: true, level: 0);

        grandparent.AddChild(parent);
        parent.AddChild(child);

        child.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 20, 30, 40), 1, new TestItem("test")));

        Assert.Equal(10, grandparent.MBR.X);
        Assert.Equal(20, grandparent.MBR.Y);
        Assert.Equal(30, grandparent.MBR.Width);
        Assert.Equal(40, grandparent.MBR.Height);
    }

    [Fact]
    public void Query_IntersectingRectangle_FindsCorrectEntries()
    {
        var root = new RTreeNode<TestItem>(isLeaf: true);

        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("test1")));
        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(40, 40, 20, 20), 2, new TestItem("test2")));
        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(70, 70, 20, 20), 3, new TestItem("test3")));

        var results = new List<RTreeEntry<TestItem>>();
        root.Query(new Rectangle(25, 25, 30, 30), results);

        Assert.Equal(2, results.Count);
        Assert.Equal("test1", results[0].Element.Name);
        Assert.Equal("test2", results[1].Element.Name);
    }

    [Fact]
    public void Query_NoIntersection_ReturnsEmpty()
    {
        var root = new RTreeNode<TestItem>(isLeaf: true);

        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("test1")));

        var results = new List<RTreeEntry<TestItem>>();
        root.Query(new Rectangle(50, 50, 20, 20), results);

        Assert.Empty(results);
    }

    [Fact]
    public void Query_ThroughInternalNodes_FindsCorrectEntries()
    {
        var root = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        child1.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("test1")));
        child2.AddEntry(new RTreeEntry<TestItem>(new Rectangle(40, 40, 20, 20), 2, new TestItem("test2")));

        root.AddChild(child1);
        root.AddChild(child2);

        var results = new List<RTreeEntry<TestItem>>();
        root.Query(new Rectangle(35, 35, 30, 30), results);

        Assert.Single(results);
        Assert.Equal("test2", results[0].Element.Name);
    }

    [Fact]
    public void QueryPoint_FindsEntriesAtPoint()
    {
        var root = new RTreeNode<TestItem>(isLeaf: true);

        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 30, 30), 1, new TestItem("test1")));
        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(20, 20, 30, 30), 2, new TestItem("test2")));
        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(60, 60, 20, 20), 3, new TestItem("test3")));

        var results = new List<RTreeEntry<TestItem>>();
        root.QueryPoint(25, 25, results);

        Assert.Equal(2, results.Count);
        Assert.Equal("test1", results[0].Element.Name);
        Assert.Equal("test2", results[1].Element.Name);
    }

    [Fact]
    public void QueryPoint_NoEntryAtPoint_ReturnsEmpty()
    {
        var root = new RTreeNode<TestItem>(isLeaf: true);

        root.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("test1")));

        var results = new List<RTreeEntry<TestItem>>();
        root.QueryPoint(50, 50, results);

        Assert.Empty(results);
    }

    [Fact]
    public void QueryPoint_ThroughInternalNodes_FindsCorrectEntries()
    {
        var root = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        child1.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 30, 30), 1, new TestItem("test1")));
        child2.AddEntry(new RTreeEntry<TestItem>(new Rectangle(40, 40, 30, 30), 2, new TestItem("test2")));

        root.AddChild(child1);
        root.AddChild(child2);

        var results = new List<RTreeEntry<TestItem>>();
        root.QueryPoint(50, 50, results);

        Assert.Single(results);
        Assert.Equal("test2", results[0].Element.Name);
    }

    [Fact]
    public void ChooseChild_SelectsChildWithMinimumEnlargement()
    {
        var parent = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        child1.AddEntry(new RTreeEntry<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("test1")));
        child2.AddEntry(new RTreeEntry<TestItem>(new Rectangle(50, 50, 20, 20), 2, new TestItem("test2")));

        parent.AddChild(child1);
        parent.AddChild(child2);

        var newBounds = new Rectangle(25, 25, 10, 10);
        var chosen = parent.ChooseChild(newBounds);

        Assert.Same(child1, chosen);
    }

    [Fact]
    public void ChooseChild_OnLeafNode_ThrowsException()
    {
        var leafNode = new RTreeNode<TestItem>(isLeaf: true);
        var bounds = new Rectangle(10, 10, 20, 20);

        var ex = Assert.Throws<InvalidOperationException>(() => leafNode.ChooseChild(bounds));
        Assert.Equal("Cannot choose child from a leaf node.", ex.Message);
    }

    [Fact]
    public void ChooseChild_WithEqualEnlargement_SelectsSmallestArea()
    {
        var parent = new RTreeNode<TestItem>(isLeaf: false);
        var child1 = new RTreeNode<TestItem>(isLeaf: true);
        var child2 = new RTreeNode<TestItem>(isLeaf: true);

        child1.AddEntry(new RTreeEntry<TestItem>(new Rectangle(0, 0, 10, 10), 1, new TestItem("small")));
        child2.AddEntry(new RTreeEntry<TestItem>(new Rectangle(20, 20, 20, 20), 2, new TestItem("large")));

        parent.AddChild(child1);
        parent.AddChild(child2);

        var newBounds = new Rectangle(50, 50, 5, 5);
        var chosen = parent.ChooseChild(newBounds);

        Assert.Same(child1, chosen);
    }

    [Fact]
    public void Split_LeafNode_DistributesEntriesCorrectly()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);

        for (int i = 0; i < 5; i++)
        {
            var entry = new RTreeEntry<TestItem>(new Rectangle(i * 20, i * 20, 10, 10), i, new TestItem($"test{i}"));
            node.AddEntry(entry);
        }

        var newNode = node.Split();

        Assert.NotNull(newNode);
        Assert.True(newNode.IsLeaf);
        Assert.Equal(node.Level, newNode.Level);
        Assert.True(node.Entries.Count >= 2);
        Assert.True(newNode.Entries.Count >= 2);
        Assert.Equal(5, node.Entries.Count + newNode.Entries.Count);
    }

    [Fact]
    public void Split_InternalNode_WithTightClusters_KeepsBalance()
    {
        var node = new RTreeNode<TestItem>(isLeaf: false, level: 1);

        // Cluster A around (0,0)
        for (int i = 0; i < 3; i++)
        {
            var child = new RTreeNode<TestItem>(isLeaf: true, level: 0);
            child.AddEntry(new RTreeEntry<TestItem>(new Rectangle(i, i, 2, 2), i, new TestItem($"A{i}")));
            node.AddChild(child);
        }

        // Cluster B around (100,100)
        for (int i = 0; i < 3; i++)
        {
            var child = new RTreeNode<TestItem>(isLeaf: true, level: 0);
            child.AddEntry(new RTreeEntry<TestItem>(new Rectangle(100 + i, 100 + i, 2, 2), i, new TestItem($"B{i}")));
            node.AddChild(child);
        }

        var newNode = node.Split();

        Assert.Equal(node.Level, newNode.Level);
        Assert.True(node.Children.Count >= 2);
        Assert.True(newNode.Children.Count >= 2);
        Assert.Equal(6, node.Children.Count + newNode.Children.Count);
    }

    [Fact]
    public void Split_InternalNode_DistributesChildrenCorrectly()
    {
        var node = new RTreeNode<TestItem>(isLeaf: false, level: 1);

        for (int i = 0; i < 5; i++)
        {
            var child = new RTreeNode<TestItem>(isLeaf: true, level: 0);
            child.AddEntry(new RTreeEntry<TestItem>(new Rectangle(i * 20, i * 20, 10, 10), i, new TestItem($"test{i}")));
            node.AddChild(child);
        }

        var newNode = node.Split();

        Assert.NotNull(newNode);
        Assert.False(newNode.IsLeaf);
        Assert.Equal(node.Level, newNode.Level);
        Assert.True(node.Children.Count >= 2);
        Assert.True(newNode.Children.Count >= 2);
        Assert.Equal(5, node.Children.Count + newNode.Children.Count);

        foreach (var child in node.Children)
        {
            Assert.Same(node, child.Parent);
        }

        foreach (var child in newNode.Children)
        {
            Assert.Same(newNode, child.Parent);
        }
    }

    [Fact]
    public void Split_UpdatesMBRsCorrectly()
    {
        var node = new RTreeNode<TestItem>(isLeaf: true);

        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(0, 0, 10, 10), 1, new TestItem("test1")));
        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(20, 20, 10, 10), 2, new TestItem("test2")));
        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(40, 40, 10, 10), 3, new TestItem("test3")));
        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(60, 60, 10, 10), 4, new TestItem("test4")));
        node.AddEntry(new RTreeEntry<TestItem>(new Rectangle(80, 80, 10, 10), 5, new TestItem("test5")));

        var newNode = node.Split();

        Assert.NotEqual(int.MaxValue, node.MBR.X);
        Assert.NotEqual(int.MaxValue, newNode.MBR.X);
        Assert.True(node.MBR.Width > 0);
        Assert.True(newNode.MBR.Width > 0);
    }
}

public class RTreeEntryTests
{
    private class TestItem
    {
        public string Name { get; }
        public TestItem(string name) => Name = name;
    }

    [Fact]
    public void Constructor_InitializesAllProperties()
    {
        var bounds = new Rectangle(10, 20, 30, 40);
        var element = new TestItem("test");
        var entry = new RTreeEntry<TestItem>(bounds, 5, element);

        Assert.Equal(bounds, entry.Bounds);
        Assert.Equal(5, entry.ZIndex);
        Assert.Same(element, entry.Element);
    }

    [Fact]
    public void Bounds_Property_ReturnsCorrectValue()
    {
        var bounds = new Rectangle(100, 200, 50, 75);
        var entry = new RTreeEntry<TestItem>(bounds, 1, new TestItem("test"));

        Assert.Equal(100, entry.Bounds.X);
        Assert.Equal(200, entry.Bounds.Y);
        Assert.Equal(50, entry.Bounds.Width);
        Assert.Equal(75, entry.Bounds.Height);
    }

    [Fact]
    public void ZIndex_Property_ReturnsCorrectValue()
    {
        var entry = new RTreeEntry<TestItem>(new Rectangle(0, 0, 10, 10), 42, new TestItem("test"));

        Assert.Equal(42, entry.ZIndex);
    }

    [Fact]
    public void Element_Property_ReturnsCorrectObject()
    {
        var element = new TestItem("myItem");
        var entry = new RTreeEntry<TestItem>(new Rectangle(0, 0, 10, 10), 1, element);

        Assert.Same(element, entry.Element);
        Assert.Equal("myItem", entry.Element.Name);
    }
}