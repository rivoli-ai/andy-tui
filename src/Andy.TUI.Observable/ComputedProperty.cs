namespace Andy.TUI.Observable;

/// <summary>
/// A computed property that automatically recalculates when its dependencies change.
/// </summary>
/// <typeparam name="T">The type of the computed value.</typeparam>
public class ComputedProperty<T> : IComputedProperty<T>, IPropertyObserver, IDisposable
{
    private readonly object _lock = new();
    private readonly List<IPropertyObserver> _observers = new();
    private readonly List<WeakReference<Action<T>>> _callbacks = new();
    private readonly IEqualityComparer<T> _comparer;
    private readonly string _propertyName;
    private readonly Func<T> _computation;
    private readonly HashSet<IObservableProperty> _dependencies = new();
    private T _cachedValue = default!;
    private bool _isValid;
    private bool _isComputing;
    private bool _isNotifying;
    private bool _disposed;

    /// <inheritdoc />
    public T Value
    {
        get
        {
            ThrowIfDisposed();

            // Track this property access if we're in a computed property context
            DependencyTracker.Current?.TrackDependency(this);

            bool needsUpdate = false;
            lock (_lock)
            {
                needsUpdate = !_isValid && !_isComputing;
            }

            if (needsUpdate)
            {
                UpdateValue();
            }

            lock (_lock)
            {
                return _cachedValue;
            }
        }
        set => throw new InvalidOperationException("Cannot set the value of a computed property directly. Modify its dependencies instead.");
    }

    /// <inheritdoc />
    object? IObservableProperty.Value => Value;

