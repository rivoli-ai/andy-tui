namespace Andy.TUI.Components;

/// <summary>
/// Provides context and services to components, including parent/child relationships,
/// service injection, theme access, and shared state management.
/// </summary>
public interface IComponentContext
{
    /// <summary>
    /// Gets the unique identifier for this context.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Gets the component that owns this context.
    /// </summary>
    IComponent Component { get; }
    
    /// <summary>
    /// Gets the parent context, if this context has a parent.
    /// </summary>
    IComponentContext? Parent { get; }
    
    /// <summary>
    /// Gets the collection of child contexts.
    /// </summary>
    IReadOnlyCollection<IComponentContext> Children { get; }
    
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    IServiceProvider Services { get; }
    
    /// <summary>
    /// Gets the theme provider for accessing theme resources.
    /// </summary>
    IThemeProvider Theme { get; }
    
    /// <summary>
    /// Gets the shared state manager for cross-component state.
    /// </summary>
    ISharedStateManager SharedState { get; }
    
    /// <summary>
    /// Adds a child context to this context.
    /// </summary>
    /// <param name="child">The child context to add.</param>
    void AddChild(IComponentContext child);
    
    /// <summary>
    /// Removes a child context from this context.
    /// </summary>
    /// <param name="child">The child context to remove.</param>
    /// <returns>True if the child was removed, false if it was not found.</returns>
    bool RemoveChild(IComponentContext child);
    
    /// <summary>
    /// Finds a child context by its component's ID.
    /// </summary>
    /// <param name="componentId">The ID of the component to find.</param>
    /// <returns>The child context if found, null otherwise.</returns>
    IComponentContext? FindChild(string componentId);
    
    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance, or null if not found.</returns>
    T? GetService<T>() where T : class;
    
    /// <summary>
    /// Gets a required service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not found.</exception>
    T GetRequiredService<T>() where T : class;
    
    /// <summary>
    /// Sets a value in the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    void SetSharedValue<T>(string key, T value);
    
    /// <summary>
    /// Gets a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value if found, default(T) otherwise.</returns>
    T? GetSharedValue<T>(string key);
    
    /// <summary>
    /// Tries to get a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">The value if found.</param>
    /// <returns>True if the value was found, false otherwise.</returns>
    bool TryGetSharedValue<T>(string key, out T? value);
}

/// <summary>
/// Provides access to theme resources and styling information.
/// </summary>
public interface IThemeProvider
{
    /// <summary>
    /// Gets the current theme name.
    /// </summary>
    string CurrentTheme { get; }
    
    /// <summary>
    /// Gets a color value from the current theme.
    /// </summary>
    /// <param name="key">The color key.</param>
    /// <returns>The color value, or null if not found.</returns>
    object? GetColor(string key);
    
    /// <summary>
    /// Gets a style value from the current theme.
    /// </summary>
    /// <param name="key">The style key.</param>
    /// <returns>The style value, or null if not found.</returns>
    object? GetStyle(string key);
    
    /// <summary>
    /// Gets a theme resource of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <returns>The resource value, or default(T) if not found.</returns>
    T? GetResource<T>(string key);
}

/// <summary>
/// Manages shared state across components.
/// </summary>
public interface ISharedStateManager
{
    /// <summary>
    /// Sets a value in the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    void SetValue<T>(string key, T value);
    
    /// <summary>
    /// Gets a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value if found, default(T) otherwise.</returns>
    T? GetValue<T>(string key);
    
    /// <summary>
    /// Tries to get a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">The value if found.</param>
    /// <returns>True if the value was found, false otherwise.</returns>
    bool TryGetValue<T>(string key, out T? value);
    
    /// <summary>
    /// Removes a value from the shared state.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns>True if the value was removed, false if it was not found.</returns>
    bool RemoveValue(string key);
    
    /// <summary>
    /// Clears all values from the shared state.
    /// </summary>
    void Clear();
    
    /// <summary>
    /// Occurs when a value in the shared state changes.
    /// </summary>
    event EventHandler<SharedStateChangedEventArgs>? ValueChanged;
}

/// <summary>
/// Event arguments for shared state changes.
/// </summary>
public class SharedStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the key of the value that changed.
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// Gets the old value.
    /// </summary>
    public object? OldValue { get; }
    
    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object? NewValue { get; }
    
    /// <summary>
    /// Initializes a new instance of the SharedStateChangedEventArgs class.
    /// </summary>
    public SharedStateChangedEventArgs(string key, object? oldValue, object? newValue)
    {
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
    }
}