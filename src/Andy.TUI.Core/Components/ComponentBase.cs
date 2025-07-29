using Andy.TUI.Core.Observable;
using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Core.Components;

/// <summary>
/// Base class for all UI components providing lifecycle management, state management, and property binding.
/// </summary>
public abstract class ComponentBase : IComponent
{
    private IComponentContext? _context;
    private readonly Dictionary<string, IObservableProperty> _observableProperties = new();
    private readonly Dictionary<string, object?> _properties = new();
    private readonly List<IDisposable> _subscriptions = new();
    private bool _disposed;
    private bool _updateRequested;
    
    /// <summary>
    /// Gets the unique identifier for this component instance.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets a value indicating whether this component has been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this component is currently mounted in the component tree.
    /// </summary>
    public bool IsMounted { get; private set; }
    
    /// <summary>
    /// Gets the parent component context, if this component has a parent.
    /// </summary>
    public IComponentContext? Parent => _context?.Parent;
    
    /// <summary>
    /// Gets the component context for this component.
    /// </summary>
    public IComponentContext Context => _context ?? throw new InvalidOperationException("Component has not been initialized");
    
    /// <summary>
    /// Occurs when the component requests to be re-rendered.
    /// </summary>
    public event EventHandler? RenderRequested;
    
    /// <summary>
    /// Occurs when the component's state changes.
    /// </summary>
    public event EventHandler? StateChanged;
    
    /// <summary>
    /// Initializes the component with the specified context.
    /// </summary>
    /// <param name="context">The component context.</param>
    public virtual void Initialize(IComponentContext context)
    {
        if (IsInitialized)
            throw new InvalidOperationException("Component has already been initialized");
        
        _context = context ?? throw new ArgumentNullException(nameof(context));
        IsInitialized = true;
        
        OnInitialize();
    }
    
    /// <summary>
    /// Called when the component is mounted in the component tree.
    /// </summary>
    public virtual void OnMount()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Component must be initialized before mounting");
        
