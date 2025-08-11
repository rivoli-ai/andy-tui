namespace Andy.TUI.Observable;

/// <summary>
/// Implementation of an observable property that notifies observers when its value changes.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public class ObservableProperty<T> : IObservableProperty<T>, IDisposable
{
    private readonly object _lock = new();
    private readonly List<IPropertyObserver> _observers = new();
    private readonly List<WeakReference<Action<T>>> _callbacks = new();
    private readonly IEqualityComparer<T> _comparer;
    private readonly string _propertyName;
    private T _value;
    private bool _disposed;

    /// <inheritdoc />
    public T Value
    {
        get
        {
            ThrowIfDisposed();

            // Track this property access if we're in a computed property context
            DependencyTracker.Current?.TrackDependency(this);

            lock (_lock)
            {
                return _value;
            }
        }
        set
        {
            ThrowIfDisposed();
            T oldValue;
            T newValue;

            lock (_lock)
            {
                if (_comparer.Equals(_value, value))
                {
                    return; // No change
                }

                oldValue = _value;
                _value = value;
                newValue = value;
            }

            // Notify outside of lock to avoid deadlocks
            OnValueChanged(oldValue, newValue);
        }
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
    public event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

    /// <inheritdoc />
    public event EventHandler<PropertyChangedEventArgs<T>>? ValueChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableProperty{T}"/> class.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="comparer">The equality comparer to use for change detection.</param>
    public ObservableProperty(T initialValue = default!, string propertyName = "", IEqualityComparer<T>? comparer = null)
    {
        _value = initialValue;
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
        T currentValue;
        lock (_lock)
        {
            currentValue = _value;
        }
        callback(currentValue);

        // Then subscribe for future changes
        return Subscribe(callback);
    }

    /// <summary>
    /// Sets the value without triggering change notifications.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void SetValueSilently(T value)
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            _value = value;
        }
    }

    /// <summary>
    /// Forces a change notification even if the value hasn't changed.
    /// </summary>
    public void ForceNotify()
    {
        ThrowIfDisposed();

        T currentValue;
        lock (_lock)
        {
            currentValue = _value;
        }

        OnValueChanged(currentValue, currentValue);
    }

    /// <summary>
    /// Creates an observable property with the specified initial value.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>A new observable property.</returns>
    public static ObservableProperty<T> Create(T initialValue = default!, string propertyName = "")
    {
        return new ObservableProperty<T>(initialValue, propertyName);
    }

    /// <summary>
    /// Called when the value changes to notify observers.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnValueChanged(T oldValue, T newValue)
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
                // Ignore observer exceptions to prevent one bad observer from breaking others
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

            _observers.Clear();
            _callbacks.Clear();
            _disposed = true;
        }

        PropertyChanged = null;
        ValueChanged = null;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ObservableProperty<T>));
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

    /// <inheritdoc />
    void IObservableProperty.Dispose() => Dispose();

    /// <summary>
    /// Implicitly converts an observable property to its value.
    /// </summary>
    /// <param name="property">The observable property.</param>
    public static implicit operator T(ObservableProperty<T> property)
    {
        ArgumentNullException.ThrowIfNull(property);
        return property.Value;
    }

    private class Subscription : IDisposable
    {
        private readonly ObservableProperty<T> _property;
        private readonly WeakReference<Action<T>> _callbackRef;
        private bool _disposed;

        public Subscription(ObservableProperty<T> property, WeakReference<Action<T>> callbackRef)
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