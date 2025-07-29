namespace Andy.TUI.Core.Components;

/// <summary>
/// Default implementation of IComponentContext providing context and services to components.
/// </summary>
public class ComponentContext : IComponentContext
{
    private readonly List<IComponentContext> _children = new();
    private readonly IServiceProvider _services;
    private readonly IThemeProvider _theme;
    private readonly ISharedStateManager _sharedState;
    
    /// <summary>
    /// Gets the unique identifier for this context.
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// Gets the component that owns this context.
    /// </summary>
    public IComponent Component { get; }
    
    /// <summary>
    /// Gets the parent context, if this context has a parent.
    /// </summary>
    public IComponentContext? Parent { get; }
    
    /// <summary>
    /// Gets the collection of child contexts.
    /// </summary>
    public IReadOnlyCollection<IComponentContext> Children => _children.AsReadOnly();
    
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider Services => _services;
    
    /// <summary>
    /// Gets the theme provider for accessing theme resources.
    /// </summary>
    public IThemeProvider Theme => _theme;
    
    /// <summary>
    /// Gets the shared state manager for cross-component state.
    /// </summary>
    public ISharedStateManager SharedState => _sharedState;
    
    /// <summary>
    /// Initializes a new instance of the ComponentContext class.
    /// </summary>
    /// <param name="component">The component that owns this context.</param>
    /// <param name="services">The service provider.</param>
    /// <param name="theme">The theme provider.</param>
    /// <param name="sharedState">The shared state manager.</param>
    /// <param name="parent">The parent context, if any.</param>
    public ComponentContext(
        IComponent component,
        IServiceProvider services,
        IThemeProvider theme,
        ISharedStateManager sharedState,
        IComponentContext? parent = null)
    {
        Component = component ?? throw new ArgumentNullException(nameof(component));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _sharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
        Parent = parent;
        Id = Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Adds a child context to this context.
    /// </summary>
    /// <param name="child">The child context to add.</param>
    public void AddChild(IComponentContext child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));
        
        if (!_children.Contains(child))
        {
            _children.Add(child);
        }
    }
    
    /// <summary>
    /// Removes a child context from this context.
    /// </summary>
    /// <param name="child">The child context to remove.</param>
    /// <returns>True if the child was removed, false if it was not found.</returns>
    public bool RemoveChild(IComponentContext child)
    {
        if (child == null)
            return false;
        
        return _children.Remove(child);
    }
    
    /// <summary>
    /// Finds a child context by its component's ID.
    /// </summary>
    /// <param name="componentId">The ID of the component to find.</param>
    /// <returns>The child context if found, null otherwise.</returns>
    public IComponentContext? FindChild(string componentId)
    {
        if (string.IsNullOrEmpty(componentId))
            return null;
        
        return _children.FirstOrDefault(c => c.Component.Id == componentId);
    }
    
    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance, or null if not found.</returns>
    public T? GetService<T>() where T : class
    {
        return _services.GetService(typeof(T)) as T;
    }
    
    /// <summary>
    /// Gets a required service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service to get.</typeparam>
    /// <returns>The service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not found.</exception>
    public T GetRequiredService<T>() where T : class
    {
        var service = GetService<T>();
        if (service == null)
            throw new InvalidOperationException($"Required service of type {typeof(T).Name} was not found");
        
        return service;
    }
    
    /// <summary>
    /// Sets a value in the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    public void SetSharedValue<T>(string key, T value)
    {
        _sharedState.SetValue(key, value);
    }
    
    /// <summary>
    /// Gets a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value if found, default(T) otherwise.</returns>
    public T? GetSharedValue<T>(string key)
    {
        return _sharedState.GetValue<T>(key);
    }
    
    /// <summary>
    /// Tries to get a value from the shared state.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">The value if found.</param>
    /// <returns>True if the value was found, false otherwise.</returns>
    public bool TryGetSharedValue<T>(string key, out T? value)
    {
        return _sharedState.TryGetValue(key, out value);
    }
}