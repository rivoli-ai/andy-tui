using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Andy.TUI.Spatial;

namespace Andy.TUI.Spatial.Tests;

public class Enhanced3DRTreeTests
{
    private class TestElement
    {
        public string Id { get; set; }
        public TestElement(string id) => Id = id;
        public override string ToString() => Id;
    }

    [Fact]
    public void Constructor_CreatesEmptyTree()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        
        Assert.Equal(0, tree.Count);
        Assert.NotNull(tree.GetStatistics());
    }

    [Fact]
    public void Insert_SingleElement_IncreasesCount()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var element = new TestElement("test1");
        var bounds = new Rectangle(10, 20, 30, 40);
        
        tree.Insert(bounds, 1, element);
        
        Assert.Equal(1, tree.Count);
    }

    [Fact]
    public void Insert_NullElement_ThrowsException()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var bounds = new Rectangle(10, 20, 30, 40);
        
        Assert.Throws<ArgumentNullException>(() => tree.Insert(bounds, 1, null!));
    }

    [Fact]
    public void Insert_MultipleElements_IncreasesCount()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        
        tree.Insert(new Rectangle(0, 0, 10, 10), 1, new TestElement("elem1"));
        tree.Insert(new Rectangle(20, 20, 10, 10), 2, new TestElement("elem2"));
        tree.Insert(new Rectangle(40, 40, 10, 10), 3, new TestElement("elem3"));
        
        Assert.Equal(3, tree.Count);
    }

    [Fact]
    public void Query_EmptyTree_ReturnsEmpty()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var queryRegion = new Rectangle(0, 0, 100, 100);
        
        var results = tree.Query(queryRegion);
        
        Assert.Empty(results);
    }

    [Fact]
    public void Query_SingleElement_ReturnsElementWhenOverlapping()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var element = new TestElement("test1");
        var bounds = new Rectangle(10, 10, 20, 20);
        
        tree.Insert(bounds, 1, element);
        
        // Query that overlaps
        var results = tree.Query(new Rectangle(15, 15, 10, 10)).ToList();
        Assert.Single(results);
        Assert.Same(element, results[0]);
        
        // Query that doesn't overlap
        var noResults = tree.Query(new Rectangle(50, 50, 10, 10)).ToList();
        Assert.Empty(noResults);
    }

    [Fact]
    public void Query_MultipleElements_ReturnsOnlyOverlapping()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        var elem3 = new TestElement("elem3");
        
        tree.Insert(new Rectangle(0, 0, 20, 20), 1, elem1);
        tree.Insert(new Rectangle(30, 30, 20, 20), 2, elem2);
        tree.Insert(new Rectangle(15, 15, 20, 20), 3, elem3);
        
        // Query that overlaps elem1 and elem3
        var results = tree.Query(new Rectangle(10, 10, 15, 15)).ToList();
        
        Assert.Equal(2, results.Count);
        Assert.Contains(elem1, results);
        Assert.Contains(elem3, results);
        Assert.DoesNotContain(elem2, results);
    }

    [Fact]
    public void QueryPoint_ReturnsElementsContainingPoint()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        
        tree.Insert(new Rectangle(0, 0, 30, 30), 1, elem1);
        tree.Insert(new Rectangle(20, 20, 30, 30), 2, elem2);
        
        // Point in both elements
        var results = tree.QueryPoint(25, 25).ToList();
        Assert.Equal(2, results.Count);
        Assert.Contains(elem1, results);
        Assert.Contains(elem2, results);
        
        // Point only in elem1
        var results2 = tree.QueryPoint(5, 5).ToList();
        Assert.Single(results2);
        Assert.Same(elem1, results2[0]);
        
        // Point outside all elements
        var results3 = tree.QueryPoint(100, 100).ToList();
        Assert.Empty(results3);
    }

    [Fact]
    public void Remove_ExistingElement_DecreasesCount()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var element = new TestElement("test1");
        var bounds = new Rectangle(10, 10, 20, 20);
        
        tree.Insert(bounds, 1, element);
        Assert.Equal(1, tree.Count);
        
        var removed = tree.Remove(bounds, 1, element);
        
        Assert.True(removed);
        Assert.Equal(0, tree.Count);
    }

    [Fact]
    public void Remove_NonExistentElement_ReturnsFalse()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var element = new TestElement("test1");
        var bounds = new Rectangle(10, 10, 20, 20);
        
        var removed = tree.Remove(bounds, 1, element);
        
        Assert.False(removed);
        Assert.Equal(0, tree.Count);
    }

    [Fact]
    public void Update_ElementPosition_MovesElement()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var element = new TestElement("test1");
        var oldBounds = new Rectangle(10, 10, 20, 20);
        var newBounds = new Rectangle(50, 50, 20, 20);
        
        tree.Insert(oldBounds, 1, element);
        
        // Verify it's at old position
        Assert.Contains(element, tree.Query(oldBounds));
        Assert.DoesNotContain(element, tree.Query(newBounds));
        
        tree.Update(oldBounds, 1, newBounds, 1, element);
        
        // Verify it's at new position
        Assert.DoesNotContain(element, tree.Query(oldBounds));
        Assert.Contains(element, tree.Query(newBounds));
        Assert.Equal(1, tree.Count);
    }

    [Fact]
    public void UpdateZIndex_ChangesElementZOrder()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var element = new TestElement("test1");
        var bounds = new Rectangle(10, 10, 20, 20);
        
        tree.Insert(bounds, 5, element);
        
        var initialMax = tree.GetMaxZIndex();
        Assert.Equal(5, initialMax);
        
        tree.UpdateZIndex(element, 5, 10);
        
        var newMax = tree.GetMaxZIndex();
        Assert.Equal(10, newMax);
    }

    [Fact]
    public void QueryWithZRange_FiltersElementsByZ()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("z1");
        var elem2 = new TestElement("z5");
        var elem3 = new TestElement("z10");
        var bounds = new Rectangle(10, 10, 20, 20);
        
        tree.Insert(bounds, 1, elem1);
        tree.Insert(bounds, 5, elem2);
        tree.Insert(bounds, 10, elem3);
        
        // Query z-range 3-7
        var results = tree.QueryWithZRange(bounds, 3, 7).ToList();
        
        Assert.Single(results);
        Assert.Same(elem2, results[0]);
    }

    [Fact]
    public void QueryTopmost_ReturnsHighestZElement()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("bottom");
        var elem2 = new TestElement("middle");
        var elem3 = new TestElement("top");
        
        tree.Insert(new Rectangle(0, 0, 30, 30), 1, elem1);
        tree.Insert(new Rectangle(10, 10, 30, 30), 5, elem2);
        tree.Insert(new Rectangle(20, 20, 30, 30), 10, elem3);
        
        // Point where all three overlap
        var topmost = tree.QueryTopmost(25, 25).ToList();
        
        Assert.Single(topmost);
        Assert.Same(elem3, topmost[0]);
    }

    [Fact]
    public void BringToFront_MovesElementToTop()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        
        tree.Insert(new Rectangle(0, 0, 10, 10), 1, elem1);
        tree.Insert(new Rectangle(0, 0, 10, 10), 5, elem2);
        
        Assert.Equal(5, tree.GetMaxZIndex());
        
        tree.BringToFront(elem1);
        
        // elem1 should now be above elem2
        var topmost = tree.QueryTopmost(5, 5).ToList();
        Assert.Single(topmost);
        Assert.Same(elem1, topmost[0]);
        Assert.True(tree.GetMaxZIndex() >= 5);
    }

    [Fact]
    public void SendToBack_MovesElementToBottom()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        
        tree.Insert(new Rectangle(0, 0, 10, 10), 5, elem1);
        tree.Insert(new Rectangle(0, 0, 10, 10), 1, elem2);
        
        Assert.Equal(1, tree.GetMinZIndex());
        
        tree.SendToBack(elem1);
        
        // elem1 should now be below elem2
        var topmost = tree.QueryTopmost(5, 5).ToList();
        Assert.Single(topmost);
        Assert.Same(elem2, topmost[0]);
        Assert.True(tree.GetMinZIndex() <= 1);
    }

    [Fact]
    public void SwapZOrder_ExchangesElementPositions()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        var bounds = new Rectangle(0, 0, 10, 10);
        
        tree.Insert(bounds, 1, elem1);
        tree.Insert(bounds, 5, elem2);
        
        // Initially elem2 is on top
        var topmostBefore = tree.QueryTopmost(5, 5).First();
        Assert.Same(elem2, topmostBefore);
        
        tree.SwapZOrder(elem1, elem2);
        
        // Now elem1 should be on top
        var topmostAfter = tree.QueryTopmost(5, 5).First();
        Assert.Same(elem1, topmostAfter);
    }

    [Fact]
    public void Clear_RemovesAllElements()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        
        tree.Insert(new Rectangle(0, 0, 10, 10), 1, new TestElement("elem1"));
        tree.Insert(new Rectangle(20, 20, 10, 10), 2, new TestElement("elem2"));
        tree.Insert(new Rectangle(40, 40, 10, 10), 3, new TestElement("elem3"));
        
        Assert.Equal(3, tree.Count);
        
        tree.Clear();
        
        Assert.Equal(0, tree.Count);
        Assert.Empty(tree.Query(new Rectangle(0, 0, 100, 100)));
    }

    [Fact]
    public void GetStatistics_ReturnsCorrectStats()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        
        tree.Insert(new Rectangle(0, 0, 10, 10), 1, new TestElement("elem1"));
        tree.Insert(new Rectangle(20, 20, 10, 10), 5, new TestElement("elem2"));
        tree.Insert(new Rectangle(40, 40, 10, 10), 10, new TestElement("elem3"));
        
        var stats = tree.GetStatistics();
        
        Assert.NotNull(stats);
        Assert.Equal(3, stats.TotalElements);
        Assert.Equal(1, stats.MinZIndex);
        Assert.Equal(10, stats.MaxZIndex);
        Assert.Equal(3, stats.UniqueZLevels);
    }

    [Fact]
    public void FindOccludedBy_DetectsOccludedElements()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var background = new TestElement("background");
        var foreground = new TestElement("foreground");
        
        // Background element
        tree.Insert(new Rectangle(10, 10, 30, 30), 1, background);
        // Foreground element that partially covers background
        tree.Insert(new Rectangle(20, 20, 30, 30), 5, foreground);
        
        var occluded = tree.FindOccludedBy(foreground).ToList();
        
        Assert.Single(occluded);
        Assert.Same(background, occluded[0]);
    }

    [Fact]
    public void FindOccluding_DetectsOccludingElements()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var background = new TestElement("background");
        var foreground = new TestElement("foreground");
        
        // Background element
        tree.Insert(new Rectangle(10, 10, 30, 30), 1, background);
        // Foreground element that partially covers background
        tree.Insert(new Rectangle(20, 20, 30, 30), 5, foreground);
        
        var occluding = tree.FindOccluding(background).ToList();
        
        Assert.Single(occluding);
        Assert.Same(foreground, occluding[0]);
    }

    [Fact]
    public void IsCompletelyOccluded_DetectsFullOcclusion()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var hidden = new TestElement("hidden");
        var cover = new TestElement("cover");
        
        // Element that will be hidden
        tree.Insert(new Rectangle(20, 20, 10, 10), 1, hidden);
        // Element that completely covers it
        tree.Insert(new Rectangle(10, 10, 30, 30), 5, cover);
        
        Assert.True(tree.IsCompletelyOccluded(hidden));
        Assert.False(tree.IsCompletelyOccluded(cover));
    }

    [Fact]
    public void GetVisibleRegion_ReturnsVisiblePortion()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var partial = new TestElement("partial");
        var cover = new TestElement("cover");
        
        // Element that will be partially covered
        tree.Insert(new Rectangle(10, 10, 30, 30), 1, partial);
        // Element that partially covers it
        tree.Insert(new Rectangle(25, 25, 30, 30), 5, cover);
        
        var visibleRegion = tree.GetVisibleRegion(partial);
        
        Assert.NotNull(visibleRegion);
        // The visible region should be smaller than the original
        Assert.True(visibleRegion.Value.Area < (30 * 30));
    }

    [Fact]
    public void FindRevealedByMovement_DetectsRevealedElements()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var hidden = new TestElement("hidden");
        var mover = new TestElement("mover");
        
        // Hidden element
        tree.Insert(new Rectangle(20, 20, 20, 20), 1, hidden);
        // Element that covers it
        tree.Insert(new Rectangle(20, 20, 20, 20), 5, mover);
        
        // Move the covering element away
        var oldBounds = new Rectangle(20, 20, 20, 20);
        var newBounds = new Rectangle(50, 50, 20, 20);
        
        var revealed = tree.FindRevealedByMovement(oldBounds, newBounds, 5).ToList();
        
        Assert.Single(revealed);
        Assert.Same(hidden, revealed[0]);
    }

    [Fact]
    public void QueryVisible_ExcludesOccludedElements()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var hidden = new TestElement("hidden");
        var visible = new TestElement("visible");
        var cover = new TestElement("cover");
        
        // Hidden element
        tree.Insert(new Rectangle(20, 20, 10, 10), 1, hidden);
        // Visible element
        tree.Insert(new Rectangle(40, 40, 10, 10), 2, visible);
        // Covering element
        tree.Insert(new Rectangle(15, 15, 20, 20), 5, cover);
        
        var queryRegion = new Rectangle(0, 0, 100, 100);
        var visibleElements = tree.QueryVisible(queryRegion).ToList();
        
        // Should return visible and cover, but not hidden
        Assert.Equal(2, visibleElements.Count);
        Assert.Contains(visible, visibleElements);
        Assert.Contains(cover, visibleElements);
        Assert.DoesNotContain(hidden, visibleElements);
    }

    [Fact]
    public void Rebuild_MaintainsAllElements()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        var elem3 = new TestElement("elem3");
        
        tree.Insert(new Rectangle(0, 0, 10, 10), 1, elem1);
        tree.Insert(new Rectangle(20, 20, 10, 10), 2, elem2);
        tree.Insert(new Rectangle(40, 40, 10, 10), 3, elem3);
        
        var countBefore = tree.Count;
        
        tree.Rebuild();
        
        Assert.Equal(countBefore, tree.Count);
        Assert.Contains(elem1, tree.Query(new Rectangle(0, 0, 10, 10)));
        Assert.Contains(elem2, tree.Query(new Rectangle(20, 20, 10, 10)));
        Assert.Contains(elem3, tree.Query(new Rectangle(40, 40, 10, 10)));
    }

    [Fact]
    public void RecalculateOcclusion_UpdatesOcclusionState()
    {
        var tree = new Enhanced3DRTree<TestElement>();
        var elem1 = new TestElement("elem1");
        var elem2 = new TestElement("elem2");
        
        tree.Insert(new Rectangle(10, 10, 20, 20), 1, elem1);
        tree.Insert(new Rectangle(15, 15, 20, 20), 5, elem2);
        
        // This should not throw and should update internal occlusion state
        tree.RecalculateOcclusion();
        
        // Verify occlusion is correctly calculated
        var occluded = tree.FindOccludedBy(elem2);
        Assert.Contains(elem1, occluded);
    }
}