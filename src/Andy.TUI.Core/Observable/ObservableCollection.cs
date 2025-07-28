using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Andy.TUI.Core.Observable;

/// <summary>
/// An observable collection that extends the standard ObservableCollection with additional features
/// including batch operations, notification suspension, and integration with Andy.TUI's observable system.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class ObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged, IObservableProperty, IDisposable
{
    private readonly object _lock = new();
    private readonly System.Collections.ObjectModel.ObservableCollection<T> _innerCollection;
    private readonly List<IPropertyObserver> _observers = new();
    private int _suspendCount;
    private bool _disposed;
    private List<NotifyCollectionChangedEventArgs>? _pendingChanges;

    /// <summary>
    /// Gets the current items as a read-only list.
    /// </summary>
    public new IReadOnlyList<T> Items 
    { 
        get
        {
            // Track this property access if we're in a computed property context
            DependencyTracker.Current?.TrackDependency(this);
            return _innerCollection;
        }
    }

    /// <inheritdoc />
    public object? Value => Items;

    /// <inheritdoc />
    public bool HasObservers
    {
        get
        {
            lock (_lock)
            {
                return _observers.Count > 0 || PropertyChanged != null || CollectionChanged != null;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether notifications are currently suspended.
    /// </summary>
    public bool AreNotificationsSuspended => _suspendCount > 0;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc />
    event EventHandler<PropertyChangedEventArgs>? IObservableProperty.PropertyChanged
    {
        add => PropertyChangedCore += value;
        remove => PropertyChangedCore -= value;
    }

    private event EventHandler<PropertyChangedEventArgs>? PropertyChangedCore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class.
    /// </summary>
    public ObservableCollection() : base(new System.Collections.ObjectModel.ObservableCollection<T>())
    {
        _innerCollection = (System.Collections.ObjectModel.ObservableCollection<T>)base.Items;
        SubscribeToInnerCollection();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class with the specified items.
    /// </summary>
    /// <param name="collection">The items to add to the collection.</param>
    public ObservableCollection(IEnumerable<T> collection) : base(new System.Collections.ObjectModel.ObservableCollection<T>(collection))
    {
        _innerCollection = (System.Collections.ObjectModel.ObservableCollection<T>)base.Items;
        SubscribeToInnerCollection();
    }

    /// <summary>
    /// Adds a range of items to the collection.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfDisposed();

        var itemsList = items.ToList();
        if (itemsList.Count == 0) return;

        using (SuspendNotifications())
        {
            foreach (var item in itemsList)
            {
                Add(item);
            }
        }
        // The suspension will fire a single reset event when disposed
    }

    /// <summary>
    /// Removes a range of items from the collection.
    /// </summary>
    /// <param name="items">The items to remove.</param>
    /// <returns>The number of items removed.</returns>
    public int RemoveRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfDisposed();

        var itemsList = items.ToList();
        if (itemsList.Count == 0) return 0;

        int removed = 0;
        using (SuspendNotifications())
        {
            foreach (var item in itemsList)
            {
                if (Remove(item))
                    removed++;
            }
        }

        // The suspension will fire a single reset event when disposed if items were removed
        return removed;
    }

    /// <summary>
    /// Removes all items that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match items against.</param>
    /// <returns>The number of items removed.</returns>
    public int RemoveAll(Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ThrowIfDisposed();

        var toRemove = this.Where(item => predicate(item)).ToList();
        return RemoveRange(toRemove);
    }

    /// <summary>
    /// Replaces an item at the specified index.
    /// </summary>
    /// <param name="index">The index of the item to replace.</param>
    /// <param name="item">The new item.</param>
    /// <returns>The replaced item.</returns>
    public T Replace(int index, T item)
    {
        ThrowIfDisposed();
        
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var oldItem = this[index];
        this[index] = item;
        return oldItem;
    }

    /// <summary>
    /// Moves an item from one index to another.
    /// </summary>
    /// <param name="oldIndex">The current index of the item.</param>
    /// <param name="newIndex">The new index for the item.</param>
    public void Move(int oldIndex, int newIndex)
    {
        ThrowIfDisposed();
        
        if (oldIndex < 0 || oldIndex >= Count)
            throw new ArgumentOutOfRangeException(nameof(oldIndex));
        if (newIndex < 0 || newIndex >= Count)
            throw new ArgumentOutOfRangeException(nameof(newIndex));
        
        if (oldIndex == newIndex) return;

        var item = this[oldIndex];
        RemoveAt(oldIndex);
        Insert(newIndex, item);
    }

    /// <summary>
    /// Suspends change notifications until the returned disposable is disposed.
    /// </summary>
    /// <returns>A disposable that resumes notifications when disposed.</returns>
    public IDisposable SuspendNotifications()
    {
        ThrowIfDisposed();
        
        lock (_lock)
        {
            if (_suspendCount == 0)
            {
                _pendingChanges = new List<NotifyCollectionChangedEventArgs>();
            }
            _suspendCount++;
        }

        return new NotificationSuspender(this);
    }

    /// <inheritdoc />
    public void AddObserver(IPropertyObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        ThrowIfDisposed();

        lock (_lock)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
    }

    /// <inheritdoc />
    public void RemoveObserver(IPropertyObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);

        lock (_lock)
        {
            _observers.Remove(observer);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            if (_disposed) return;

            UnsubscribeFromInnerCollection();
            _observers.Clear();
            _pendingChanges?.Clear();
            _disposed = true;
        }

        PropertyChanged = null;
        CollectionChanged = null;
        PropertyChangedCore = null;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets an enumerator that tracks dependency access.
    /// </summary>
    public new IEnumerator<T> GetEnumerator()
    {
        // Track this collection access if we're in a computed property context
        DependencyTracker.Current?.TrackDependency(this);
        return base.GetEnumerator();
    }

    protected override void ClearItems()
    {
        ThrowIfDisposed();
        
        // Track this property access if we're in a computed property context
        DependencyTracker.Current?.TrackDependency(this);
        
        lock (_lock)
        {
            base.ClearItems();
        }
    }

    protected override void InsertItem(int index, T item)
    {
        ThrowIfDisposed();
        
        // Track this property access if we're in a computed property context
        DependencyTracker.Current?.TrackDependency(this);
        
        lock (_lock)
        {
            base.InsertItem(index, item);
        }
    }

    protected override void RemoveItem(int index)
    {
        ThrowIfDisposed();
        
        // Track this property access if we're in a computed property context
        DependencyTracker.Current?.TrackDependency(this);
        
        lock (_lock)
        {
            base.RemoveItem(index);
        }
    }

    protected override void SetItem(int index, T item)
    {
        ThrowIfDisposed();
        
        // Track this property access if we're in a computed property context
        DependencyTracker.Current?.TrackDependency(this);
        
        lock (_lock)
        {
            base.SetItem(index, item);
        }
    }

    private void SubscribeToInnerCollection()
    {
        _innerCollection.CollectionChanged += OnInnerCollectionChanged;
        ((INotifyPropertyChanged)_innerCollection).PropertyChanged += OnInnerPropertyChanged;
    }

    private void UnsubscribeFromInnerCollection()
    {
        _innerCollection.CollectionChanged -= OnInnerCollectionChanged;
        ((INotifyPropertyChanged)_innerCollection).PropertyChanged -= OnInnerPropertyChanged;
    }

    private void OnInnerCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnCollectionChanged(e);
    }

    private void OnInnerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(e);
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (_suspendCount > 0)
        {
            lock (_lock)
            {
                _pendingChanges?.Add(e);
            }
            return;
        }

        CollectionChanged?.Invoke(this, e);
        
        // Notify property observers
        var propertyArgs = new PropertyChangedEventArgs("Items", null, Items);
        NotifyPropertyObservers(propertyArgs);
    }

    protected virtual void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    private void NotifyPropertyObservers(PropertyChangedEventArgs args)
    {
        IPropertyObserver[] observers;
        lock (_lock)
        {
            observers = _observers.ToArray();
        }

        foreach (var observer in observers)
        {
            try
            {
                observer.OnPropertyChanged(this, args);
            }
            catch
            {
                // Ignore observer exceptions
            }
        }

        PropertyChangedCore?.Invoke(this, args);
    }

    private void ResumeNotifications()
    {
        List<NotifyCollectionChangedEventArgs>? pendingChanges = null;
        
        lock (_lock)
        {
            _suspendCount--;
            if (_suspendCount == 0 && _pendingChanges != null && _pendingChanges.Count > 0)
            {
                pendingChanges = _pendingChanges;
                _pendingChanges = null;
            }
        }

        // Fire a reset event if we had pending changes
        if (pendingChanges != null)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ObservableCollection<T>));
        }
    }

    private class NotificationSuspender : IDisposable
    {
        private readonly ObservableCollection<T> _collection;
        private bool _disposed;

        public NotificationSuspender(ObservableCollection<T> collection)
        {
            _collection = collection;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _collection.ResumeNotifications();
                _disposed = true;
            }
        }
    }
}