        IsMounted = true;
        OnMounted();
    }
    
    /// <summary>
    /// Called when the component is unmounted from the component tree.
    /// </summary>
    public virtual void OnUnmount()
    {
        if (IsMounted)
        {
            IsMounted = false;
            OnUnmounted();
        }
    }
    
    /// <summary>
    /// Renders the component's virtual DOM representation.
    /// </summary>
    /// <returns>The virtual DOM node representing this component's UI.</returns>
    public VirtualNode Render()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Component must be initialized before rendering");
        
        var result = OnRender();
        OnAfterRender();
        return result;
    }
    
    /// <summary>
    /// Called when the component's state or properties have changed and it needs to update.
    /// </summary>
    public virtual void Update()
    {
        if (!IsInitialized)
            return;
        
        _updateRequested = false;
        OnUpdate();
    }
    
    /// <summary>
    /// Called before the component is re-rendered to determine if a re-render is necessary.
    /// </summary>
    /// <returns>True if the component should be re-rendered, false otherwise.</returns>
    public virtual bool ShouldUpdate()
    {
        return _updateRequested || OnShouldUpdate();
    }
    
    /// <summary>
    /// Called after the component has been rendered and the virtual DOM has been updated.
    /// </summary>
    public virtual void OnAfterRender()
    {
        OnAfterRendered();
    }
    
    /// <summary>
    /// Requests that this component be re-rendered on the next render cycle.
    /// </summary>
    public void RequestRender()
    {
        if (!IsInitialized || _disposed)
            return;
        
        _updateRequested = true;
        RenderRequested?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Creates an observable property for this component.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <param name="initialValue">The initial value of the property.</param>
    /// <returns>The observable property.</returns>
    protected ObservableProperty<T> CreateObservableProperty<T>(string name, T initialValue = default!)
    {
        if (_observableProperties.ContainsKey(name))
            throw new ArgumentException($"Property '{name}' already exists", nameof(name));
        
        var property = new ObservableProperty<T>(initialValue);
        _observableProperties[name] = property;
        
        // Subscribe to changes and request re-render
        var subscription = property.Subscribe(_ => 
        {
            OnPropertyChanged(name);
            RequestRender();
        });
        _subscriptions.Add(subscription);
        
        return property;
    }
    
    /// <summary>
    /// Gets an observable property by name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>The observable property.</returns>
    /// <exception cref="ArgumentException">Thrown if the property doesn't exist or has a different type.</exception>
    protected ObservableProperty<T> GetObservableProperty<T>(string name)
    {
        if (!_observableProperties.TryGetValue(name, out var property))
            throw new ArgumentException($"Property '{name}' not found", nameof(name));
        
        if (property is not ObservableProperty<T> typedProperty)
            throw new ArgumentException($"Property '{name}' is not of type {typeof(T).Name}", nameof(name));
        
        return typedProperty;
    }
    
    /// <summary>
    /// Sets a component property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    protected void SetProperty<T>(string name, T value)
    {
        var oldValue = _properties.TryGetValue(name, out var existing) ? existing : default(T);
        
        if (!EqualityComparer<T>.Default.Equals((T?)oldValue, value))
        {
            _properties[name] = value;
            OnPropertyChanged(name);
            RequestRender();
        }
    }
    
    /// <summary>
    /// Gets a component property value.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value if the property doesn't exist.</param>
    /// <returns>The property value.</returns>
    protected T GetProperty<T>(string name, T defaultValue = default!)
    {
        if (_properties.TryGetValue(name, out var value) && value is T typedValue)
            return typedValue;
        
        return defaultValue;
    }
    
    /// <summary>
    /// Subscribes to an observable and automatically disposes the subscription when the component is disposed.
    /// </summary>
    /// <typeparam name="T">The type of the observable values.</typeparam>
    /// <param name="observable">The observable to subscribe to.</param>
    /// <param name="onNext">The action to execute when a new value is received.</param>
    /// <returns>The subscription disposable.</returns>
    protected IDisposable Subscribe<T>(IObservableProperty<T> observable, Action<T> onNext)
    {
        var subscription = observable.Subscribe(onNext);
        _subscriptions.Add(subscription);
        return subscription;
    }
    
    /// <summary>
    /// Called during initialization. Override to perform custom initialization logic.
    /// </summary>
    protected virtual void OnInitialize() { }
    
    /// <summary>
    /// Called when the component is mounted. Override to perform mounting logic.
    /// </summary>
    protected virtual void OnMounted() { }
    
    /// <summary>
    /// Called when the component is unmounted. Override to perform cleanup logic.
    /// </summary>
    protected virtual void OnUnmounted() { }
    
    /// <summary>
    /// Called to render the component. Override to provide the component's UI.
    /// </summary>
    /// <returns>The virtual DOM node representing this component's UI.</returns>
    protected abstract VirtualNode OnRender();
    
    /// <summary>
    /// Called when the component needs to update. Override to perform update logic.
    /// </summary>
    protected virtual void OnUpdate() { }
    
    /// <summary>
    /// Called to determine if the component should update. Override to provide custom update logic.
    /// </summary>
    /// <returns>True if the component should update, false otherwise.</returns>
    protected virtual bool OnShouldUpdate() => false;
    
    /// <summary>
    /// Called after the component has been rendered. Override to perform post-render logic.
    /// </summary>
    protected virtual void OnAfterRendered() { }
    
    /// <summary>
    /// Called when a property value changes. Override to handle property changes.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Disposes the component and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Disposes the component resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            OnUnmount();
            
            // Dispose all subscriptions
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
            
            // Dispose all observable properties
            foreach (var property in _observableProperties.Values)
            {
                if (property is IDisposable disposableProperty)
                    disposableProperty.Dispose();
            }
            _observableProperties.Clear();
            
            _properties.Clear();
            
            OnDispose();
        }
        
        _disposed = true;
    }
    
    /// <summary>
    /// Called when the component is being disposed. Override to perform custom cleanup.
    /// </summary>
    protected virtual void OnDispose() { }
}