    /// <inheritdoc />
    public bool HasObservers
    {
        get
        {
            lock (_lock)
            {
                CleanupCallbacks();
                return _observers.Count > 0 || _callbacks.Count > 0 || PropertyChanged != null || ValueChanged != null;
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlySet<IObservableProperty> Dependencies
    {
        get
        {
            lock (_lock)
            {
                return new HashSet<IObservableProperty>(_dependencies);
            }
        }
    }

    /// <inheritdoc />
    public Func<T> Computation => _computation;

    /// <inheritdoc />
    public bool IsValid
    {
        get
        {
            lock (_lock)
            {
                return _isValid;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

    /// <inheritdoc />
    public event EventHandler<PropertyChangedEventArgs<T>>? ValueChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComputedProperty{T}"/> class.
    /// </summary>
    /// <param name="computation">The function to compute the value.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="comparer">The equality comparer to use for change detection.</param>
    public ComputedProperty(Func<T> computation, string propertyName = "", IEqualityComparer<T>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(computation);
        _computation = computation;
        _propertyName = propertyName;
        _comparer = comparer ?? EqualityComparer<T>.Default;
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
    public IDisposable Subscribe(Action<T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ThrowIfDisposed();

        var weakRef = new WeakReference<Action<T>>(callback);

        lock (_lock)
        {
            _callbacks.Add(weakRef);
        }

        return new Subscription(this, weakRef);
    }

    /// <inheritdoc />
    public IDisposable Observe(Action<T> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ThrowIfDisposed();

        // Invoke immediately with current value
        callback(Value);

        // Then subscribe for future changes
        return Subscribe(callback);
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (!_isValid)
            {
                return; // Already invalid
            }
            _isValid = false;
        }

        // If we have observers, trigger recomputation
        if (HasObservers)
        {
            _ = Value; // This will recompute and notify if needed
        }
    }

    /// <inheritdoc />
    public void OnPropertyChanged(IObservableProperty property, PropertyChangedEventArgs args)
    {
        // One of our dependencies changed, invalidate our cached value
        lock (_lock)
        {
            if (!_isValid)
            {
                return; // Already invalid
            }
            _isValid = false;
        }

        // If we have observers, trigger recomputation
        if (HasObservers)
        {
            _ = Value; // This will recompute and notify if needed
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            // Unsubscribe from all dependencies
            foreach (var dependency in _dependencies)
            {
                dependency.RemoveObserver(this);
            }
            _dependencies.Clear();

            _observers.Clear();
            _callbacks.Clear();
            _disposed = true;
        }

        PropertyChanged = null;
        ValueChanged = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    void IObservableProperty.Dispose() => Dispose();

    private void UpdateValue()
    {
        bool wasComputing;
        lock (_lock)
        {
            wasComputing = _isComputing;
            if (wasComputing)
            {
                throw new InvalidOperationException($"Circular dependency detected in computed property '{_propertyName}'");
            }
            _isComputing = true;
        }
        T oldValue;
        T newValue;
        bool valueChanged = false;
        HashSet<IObservableProperty> oldDependencies;
        HashSet<IObservableProperty> newDependencies;

        try
        {
            lock (_lock)
            {
                oldValue = _isValid ? _cachedValue : default!;
            }

            // Track dependencies during computation
            using (var tracker = DependencyTracker.BeginTracking())
            {
                newValue = _computation();
                newDependencies = new HashSet<IObservableProperty>(DependencyTracker.Current!.Dependencies);
            }

            // Update dependencies and value
            lock (_lock)
            {
                oldDependencies = new HashSet<IObservableProperty>(_dependencies);

                // Check if value changed
                if (_isValid && !_comparer.Equals(oldValue, newValue))
                {
                    valueChanged = true;
                }
                else if (!_isValid)
                {
                    // First computation
                    valueChanged = true;
                    oldValue = _cachedValue;
                }

                _cachedValue = newValue;

                // Remove old dependencies
                var toRemove = oldDependencies.Except(newDependencies);
                foreach (var dep in toRemove)
                {
                    dep.RemoveObserver(this);
                    _dependencies.Remove(dep);
                }

                // Add new dependencies
                var toAdd = newDependencies.Except(oldDependencies);
                foreach (var dep in toAdd)
                {
                    dep.AddObserver(this);
                    _dependencies.Add(dep);
                }

                _isValid = true;
            }
        }
        finally
        {
            lock (_lock)
            {
                _isComputing = false;
            }
        }

        // Notify observers outside of lock if value changed
        if (valueChanged)
        {
            OnValueChanged(oldValue, newValue);
        }
    }

    private void OnValueChanged(T oldValue, T newValue)
    {
        // Prevent re-entrant notifications
        lock (_lock)
        {
            if (_isNotifying)
            {
                return;
            }
            _isNotifying = true;
        }

        try
        {
            var genericArgs = new PropertyChangedEventArgs(_propertyName, oldValue, newValue);
            var typedArgs = new PropertyChangedEventArgs<T>(_propertyName, oldValue, newValue);

            // Get observers snapshot to avoid modifications during iteration
            IPropertyObserver[] observers;
            List<Action<T>> callbacks;

            lock (_lock)
            {
                observers = _observers.ToArray();
                callbacks = GetActiveCallbacks();
            }

            // Notify observers
            foreach (var observer in observers)
            {
                try
                {
                    observer.OnPropertyChanged(this, genericArgs);
                }
                catch (Exception)
                {
                    // Ignore observer exceptions
                }
            }

            // Notify callbacks
            foreach (var callback in callbacks)
            {
                try
                {
                    callback(newValue);
                }
                catch (Exception)
                {
                    // Ignore callback exceptions
                }
            }

            // Fire events
            try
            {
                PropertyChanged?.Invoke(this, genericArgs);
                ValueChanged?.Invoke(this, typedArgs);
            }
            catch (Exception)
            {
                // Ignore event handler exceptions
            }
        }
        finally
        {
            lock (_lock)
            {
                _isNotifying = false;
            }
        }
    }

    private List<Action<T>> GetActiveCallbacks()
    {
        var active = new List<Action<T>>();
        _callbacks.RemoveAll(weakRef =>
        {
            if (weakRef.TryGetTarget(out var callback))
            {
                active.Add(callback);
                return false;
            }
            return true; // Remove dead references
        });
        return active;
    }

    private void CleanupCallbacks()
    {
        _callbacks.RemoveAll(weakRef => !weakRef.TryGetTarget(out _));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ComputedProperty<T>));
        }
    }

    private class Subscription : IDisposable
    {
        private readonly ComputedProperty<T> _property;
        private readonly WeakReference<Action<T>> _callbackRef;
        private bool _disposed;

        public Subscription(ComputedProperty<T> property, WeakReference<Action<T>> callbackRef)
        {
            _property = property;
            _callbackRef = callbackRef;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                lock (_property._lock)
                {
                    _property._callbacks.Remove(_callbackRef);
                }
                _disposed = true;
            }
        }
    }
}