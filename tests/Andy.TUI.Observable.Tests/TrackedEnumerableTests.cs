using Andy.TUI.Observable;
using System.Collections;

namespace Andy.TUI.Core.Tests.Observable;

public class TrackedEnumerableTests
{
    [Fact]
    public void TrackedEnumerable_EnumeratesSourceCorrectly()
    {
        var source = new[] { 1, 2, 3, 4, 5 };
        var property = new ObservableProperty<int>(42);
        var tracked = new TrackedEnumerable<int>(source, property);
        
        var result = tracked.ToList();
        
        Assert.Equal(5, result.Count);
        Assert.Equal(source, result);
    }

    [Fact]
    public void TrackedEnumerable_TracksPropertyOnEnumeration()
    {
        var source = new[] { "a", "b", "c" };
        var property = new ObservableProperty<string>("test");
        var tracked = new TrackedEnumerable<string>(source, property);
        bool wasTracked = false;
        
        // Create a computed property to test tracking
        var computed = new ComputedProperty<string>(() =>
        {
            wasTracked = true;
            return string.Join(",", tracked);
        });
        
        wasTracked = false;
        var result = computed.Value;
        Assert.True(wasTracked);
        Assert.Equal("a,b,c", result);
        
        // Change the property to verify dependency
        wasTracked = false;
        property.Value = "changed";
        result = computed.Value;
        Assert.True(wasTracked); // Should recompute due to dependency
        Assert.Equal("a,b,c", result); // Result unchanged as source didn't change
    }

    [Fact]
    public void TrackedEnumerable_IEnumerableInterface_Works()
    {
        var source = new[] { 10, 20, 30 };
        var property = new ObservableProperty<int>(0);
        var tracked = new TrackedEnumerable<int>(source, property);
        
        // Test non-generic IEnumerable
        var result = new List<int>();
        foreach (var item in (IEnumerable)tracked)
        {
            result.Add((int)item);
        }
        
        Assert.Equal(source, result);
    }

    [Fact]
    public void TrackedEnumerable_MultipleEnumerations_TrackEachTime()
    {
        var source = new[] { 1, 2, 3 };
        var property = new ObservableProperty<int>(0);
        var tracked = new TrackedEnumerable<int>(source, property);
        var trackCount = 0;
        
        var computed = new ComputedProperty<int>(() =>
        {
            trackCount++;
            // Enumerate multiple times
            var sum1 = tracked.Sum();
            var sum2 = tracked.Sum();
            return sum1 + sum2;
        });
        
        trackCount = 0;
        var result = computed.Value;
        Assert.Equal(1, trackCount); // Only one computation
        Assert.Equal(12, result); // 6 + 6
    }

    [Fact]
    public void TrackedEnumerable_EmptySource_HandlesCorrectly()
    {
        var source = Array.Empty<string>();
        var property = new ObservableProperty<string>("test");
        var tracked = new TrackedEnumerable<string>(source, property);
        
        var result = tracked.ToList();
        
        Assert.Empty(result);
    }

    [Fact]
    public void TrackedEnumerable_WithLinqOperations_WorksCorrectly()
    {
        var source = Enumerable.Range(1, 10);
        var property = new ObservableProperty<int>(0);
        var tracked = new TrackedEnumerable<int>(source, property);
        
        var result = tracked
            .Where(x => x % 2 == 0)
            .Select(x => x * 2)
            .ToList();
        
        Assert.Equal(new[] { 4, 8, 12, 16, 20 }, result);
    }

    [Fact]
    public void TrackedEnumerable_DeferredExecution_TracksAtEnumeration()
    {
        var source = new[] { 1, 2, 3 };
        var property = new ObservableProperty<int>(0);
        var tracked = new TrackedEnumerable<int>(source, property);
        var wasTracked = false;
        
        var computed = new ComputedProperty<IEnumerable<int>>(() =>
        {
            wasTracked = true;
            // Return without enumerating
            return tracked.Where(x => x > 1);
        });
        
        // Getting the enumerable tracks the computed property
        wasTracked = false;
        var enumerable = computed.Value;
        Assert.True(wasTracked);
        
        // The TrackedEnumerable should track when we actually enumerate
        var dependencies = new List<IObservableProperty>();
        using (var tracker = DependencyTracker.BeginTracking())
        {
            var result = enumerable.ToList();
            dependencies.AddRange(DependencyTracker.Current!.Dependencies);
        }
        
        // Should have tracked both the computed property and the observable property
        Assert.Contains(property, dependencies);
        Assert.Equal(new[] { 2, 3 }, enumerable.ToList());
    }

    [Fact]
    public void TrackedEnumerable_IntegrationWithObservableCollection()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var tracked = new TrackedEnumerable<int>(collection, collection);
        
        var sum = new ComputedProperty<int>(() => tracked.Sum());
        
        Assert.Equal(6, sum.Value);
        
        collection.Add(4);
        Assert.Equal(10, sum.Value);
        
        collection.Remove(2);
        Assert.Equal(8, sum.Value);
    }
}