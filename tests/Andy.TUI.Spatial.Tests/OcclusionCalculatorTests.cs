using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Andy.TUI.Spatial;

namespace Andy.TUI.Spatial.Tests;

public class OcclusionCalculatorTests
{
    private class TestItem
    {
        public string Id { get; set; }
        public TestItem(string id) => Id = id;
        public override string ToString() => Id;
    }

    [Fact]
    public void CalculateVisible_EmptyCollection_ReturnsEmpty()
    {
        var calculator = new OcclusionCalculator();
        var elements = new List<SpatialElement<TestItem>>();
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Empty(visible);
    }

    [Fact]
    public void CalculateVisible_SingleElement_ReturnsElement()
    {
        var calculator = new OcclusionCalculator();
        var item = new TestItem("single");
        var element = new SpatialElement<TestItem>(new Rectangle(10, 10, 20, 20), 1, item);
        var elements = new List<SpatialElement<TestItem>> { element };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Single(visible);
        Assert.Same(element, visible[0]);
        Assert.False(element.IsFullyOccluded);
    }

    [Fact]
    public void CalculateVisible_ElementOutsideViewport_NotReturned()
    {
        var calculator = new OcclusionCalculator();
        var item = new TestItem("outside");
        var element = new SpatialElement<TestItem>(new Rectangle(150, 150, 20, 20), 1, item);
        var elements = new List<SpatialElement<TestItem>> { element };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Empty(visible);
    }

