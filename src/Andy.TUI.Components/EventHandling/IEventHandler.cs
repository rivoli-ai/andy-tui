namespace Andy.TUI.Components.EventHandling;

/// <summary>
/// Defines the contract for handling component events.
/// </summary>
public interface IEventHandler
{
    /// <summary>
    /// Gets the event type that this handler can process.
    /// </summary>
    Type EventType { get; }
    
    /// <summary>
    /// Handles the specified event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="context">The component context where the event occurred.</param>
    /// <returns>True if the event was handled and should not propagate further, false otherwise.</returns>
    bool Handle(object eventArgs, IComponentContext context);
}

/// <summary>
/// Generic interface for strongly-typed event handlers.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public interface IEventHandler<in TEvent> : IEventHandler
    where TEvent : ComponentEventArgs
{
    /// <summary>
    /// Handles the specified event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="context">The component context where the event occurred.</param>
    /// <returns>True if the event was handled and should not propagate further, false otherwise.</returns>
    bool Handle(TEvent eventArgs, IComponentContext context);
}

/// <summary>
/// Base class for all component event arguments.
/// </summary>
public abstract class ComponentEventArgs : EventArgs
{
    /// <summary>
    /// Gets the component that originated the event.
    /// </summary>
    public IComponent Source { get; }
    
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the event has been handled.
    /// </summary>
    public bool Handled { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the event should stop propagating.
    /// </summary>
    public bool StopPropagation { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the ComponentEventArgs class.
    /// </summary>
    /// <param name="source">The component that originated the event.</param>
    protected ComponentEventArgs(IComponent source)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Timestamp = DateTime.UtcNow;
    }
}

/// <summary>
/// Event arguments for component lifecycle events.
/// </summary>
public class ComponentLifecycleEventArgs : ComponentEventArgs
{
    /// <summary>
    /// Gets the lifecycle phase.
    /// </summary>
    public ComponentLifecyclePhase Phase { get; }
    
    /// <summary>
    /// Initializes a new instance of the ComponentLifecycleEventArgs class.
    /// </summary>
    /// <param name="source">The component that originated the event.</param>
    /// <param name="phase">The lifecycle phase.</param>
    public ComponentLifecycleEventArgs(IComponent source, ComponentLifecyclePhase phase)
        : base(source)
    {
        Phase = phase;
    }
}

/// <summary>
/// Event arguments for property change events.
/// </summary>
public class PropertyChangedEventArgs : ComponentEventArgs
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
    /// Initializes a new instance of the PropertyChangedEventArgs class.
    /// </summary>
    /// <param name="source">The component that originated the event.</param>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <param name="oldValue">The old value of the property.</param>
    /// <param name="newValue">The new value of the property.</param>
    public PropertyChangedEventArgs(IComponent source, string propertyName, object? oldValue, object? newValue)
        : base(source)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Event arguments for user interaction events.
/// </summary>
public class InteractionEventArgs : ComponentEventArgs
{
    /// <summary>
    /// Gets the type of interaction.
    /// </summary>
    public InteractionType InteractionType { get; }
    
    /// <summary>
    /// Gets additional data associated with the interaction.
    /// </summary>
    public object? Data { get; }
    
    /// <summary>
    /// Initializes a new instance of the InteractionEventArgs class.
    /// </summary>
    /// <param name="source">The component that originated the event.</param>
    /// <param name="interactionType">The type of interaction.</param>
    /// <param name="data">Additional data associated with the interaction.</param>
    public InteractionEventArgs(IComponent source, InteractionType interactionType, object? data = null)
        : base(source)
    {
        InteractionType = interactionType;
        Data = data;
    }
}

/// <summary>
/// Enumeration of component lifecycle phases.
/// </summary>
public enum ComponentLifecyclePhase
{
    /// <summary>
    /// Component is being initialized.
    /// </summary>
    Initialize,
    
    /// <summary>
    /// Component is being mounted.
    /// </summary>
    Mount,
    
    /// <summary>
    /// Component is being updated.
    /// </summary>
    Update,
    
    /// <summary>
    /// Component is being rendered.
    /// </summary>
    Render,
    
    /// <summary>
    /// Component has finished rendering.
    /// </summary>
    AfterRender,
    
    /// <summary>
    /// Component is being unmounted.
    /// </summary>
    Unmount,
    
    /// <summary>
    /// Component is being disposed.
    /// </summary>
    Dispose
}

/// <summary>
/// Enumeration of interaction types.
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Click interaction.
    /// </summary>
    Click,
    
    /// <summary>
    /// Double-click interaction.
    /// </summary>
    DoubleClick,
    
    /// <summary>
    /// Key press interaction.
    /// </summary>
    KeyPress,
    
    /// <summary>
    /// Key down interaction.
    /// </summary>
    KeyDown,
    
    /// <summary>
    /// Key up interaction.
    /// </summary>
    KeyUp,
    
    /// <summary>
    /// Mouse enter interaction.
    /// </summary>
    MouseEnter,
    
    /// <summary>
    /// Mouse leave interaction.
    /// </summary>
    MouseLeave,
    
    /// <summary>
    /// Mouse move interaction.
    /// </summary>
    MouseMove,
    
    /// <summary>
    /// Focus gained interaction.
    /// </summary>
    Focus,
    
    /// <summary>
    /// Focus lost interaction.
    /// </summary>
    Blur,
    
    /// <summary>
    /// Value changed interaction.
    /// </summary>
    ValueChanged,
    
    /// <summary>
    /// Custom interaction.
    /// </summary>
    Custom
}