using System;
using System.Collections.Generic;
using Xunit;
using Andy.TUI.Spatial;

namespace Andy.TUI.Spatial.Tests;

public class SpatialElementTests
{
    private class TestObject
    {
        public string Name { get; set; }
        public TestObject(string name) => Name = name;
        public override string ToString() => Name;
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var bounds = new Rectangle(10, 20, 30, 40);
        var obj = new TestObject("test");
        
        var element = new SpatialElement<TestObject>(bounds, 5, obj);
        
        Assert.Equal(bounds, element.Bounds);
        Assert.Equal(5, element.ZIndex);
        Assert.Same(obj, element.Element);
        Assert.False(element.IsFullyOccluded);
        Assert.NotEqual(Guid.Empty, element.Id);
    }

    [Fact]
    public void Constructor_NullElement_ThrowsException()
    {
        var bounds = new Rectangle(10, 20, 30, 40);
        
        Assert.Throws<ArgumentNullException>(() => new SpatialElement<TestObject>(bounds, 5, null!));
    }

    [Fact]
    public void Bounds_CanBeModified()
    {
        var obj = new TestObject("test");
        var element = new SpatialElement<TestObject>(new Rectangle(0, 0, 10, 10), 1, obj);
        
        var newBounds = new Rectangle(20, 30, 40, 50);
        element.Bounds = newBounds;
        
        Assert.Equal(newBounds, element.Bounds);
    }

    [Fact]
    public void ZIndex_CanBeModified()
    {
        var obj = new TestObject("test");
        var element = new SpatialElement<TestObject>(new Rectangle(0, 0, 10, 10), 1, obj);
        
        element.ZIndex = 10;
        
        Assert.Equal(10, element.ZIndex);
    }

    [Fact]
    public void IsFullyOccluded_CanBeModified()
    {
        var obj = new TestObject("test");
        var element = new SpatialElement<TestObject>(new Rectangle(0, 0, 10, 10), 1, obj);
        
        Assert.False(element.IsFullyOccluded);
        
        element.IsFullyOccluded = true;
        
        Assert.True(element.IsFullyOccluded);
    }

    [Fact]
    public void CompletelyOccludes_HigherZAndContains_ReturnsTrue()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 50, 50), 10, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(20, 20, 20, 20), 5, obj2);
        
        Assert.True(element1.CompletelyOccludes(element2));
        Assert.False(element2.CompletelyOccludes(element1));
    }

    [Fact]
    public void CompletelyOccludes_LowerZ_ReturnsFalse()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 50, 50), 1, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(20, 20, 20, 20), 5, obj2);
        
        Assert.False(element1.CompletelyOccludes(element2));
    }

    [Fact]
    public void CompletelyOccludes_NotContaining_ReturnsFalse()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 10, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(40, 40, 20, 20), 5, obj2);
        
        Assert.False(element1.CompletelyOccludes(element2));
    }

    [Fact]
    public void PartiallyOccludes_HigherZAndIntersects_ReturnsTrue()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 30, 30), 10, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(25, 25, 30, 30), 5, obj2);
        
        Assert.True(element1.PartiallyOccludes(element2));
        Assert.False(element2.PartiallyOccludes(element1));
    }

    [Fact]
    public void PartiallyOccludes_CompletelyContains_ReturnsFalse()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 50, 50), 10, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(20, 20, 20, 20), 5, obj2);
        
        // Should return false because it completely occludes, not partially
        Assert.False(element1.PartiallyOccludes(element2));
    }

    [Fact]
    public void PartiallyOccludes_NoIntersection_ReturnsFalse()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 10, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(40, 40, 20, 20), 5, obj2);
        
        Assert.False(element1.PartiallyOccludes(element2));
    }

    [Fact]
    public void GetVisibleRegion_NotOccluded_ReturnsFullBounds()
    {
        var obj = new TestObject("test");
        var bounds = new Rectangle(10, 10, 30, 30);
        var element = new SpatialElement<TestObject>(bounds, 1, obj);
        
        var visibleRegion = element.GetVisibleRegion();
        
        Assert.NotNull(visibleRegion);
        Assert.Equal(bounds, visibleRegion.Value);
    }

    [Fact]
    public void GetVisibleRegion_FullyOccluded_ReturnsNull()
    {
        var obj = new TestObject("test");
        var element = new SpatialElement<TestObject>(new Rectangle(10, 10, 30, 30), 1, obj);
        element.IsFullyOccluded = true;
        
        var visibleRegion = element.GetVisibleRegion();
        
        Assert.Null(visibleRegion);
    }

    [Fact]
    public void OccludedBy_CanAddElements()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        var obj3 = new TestObject("obj3");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 1, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(15, 15, 20, 20), 5, obj2);
        var element3 = new SpatialElement<TestObject>(new Rectangle(20, 20, 20, 20), 10, obj3);
        
        element1.OccludedBy.Add(element2);
        element1.OccludedBy.Add(element3);
        
        Assert.Equal(2, element1.OccludedBy.Count);
        Assert.Contains(element2, element1.OccludedBy);
        Assert.Contains(element3, element1.OccludedBy);
    }

    [Fact]
    public void Occludes_CanAddElements()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        var obj3 = new TestObject("obj3");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 40, 40), 10, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(15, 15, 20, 20), 5, obj2);
        var element3 = new SpatialElement<TestObject>(new Rectangle(20, 20, 20, 20), 1, obj3);
        
        element1.Occludes.Add(element2);
        element1.Occludes.Add(element3);
        
        Assert.Equal(2, element1.Occludes.Count);
        Assert.Contains(element2, element1.Occludes);
        Assert.Contains(element3, element1.Occludes);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 1, obj1);
        var element2 = element1; // Same reference
        
        Assert.True(element1.Equals(element2));
        Assert.True(element1.Equals((object)element2));
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var obj1 = new TestObject("obj1");
        var obj2 = new TestObject("obj2");
        
        var element1 = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 1, obj1);
        var element2 = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 1, obj2);
        
        Assert.False(element1.Equals(element2));
        Assert.False(element1.Equals((object)element2));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var obj = new TestObject("obj");
        var element = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 1, obj);
        
        Assert.False(element.Equals(null));
        Assert.False(element.Equals((object?)null));
    }

    [Fact]
    public void GetHashCode_BasedOnId()
    {
        var obj = new TestObject("obj");
        var element = new SpatialElement<TestObject>(new Rectangle(10, 10, 20, 20), 1, obj);
        
        var hash1 = element.GetHashCode();
        var hash2 = element.Id.GetHashCode();
        
        Assert.Equal(hash2, hash1);
    }

    [Fact]
    public void ToString_ContainsAllInfo()
    {
        var obj = new TestObject("TestObj");
        var bounds = new Rectangle(10, 20, 30, 40);
        var element = new SpatialElement<TestObject>(bounds, 5, obj);
        element.IsFullyOccluded = true;
        
        var str = element.ToString();
        
        Assert.Contains("TestObj", str);
        Assert.Contains("Bounds", str);
        Assert.Contains("Z=5", str);
        Assert.Contains("Occluded=True", str);
    }

    [Fact]
    public void Id_IsUnique()
    {
        var obj = new TestObject("obj");
        var element1 = new SpatialElement<TestObject>(new Rectangle(0, 0, 10, 10), 1, obj);
        var element2 = new SpatialElement<TestObject>(new Rectangle(0, 0, 10, 10), 1, obj);
        
        Assert.NotEqual(element1.Id, element2.Id);
        Assert.NotEqual(Guid.Empty, element1.Id);
        Assert.NotEqual(Guid.Empty, element2.Id);
    }
}