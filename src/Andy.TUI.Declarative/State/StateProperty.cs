using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Andy.TUI.Declarative.State;

/// <summary>
/// A reactive property wrapper that notifies observers when its value changes.
/// Similar to SwiftUI's @State property wrapper functionality.
/// </summary>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public class StateProperty<T> : INotifyPropertyChanged
{
    private T _value;
    private readonly string _propertyName;

    /// <summary>
    /// Gets or sets the wrapped value.
    /// Setting the value will trigger PropertyChanged if the value actually changes.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                var oldValue = _value;
                _value = value;
                OnPropertyChanged();
                ValueChanged?.Invoke(this, new StateChangedEventArgs<T>(oldValue, value, _propertyName));
            }
        }
    }

    /// <summary>
    /// Occurs when the Value property changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Occurs when the value changes, providing both old and new values.
    /// </summary>
    public event EventHandler<StateChangedEventArgs<T>>? ValueChanged;

    /// <summary>
    /// Initializes a new StateProperty with a default value.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="propertyName">The name of the property for debugging.</param>
    public StateProperty(T initialValue = default!, [CallerMemberName] string propertyName = "")
    {
        _value = initialValue;
        _propertyName = propertyName;
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Implicitly converts a StateProperty to its wrapped value.
    /// </summary>
    public static implicit operator T(StateProperty<T> stateProperty) => stateProperty.Value;

    /// <summary>
    /// Implicitly converts a value to a StateProperty.
    /// </summary>
    public static implicit operator StateProperty<T>(T value) => new(value);

    /// <summary>
    /// Returns the string representation of the wrapped value.
    /// </summary>
    public override string ToString() => _value?.ToString() ?? "";

    /// <summary>
    /// Returns the hash code of the wrapped value.
    /// </summary>
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    /// <summary>
    /// Determines whether this StateProperty equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is StateProperty<T> other)
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        
        if (obj is T directValue)
            return EqualityComparer<T>.Default.Equals(_value, directValue);

        return false;
    }
}

/// <summary>
/// Event arguments for state change events.
/// </summary>
/// <typeparam name="T">The type of the state value.</typeparam>
public class StateChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets the previous value.
    /// </summary>
    public T OldValue { get; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public T NewValue { get; }

    /// <summary>
    /// Gets the name of the property that changed.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Initializes a new StateChangedEventArgs.
    /// </summary>
    public StateChangedEventArgs(T oldValue, T newValue, string propertyName)
    {
        OldValue = oldValue;
        NewValue = newValue;
        PropertyName = propertyName;
    }
}