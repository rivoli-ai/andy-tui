using System;
using System.Collections.Generic;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Declarative;

/// <summary>
/// Represents a runtime instance of a view component with state preservation.
/// Similar to SwiftUI's view instance backing storage.
/// </summary>
public abstract class ViewInstance
{
    private readonly string _id;
    private bool _needsUpdate = true;
    
    /// <summary>
    /// Gets the unique identifier for this view instance.
    /// </summary>
    public string Id => _id;
    
    /// <summary>
    /// Gets whether this view needs to be updated.
    /// </summary>
    public bool NeedsUpdate => _needsUpdate;
    
    /// <summary>
    /// Gets or sets the context for this view instance.
    /// </summary>
    public DeclarativeContext? Context { get; set; }
    
    protected ViewInstance(string id)
    {
        _id = id ?? throw new ArgumentNullException(nameof(id));
    }
    
    /// <summary>
    /// Marks this view as needing an update.
    /// </summary>
    public void InvalidateView()
    {
        _needsUpdate = true;
        Context?.RequestRender();
    }
    
    /// <summary>
    /// Updates this view instance from a view declaration.
    /// </summary>
    public void Update(ISimpleComponent viewDeclaration)
    {
        OnUpdate(viewDeclaration);
        _needsUpdate = false;
    }
    
    /// <summary>
    /// Renders this view instance to virtual DOM.
    /// </summary>
    public abstract VirtualNode Render();
    
    /// <summary>
    /// Called when the view needs to update from a declaration.
    /// </summary>
    protected abstract void OnUpdate(ISimpleComponent viewDeclaration);
    
    /// <summary>
    /// Called when this view instance is being disposed.
    /// </summary>
    public virtual void Dispose()
    {
    }
}

/// <summary>
/// Manages the lifecycle of view instances.
/// </summary>
public class ViewInstanceManager
{
    private readonly Dictionary<string, ViewInstance> _instances = new();
    private readonly DeclarativeContext _context;
    
    public ViewInstanceManager(DeclarativeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    /// <summary>
    /// Gets or creates a view instance for the given declaration.
    /// </summary>
    public ViewInstance GetOrCreateInstance(ISimpleComponent viewDeclaration, string path)
    {
        var key = $"{path}:{viewDeclaration.GetType().Name}";
        
        if (!_instances.TryGetValue(key, out var instance))
        {
            instance = CreateInstance(viewDeclaration, key);
            instance.Context = _context;
            _instances[key] = instance;
        }
        
        instance.Update(viewDeclaration);
        return instance;
    }
    
    /// <summary>
    /// Creates a new view instance for the given declaration.
    /// </summary>
    private ViewInstance CreateInstance(ISimpleComponent viewDeclaration, string id)
    {
        return viewDeclaration switch
        {
            TextField textField => new TextFieldInstance(id),
            Button button => new ButtonInstance(id),
            Text text => new TextInstance(id),
            VStack vstack => new VStackInstance(id),
            HStack hstack => new HStackInstance(id),
            _ => CreateGenericInstance(viewDeclaration, id)
        };
    }
    
    private ViewInstance CreateGenericInstance(ISimpleComponent viewDeclaration, string id)
    {
        var type = viewDeclaration.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dropdown<>))
        {
            var itemType = type.GetGenericArguments()[0];
            var instanceType = typeof(DropdownInstance<>).MakeGenericType(itemType);
            return (ViewInstance)Activator.CreateInstance(instanceType, id)!;
        }
        
        throw new NotSupportedException($"No instance type for {type}");
    }
    
    /// <summary>
    /// Clears all view instances.
    /// </summary>
    public void Clear()
    {
        foreach (var instance in _instances.Values)
        {
            instance.Dispose();
        }
        _instances.Clear();
    }
}