using System.Collections.Concurrent;

namespace Andy.TUI.Components.EventHandling;

/// <summary>
/// Manages event handling and propagation for components.
/// </summary>
public class EventManager
{
    private readonly ConcurrentDictionary<Type, List<IEventHandler>> _handlers = new();
    private readonly ConcurrentDictionary<string, List<EventSubscription>> _subscriptions = new();
    
    /// <summary>
    /// Registers an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to handle.</typeparam>
    /// <param name="handler">The event handler.</param>
    public void RegisterHandler<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : ComponentEventArgs
    {
        var eventType = typeof(TEvent);
        var handlers = _handlers.GetOrAdd(eventType, _ => new List<IEventHandler>());
        
        lock (handlers)
        {
            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }
    }
    
    /// <summary>
    /// Unregisters an event handler for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to handle.</typeparam>
    /// <param name="handler">The event handler.</param>
    public void UnregisterHandler<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : ComponentEventArgs
    {
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            lock (handlers)
            {
                handlers.Remove(handler);
            }
        }
    }
    
    /// <summary>
    /// Subscribes to events from a specific component.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
    /// <param name="componentId">The ID of the component to subscribe to.</param>
    /// <param name="callback">The callback to execute when the event occurs.</param>
    /// <returns>A subscription that can be disposed to unsubscribe.</returns>
    public IDisposable Subscribe<TEvent>(string componentId, Action<TEvent> callback)
        where TEvent : ComponentEventArgs
    {
        var subscription = new EventSubscription<TEvent>(componentId, callback);
        var subscriptions = _subscriptions.GetOrAdd(componentId, _ => new List<EventSubscription>());
        
        lock (subscriptions)
        {
            subscriptions.Add(subscription);
        }
        
        return subscription;
    }
    
    /// <summary>
    /// Publishes an event to all registered handlers and subscribers.
    /// </summary>
    /// <typeparam name="TEvent">The type of event to publish.</typeparam>
    /// <param name="eventArgs">The event arguments.</param>
    /// <param name="context">The component context where the event occurred.</param>
    public void PublishEvent<TEvent>(TEvent eventArgs, IComponentContext context)
        where TEvent : ComponentEventArgs
    {
        if (eventArgs == null || context == null)
            return;
        
        // Handle registered handlers first
        var eventType = typeof(TEvent);
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            List<IEventHandler> handlersCopy;
            lock (handlers)
            {
                handlersCopy = new List<IEventHandler>(handlers);
            }
            
            foreach (var handler in handlersCopy)
            {
                if (eventArgs.StopPropagation)
                    break;
                
                try
                {
                    var handled = handler.Handle(eventArgs, context);
                    if (handled)
                    {
                        eventArgs.Handled = true;
                        if (eventArgs.StopPropagation)
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception but continue processing other handlers
                    Console.WriteLine($"Error in event handler: {ex.Message}");
                }
            }
        }
        
        // Handle component-specific subscriptions
        var componentId = eventArgs.Source.Id;
        if (_subscriptions.TryGetValue(componentId, out var subscriptions))
        {
            List<EventSubscription> subscriptionsCopy;
            lock (subscriptions)
            {
                subscriptionsCopy = new List<EventSubscription>(subscriptions.Where(s => !s.IsDisposed));
                // Remove disposed subscriptions
                subscriptions.RemoveAll(s => s.IsDisposed);
            }
            
            foreach (var subscription in subscriptionsCopy)
            {
                if (eventArgs.StopPropagation)
                    break;
                
                try
                {
                    subscription.Handle(eventArgs);
                }
                catch (Exception ex)
                {
                    // Log the exception but continue processing other subscriptions
                    Console.WriteLine($"Error in event subscription: {ex.Message}");
                }
            }
        }
        
        // Propagate to parent if not handled and propagation is not stopped
        if (!eventArgs.Handled && !eventArgs.StopPropagation && context.Parent != null)
        {
            PublishEvent(eventArgs, context.Parent);
        }
    }
    
    /// <summary>
    /// Clears all handlers and subscriptions for a specific component.
    /// </summary>
    /// <param name="componentId">The ID of the component to clear.</param>
    public void ClearComponent(string componentId)
    {
        if (_subscriptions.TryRemove(componentId, out var subscriptions))
        {
            lock (subscriptions)
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.Dispose();
                }
            }
        }
    }
    
    /// <summary>
    /// Clears all handlers and subscriptions.
    /// </summary>
    public void Clear()
    {
        _handlers.Clear();
        
        foreach (var subscriptions in _subscriptions.Values)
        {
            lock (subscriptions)
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.Dispose();
                }
            }
        }
        _subscriptions.Clear();
    }
}

/// <summary>
/// Base class for event subscriptions.
/// </summary>
public abstract class EventSubscription : IDisposable
{
    /// <summary>
    /// Gets the component ID this subscription is for.
    /// </summary>
    public string ComponentId { get; }
    
    /// <summary>
    /// Gets a value indicating whether this subscription has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }
    
    /// <summary>
    /// Initializes a new instance of the EventSubscription class.
    /// </summary>
    /// <param name="componentId">The component ID this subscription is for.</param>
    protected EventSubscription(string componentId)
    {
        ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
    }
    
    /// <summary>
    /// Handles the specified event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    public abstract void Handle(ComponentEventArgs eventArgs);
    
    /// <summary>
    /// Disposes the subscription.
    /// </summary>
    public virtual void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Generic event subscription for strongly-typed events.
/// </summary>
/// <typeparam name="TEvent">The type of event.</typeparam>
public class EventSubscription<TEvent> : EventSubscription
    where TEvent : ComponentEventArgs
{
    private readonly Action<TEvent> _callback;
    
    /// <summary>
    /// Initializes a new instance of the EventSubscription class.
    /// </summary>
    /// <param name="componentId">The component ID this subscription is for.</param>
    /// <param name="callback">The callback to execute when the event occurs.</param>
    public EventSubscription(string componentId, Action<TEvent> callback)
        : base(componentId)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }
    
    /// <summary>
    /// Handles the specified event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    public override void Handle(ComponentEventArgs eventArgs)
    {
        if (IsDisposed || eventArgs is not TEvent typedEventArgs)
            return;
        
        _callback(typedEventArgs);
    }
}