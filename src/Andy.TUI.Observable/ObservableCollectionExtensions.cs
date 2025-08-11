using System.Collections;

namespace Andy.TUI.Observable;

/// <summary>
/// Extension methods for ObservableCollection to ensure proper dependency tracking with LINQ operations.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Creates an enumerable that tracks the collection as a dependency when enumerated.
    /// </summary>
    public static IEnumerable<T> AsTracked<T>(this ObservableCollection<T> collection)
    {
        // Track the collection access
        DependencyTracker.Current?.TrackDependency(collection);

        // Return the items
        foreach (var item in collection)
        {
            yield return item;
        }
    }
}

/// <summary>
/// A wrapper that ensures dependency tracking for collection operations.
/// </summary>
internal class TrackedEnumerable<T> : IEnumerable<T>
{
    private readonly IEnumerable<T> _source;
    private readonly IObservableProperty _property;

    public TrackedEnumerable(IEnumerable<T> source, IObservableProperty property)
    {
        _source = source;
        _property = property;
    }

    public IEnumerator<T> GetEnumerator()
    {
        // Track the dependency when enumeration begins
        DependencyTracker.Current?.TrackDependency(_property);
        return _source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}