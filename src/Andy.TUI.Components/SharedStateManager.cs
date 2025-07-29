using System.Collections.Concurrent;

namespace Andy.TUI.Components;

/// <summary>
/// Default implementation of ISharedStateManager that manages shared state across components.
/// </summary>
public class SharedStateManager : ISharedStateManager
{
    private readonly ConcurrentDictionary<string, object?> _state = new();
    
    /// <summary>
    /// Occurs when a value in the shared state changes.
    /// </summary>
    public event EventHandler<SharedStateChangedEventArgs>? ValueChanged;
    
    /// <summary>
    /// Sets a value in the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    public void SetValue<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        var oldValue = _state.TryGetValue(key, out var existing) ? existing : default(T);
        _state[key] = value;
        
        // Only fire event if value actually changed
        if (!EqualityComparer<T>.Default.Equals((T?)oldValue, value))
        {
            ValueChanged?.Invoke(this, new SharedStateChangedEventArgs(key, oldValue, value));
        }
    }
    
    /// <summary>
    /// Gets a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value if found, default(T) otherwise.</returns>
    public T? GetValue<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            return default(T);
        
        if (_state.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        
        return default(T);
    }
    
    /// <summary>
    /// Tries to get a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">The value if found.</param>
    /// <returns>True if the value was found, false otherwise.</returns>
    public bool TryGetValue<T>(string key, out T? value)
    {
        value = default(T);
        
        if (string.IsNullOrEmpty(key))
            return false;
        
        if (_state.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Removes a value from the shared state.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns>True if the value was removed, false if it was not found.</returns>
    public bool RemoveValue(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
        
        if (_state.TryRemove(key, out var oldValue))
        {
            ValueChanged?.Invoke(this, new SharedStateChangedEventArgs(key, oldValue, null));
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Clears all values from the shared state.
    /// </summary>
    public void Clear()
    {
        var keys = _state.Keys.ToList();
        _state.Clear();
        
        // Fire events for all cleared values
        foreach (var key in keys)
        {
            ValueChanged?.Invoke(this, new SharedStateChangedEventArgs(key, null, null));
        }
    }
}