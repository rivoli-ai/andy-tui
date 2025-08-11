namespace Andy.TUI.Observable;

/// <summary>
/// Tracks dependencies between observable properties for computed properties.
/// </summary>
public class DependencyTracker
{
    [ThreadStatic]
    private static DependencyTracker? _current;

    private readonly HashSet<IObservableProperty> _dependencies = new();

    /// <summary>
    /// Gets the current dependency tracker for the thread.
    /// </summary>
    public static DependencyTracker? Current => _current;

    /// <summary>
    /// Gets the tracked dependencies.
    /// </summary>
    public IReadOnlySet<IObservableProperty> Dependencies => _dependencies;

    /// <summary>
    /// Begins tracking dependencies.
    /// </summary>
    /// <returns>A disposable that ends tracking when disposed.</returns>
    public static IDisposable BeginTracking()
    {
        var tracker = new DependencyTracker();
        var previous = _current;
        _current = tracker;

        return new TrackingScope(tracker, previous);
    }

    /// <summary>
    /// Tracks a dependency on an observable property.
    /// </summary>
    /// <param name="property">The property to track.</param>
    public void TrackDependency(IObservableProperty property)
    {
        ArgumentNullException.ThrowIfNull(property);
        _dependencies.Add(property);
    }

    /// <summary>
    /// Clears all tracked dependencies.
    /// </summary>
    public void Clear()
    {
        _dependencies.Clear();
    }

    private class TrackingScope : IDisposable
    {
        private readonly DependencyTracker _tracker;
        private readonly DependencyTracker? _previous;
        private bool _disposed;

        public TrackingScope(DependencyTracker tracker, DependencyTracker? previous)
        {
            _tracker = tracker;
            _previous = previous;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _current = _previous;
                _disposed = true;
            }
        }
    }
}