    [Fact]
    public void CalculateVisible_NonOverlappingElements_ReturnsAll()
    {
        var calculator = new OcclusionCalculator();
        var elem1 = new SpatialElement<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("elem1"));
        var elem2 = new SpatialElement<TestItem>(new Rectangle(40, 40, 20, 20), 2, new TestItem("elem2"));
        var elem3 = new SpatialElement<TestItem>(new Rectangle(70, 70, 20, 20), 3, new TestItem("elem3"));
        var elements = new List<SpatialElement<TestItem>> { elem1, elem2, elem3 };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Equal(3, visible.Count);
        Assert.All(visible, e => Assert.False(e.IsFullyOccluded));
    }

    [Fact]
    public void CalculateVisible_FullyOccludedElement_NotReturned()
    {
        var calculator = new OcclusionCalculator();
        var hidden = new SpatialElement<TestItem>(new Rectangle(20, 20, 10, 10), 1, new TestItem("hidden"));
        var cover = new SpatialElement<TestItem>(new Rectangle(15, 15, 20, 20), 5, new TestItem("cover"));
        var elements = new List<SpatialElement<TestItem>> { hidden, cover };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Single(visible);
        Assert.Same(cover, visible[0]);
        Assert.False(cover.IsFullyOccluded);
        Assert.True(hidden.IsFullyOccluded);
    }

    [Fact]
    public void CalculateVisible_PartiallyOccludedElement_IsReturned()
    {
        var calculator = new OcclusionCalculator();
        var partial = new SpatialElement<TestItem>(new Rectangle(10, 10, 30, 30), 1, new TestItem("partial"));
        var cover = new SpatialElement<TestItem>(new Rectangle(25, 25, 20, 20), 5, new TestItem("cover"));
        var elements = new List<SpatialElement<TestItem>> { partial, cover };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Equal(2, visible.Count);
        // Higher z-index first
        Assert.Same(cover, visible[0]);
        Assert.Same(partial, visible[1]);
        Assert.False(partial.IsFullyOccluded);
    }

    [Fact]
    public void CalculateVisible_MultipleOverlappingLayers_CorrectVisibility()
    {
        var calculator = new OcclusionCalculator();
        var bottom = new SpatialElement<TestItem>(new Rectangle(10, 10, 40, 40), 1, new TestItem("bottom"));
        var middle = new SpatialElement<TestItem>(new Rectangle(20, 20, 30, 30), 5, new TestItem("middle"));
        var top = new SpatialElement<TestItem>(new Rectangle(30, 30, 20, 20), 10, new TestItem("top"));
        var elements = new List<SpatialElement<TestItem>> { bottom, middle, top };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        // All should be visible as none is fully occluded
        Assert.Equal(3, visible.Count);
        // Check order (highest z-index first)
        Assert.Same(top, visible[0]);
        Assert.Same(middle, visible[1]);
        Assert.Same(bottom, visible[2]);
    }

    [Fact]
    public void CalculateVisible_SameZIndex_BothVisible()
    {
        var calculator = new OcclusionCalculator();
        var elem1 = new SpatialElement<TestItem>(new Rectangle(10, 10, 20, 20), 5, new TestItem("elem1"));
        var elem2 = new SpatialElement<TestItem>(new Rectangle(15, 15, 20, 20), 5, new TestItem("elem2"));
        var elements = new List<SpatialElement<TestItem>> { elem1, elem2 };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        // Both should be visible even with same z-index
        Assert.Equal(2, visible.Count);
        Assert.Contains(elem1, visible);
        Assert.Contains(elem2, visible);
    }

    [Fact]
    public void CalculateVisible_ElementPartiallyInViewport_IsReturned()
    {
        var calculator = new OcclusionCalculator();
        var partial = new SpatialElement<TestItem>(new Rectangle(90, 90, 20, 20), 1, new TestItem("partial"));
        var elements = new List<SpatialElement<TestItem>> { partial };
        var viewport = new Rectangle(0, 0, 100, 100);
        
        var visible = calculator.CalculateVisible(elements, viewport).ToList();
        
        Assert.Single(visible);
        Assert.Same(partial, visible[0]);
    }

    [Fact]
    public void CalculateDirtyRegions_ElementMoved_ReturnsOldAndNewBounds()
    {
        var calculator = new OcclusionCalculator();
        var elem = new SpatialElement<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("mover"));
        var allElements = new List<SpatialElement<TestItem>> { elem };
        
        var changes = new[] 
        {
            (elem.Element, new Rectangle(10, 10, 20, 20), 1, new Rectangle(30, 30, 20, 20), 1)
        };
        
        var dirty = calculator.CalculateDirtyRegions(allElements, changes).ToList();
        
        // Should contain at least the old and new bounds
        Assert.NotEmpty(dirty);
        Assert.Contains(dirty, r => r.Contains(15, 15)); // Old position
        Assert.Contains(dirty, r => r.Contains(35, 35)); // New position
    }

    [Fact]
    public void CalculateDirtyRegions_ZIndexChanged_AffectsOverlappingElements()
    {
        var calculator = new OcclusionCalculator();
        var elem1 = new SpatialElement<TestItem>(new Rectangle(10, 10, 30, 30), 1, new TestItem("elem1"));
        var elem2 = new SpatialElement<TestItem>(new Rectangle(20, 20, 30, 30), 2, new TestItem("elem2"));
        var allElements = new List<SpatialElement<TestItem>> { elem1, elem2 };
        
        // Change z-index of elem1 from 1 to 3 (moving above elem2)
        var changes = new[]
        {
            (elem1.Element, elem1.Bounds, 1, elem1.Bounds, 3)
        };
        
        var dirty = calculator.CalculateDirtyRegions(allElements, changes).ToList();
        
        // Should mark both elements as dirty since their relative order changed
        Assert.NotEmpty(dirty);
    }

    [Fact]
    public void CalculateDirtyRegions_NoChanges_ReturnsEmpty()
    {
        var calculator = new OcclusionCalculator();
        var elem = new SpatialElement<TestItem>(new Rectangle(10, 10, 20, 20), 1, new TestItem("static"));
        var allElements = new List<SpatialElement<TestItem>> { elem };
        
        var changes = Array.Empty<(TestItem, Rectangle, int, Rectangle, int)>();
        
        var dirty = calculator.CalculateDirtyRegions(allElements, changes).ToList();
        
        Assert.Empty(dirty);
    }
}