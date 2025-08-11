using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.VirtualDom;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.ViewInstances;

namespace Andy.TUI.Declarative;

/// <summary>
/// Represents a runtime instance of a view component with state preservation.
/// Similar to SwiftUI's view instance backing storage.
/// </summary>
public abstract class ViewInstance
{
    private readonly string _id;
    private bool _needsUpdate = true;
    private bool _needsLayout = true;
    private LayoutBox _layout = new();
    
    /// <summary>
    /// Gets the unique identifier for this view instance.
    /// </summary>
    public string Id => _id;
    
    /// <summary>
    /// Gets whether this view needs to be updated.
    /// </summary>
    public bool NeedsUpdate => _needsUpdate;
    
    /// <summary>
    /// Gets whether this view needs layout recalculation.
    /// </summary>
    public bool NeedsLayout => _needsLayout;
    
    private DeclarativeContext? _context;
    
    /// <summary>
    /// Gets or sets the context for this view instance.
    /// </summary>
    public DeclarativeContext? Context 
    { 
        get => _context;
        set 
        {
            if (_context != value)
            {
                // Unregister from old context
                if (_context != null && this is IFocusable focusable)
                {
                    _context.FocusManager.UnregisterFocusable(focusable);
                }
                
                _context = value;
                
                // Register with new context
                if (_context != null && this is IFocusable newFocusable)
                {
                    _context.FocusManager.RegisterFocusable(newFocusable);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the calculated layout box for this view.
    /// </summary>
    public LayoutBox Layout => _layout;
    
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
    
    #region Z-Index Support
    
    private int _relativeZIndex = 0;
    private int _absoluteZIndex = 0;
    
    /// <summary>
    /// Gets or sets the z-index relative to the parent component.
    /// </summary>
    public virtual int RelativeZIndex
    {
        get => _relativeZIndex;
        set => _relativeZIndex = value;
    }
    
    /// <summary>
    /// Gets the computed absolute z-index after hierarchical resolution.
    /// </summary>
    public virtual int AbsoluteZIndex => _absoluteZIndex;
    
    /// <summary>
    /// Updates the absolute z-index based on the parent's context.
    /// </summary>
    /// <param name="parentAbsoluteZ">The parent's absolute z-index.</param>
    public virtual void UpdateAbsoluteZIndex(int parentAbsoluteZ)
    {
        _absoluteZIndex = parentAbsoluteZ + _relativeZIndex;
        
        // Propagate to children if any
        foreach (var child in GetChildren())
        {
            child.UpdateAbsoluteZIndex(_absoluteZIndex);
        }
    }
    
    /// <summary>
    /// Gets the child instances for z-index propagation.
    /// Override in container components.
    /// </summary>
    protected virtual IEnumerable<ViewInstance> GetChildren()
    {
        return Enumerable.Empty<ViewInstance>();
    }
    
    #endregion
    
    /// <summary>
    /// Marks this view as needing layout recalculation.
    /// </summary>
    public void InvalidateLayout()
    {
        _needsLayout = true;
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
    /// Calculates the layout for this view given constraints.
    /// </summary>
    public void CalculateLayout(LayoutConstraints constraints)
    {
        _layout = PerformLayout(constraints);
        _needsLayout = false;
    }
    
    /// <summary>
    /// Performs layout calculation for this view.
    /// Override to implement custom layout logic.
    /// </summary>
    protected virtual LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Default implementation - just fill available space
        return new LayoutBox
        {
            Width = constraints.MaxWidth,
            Height = constraints.MaxHeight
        };
    }
    
    /// <summary>
    /// Renders this view instance to virtual DOM using the calculated layout.
    /// </summary>
    public VirtualNode Render()
    {
        return RenderWithLayout(_layout);
    }
    
    /// <summary>
    /// Renders this view instance with the given layout information.
    /// </summary>
    protected abstract VirtualNode RenderWithLayout(LayoutBox layout);
    
    /// <summary>
    /// Called when the view needs to update from a declaration.
    /// </summary>
    protected abstract void OnUpdate(ISimpleComponent viewDeclaration);
    
    /// <summary>
    /// Called when this view instance is being disposed.
    /// </summary>
    public virtual void Dispose()
    {
        // Unregister from focus manager if focusable
        if (_context != null && this is IFocusable focusable)
        {
            _context.FocusManager.UnregisterFocusable(focusable);
        }
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
    /// Clears the instance cache to force recreation of all components.
    /// </summary>
    public void ClearCache()
    {
        // Dispose all cached instances
        foreach (var instance in _instances.Values)
        {
            instance.Dispose();
        }
        _instances.Clear();
    }
    
    /// <summary>
    /// Creates a new view instance for the given declaration.
    /// </summary>
    private ViewInstance CreateInstance(ISimpleComponent viewDeclaration, string id)
    {
        return viewDeclaration switch
        {
            Box box => new BoxInstance(id),
            TextField textField => new TextFieldInstance(id),
            TextArea textArea => new TextAreaInstance(id),
            Button button => new ButtonInstance(id),
            Text text => new TextInstance(id),
            VStack vstack => new VStackInstance(id),
            HStack hstack => new HStackInstance(id),
            ZStack zstack => new ZStackInstance(id),
            Grid grid => new GridInstance(id),
            Spacer spacer => new SpacerInstance(spacer, id),
            Modal modal => new ModalInstance(id, this),
            Newline newline => new NewlineInstance(id),
            Transform transform => new TransformInstance(id),
            Checkbox checkbox => new CheckboxInstance(id),
            List list => new ListInstance(id),
            ProgressBar progressBar => new ProgressBarInstance(id),
            Spinner spinner => new SpinnerInstance(id),
            Gradient gradient => new GradientInstance(id),
            BigText bigText => new BigTextInstance(id),
            Slider slider => new SliderInstance(id),
            Badge badge => new BadgeInstance(id),
            TabView tabView => new TabViewInstance(tabView),
            _ => CreateGenericInstance(viewDeclaration, id)
        };
    }
    
    private ViewInstance CreateGenericInstance(ISimpleComponent viewDeclaration, string id)
    {
        var type = viewDeclaration.GetType();
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            var itemType = type.GetGenericArguments()[0];
            
            if (genericTypeDef == typeof(Dropdown<>))
            {
                var instanceType = typeof(DropdownInstance<>).MakeGenericType(itemType);
                return (ViewInstance)Activator.CreateInstance(instanceType, id)!;
            }
            else if (genericTypeDef == typeof(SelectInput<>))
            {
                var instanceType = typeof(SelectInputInstance<>).MakeGenericType(itemType);
                return (ViewInstance)Activator.CreateInstance(instanceType, id)!;
            }
            else if (genericTypeDef == typeof(Table<>))
            {
                var instanceType = typeof(TableInstance<>).MakeGenericType(itemType);
                return (ViewInstance)Activator.CreateInstance(instanceType, id)!;
            }
            else if (genericTypeDef == typeof(MultiSelectInput<>))
            {
                var instanceType = typeof(MultiSelectInputInstance<>).MakeGenericType(itemType);
                return (ViewInstance)Activator.CreateInstance(instanceType, id)!;
            }
            else if (genericTypeDef == typeof(RadioGroup<>))
            {
                var instanceType = typeof(RadioGroupInstance<>).MakeGenericType(itemType);
                return (ViewInstance)Activator.CreateInstance(instanceType, id)!;
            }
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