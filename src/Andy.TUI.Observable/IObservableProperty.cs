namespace Andy.TUI.Observable;

/// <summary>
/// Represents a property that notifies observers when its value changes.
/// </summary>
public interface IObservableProperty
{
    /// <summary>
    /// Gets the current value of the property.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Gets a value indicating whether this property has any observers.
    /// </summary>
    bool HasObservers { get; }

    /// <summary>
    /// Occurs when the property value changes.
    /// </summary>
    event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

    /// <summary>
    /// Adds an observer to this property.
    /// </summary>
    /// <param name="observer">The observer to add.</param>
    void AddObserver(IPropertyObserver observer);

    /// <summary>
    /// Removes an observer from this property.
    /// </summary>
    /// <param name="observer">The observer to remove.</param>
    void RemoveObserver(IPropertyObserver observer);

    /// <summary>
    /// Disposes of the property and cleans up resources.
    /// </summary>
    void Dispose();
}

/// <summary>
/// Represents a strongly-typed observable property.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public interface IObservableProperty<T> : IObservableProperty
{
    /// <summary>
    /// Gets or sets the value of the property.
    /// </summary>
    new T Value { get; set; }

    /// <summary>
    /// Occurs when the property value changes, providing old and new values.
    /// </summary>
    event EventHandler<PropertyChangedEventArgs<T>>? ValueChanged;

    /// <summary>
    /// Subscribes to value changes with a callback.
    /// </summary>
    /// <param name="callback">The callback to invoke when the value changes.</param>
    /// <returns>A disposable that unsubscribes when disposed.</returns>
    IDisposable Subscribe(Action<T> callback);

    /// <summary>
    /// Observes the property value, invoking the callback immediately and on changes.
    /// </summary>
    /// <param name="callback">The callback to invoke.</param>
    /// <returns>A disposable that stops observation when disposed.</returns>
    IDisposable Observe(Action<T> callback);
}

/// <summary>
/// Represents an observer of property changes.
/// </summary>
public interface IPropertyObserver
{
    /// <summary>
    /// Called when an observed property changes.
    /// </summary>
    /// <param name="property">The property that changed.</param>
    /// <param name="args">The change event arguments.</param>
    void OnPropertyChanged(IObservableProperty property, PropertyChangedEventArgs args);
}

/// <summary>
/// Provides data for property changed events.
/// </summary>
public class PropertyChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the property that changed.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the old value of the property.
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// Gets the new value of the property.
    /// </summary>
    public object? NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChangedEventArgs"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public PropertyChangedEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Provides strongly-typed data for property changed events.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public class PropertyChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets the name of the property that changed.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the old value of the property.
    /// </summary>
    public T OldValue { get; }

    /// <summary>
    /// Gets the new value of the property.
    /// </summary>
    public T NewValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChangedEventArgs{T}"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public PropertyChangedEventArgs(string propertyName, T oldValue, T newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}