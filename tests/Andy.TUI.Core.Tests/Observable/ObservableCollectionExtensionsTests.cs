using Andy.TUI.Core.Observable;

namespace Andy.TUI.Core.Tests.Observable;

public class ObservableCollectionExtensionsTests
{
    [Fact]
    public void AsTracked_EnumeratesAllItems()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3, 4, 5 });
        
        var result = collection.AsTracked().ToList();
        
        Assert.Equal(5, result.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, result);
    }

    [Fact]
    public void AsTracked_TracksCollectionAsPropertyDependency()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        bool wasTracked = false;
        
        // Create a computed property that uses AsTracked
        var sum = new ComputedProperty<int>(() =>
        {
            wasTracked = true;
            return collection.AsTracked().Sum();
        });
        
        // Initial computation should track
        wasTracked = false;
        var value = sum.Value;
        Assert.True(wasTracked);
        Assert.Equal(6, value);
        
        // Changing collection should trigger recomputation
        wasTracked = false;
        collection.Add(4);
        value = sum.Value;
        Assert.True(wasTracked);
        Assert.Equal(10, value);
    }

    [Fact]
    public void AsTracked_WorksWithLinqOperations()
    {
        var collection = new ObservableCollection<string>(new[] { "apple", "banana", "cherry" });
        
        var filtered = collection.AsTracked()
            .Where(x => x.StartsWith("a"))
            .ToList();
        
        Assert.Single(filtered);
        Assert.Equal("apple", filtered[0]);
    }

    [Fact]
    public void AsTracked_MultipleEnumerations_TrackEachTime()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var trackCount = 0;
        
        var computed = new ComputedProperty<int>(() =>
        {
            trackCount++;
            // Enumerate twice
            var first = collection.AsTracked().Sum();
            var second = collection.AsTracked().Count();
            return first + second;
        });
        
        trackCount = 0;
        var result = computed.Value;
        Assert.Equal(1, trackCount); // Should only track once per computation
        Assert.Equal(9, result); // 6 + 3
    }

    [Fact]
    public void AsTracked_EmptyCollection_ReturnsEmpty()
    {
        var collection = new ObservableCollection<int>();
        
        var result = collection.AsTracked().ToList();
        
        Assert.Empty(result);
    }

    [Fact]
    public void AsTracked_WithProjection_AppliesCorrectly()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        
        var result = collection.AsTracked()
            .Select(x => x * 2)
            .ToList();
        
        Assert.Equal(new[] { 2, 4, 6 }, result);
    }

    [Fact]
    public void AsTracked_Aggregate_WorksCorrectly()
    {
        var collection = new ObservableCollection<string>(new[] { "a", "b", "c" });
        
        var result = collection.AsTracked()
            .Aggregate("", (acc, x) => acc + x);
        
        Assert.Equal("abc", result);
    }

    [Fact]
    public void AsTracked_LazyEvaluation_DoesNotTrackUntilEnumerated()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var wasTracked = false;
        
        var computed = new ComputedProperty<IEnumerable<int>>(() =>
        {
            wasTracked = true;
            // Return the enumerable without enumerating
            return collection.AsTracked().Where(x => x > 1);
        });
        
        wasTracked = false;
        var enumerable = computed.Value;
        Assert.True(wasTracked); // Tracked when getting the enumerable
        
        // Now enumerate
        var result = enumerable.ToList();
        Assert.Equal(new[] { 2, 3 }, result);
    }
}