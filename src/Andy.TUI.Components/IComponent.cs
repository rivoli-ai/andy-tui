using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Components;

/// <summary>
/// Defines the contract for all UI components in the Andy.TUI framework.
/// Components are reusable UI elements that manage their own state and lifecycle.
/// </summary>
public interface IComponent : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this component instance.
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Gets a value indicating whether this component has been initialized.
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// Gets a value indicating whether this component is currently mounted in the component tree.
    /// </summary>
    bool IsMounted { get; }
    
    /// <summary>
    /// Gets the parent component context, if this component has a parent.
    /// </summary>
    IComponentContext? Parent { get; }
    
    /// <summary>
    /// Gets the component context for this component.
    /// </summary>
    IComponentContext Context { get; }
    
    /// <summary>
    /// Initializes the component. This is called once when the component is first created.
    /// </summary>
    /// <param name="context">The component context providing access to services and parent/child relationships.</param>
    void Initialize(IComponentContext context);
    
    /// <summary>
    /// Called when the component is mounted in the component tree.
    /// This happens after initialization and when the component becomes part of the active UI.
    /// </summary>
    void OnMount();
    
    /// <summary>
    /// Called when the component is unmounted from the component tree.
    /// This happens when the component is removed from the active UI.
    /// </summary>
    void OnUnmount();
    
    /// <summary>
    /// Renders the component's virtual DOM representation.
    /// This method is called whenever the component needs to be re-rendered.
    /// </summary>
    /// <returns>The virtual DOM node representing this component's UI.</returns>
    VirtualNode Render();
    
    /// <summary>
    /// Called when the component's state or properties have changed and it needs to update.
    /// This is called before Render() to allow the component to perform any necessary updates.
    /// </summary>
    void Update();
    
    /// <summary>
    /// Called before the component is re-rendered to determine if a re-render is necessary.
    /// </summary>
    /// <returns>True if the component should be re-rendered, false otherwise.</returns>
    bool ShouldUpdate();
    
    /// <summary>
    /// Called after the component has been rendered and the virtual DOM has been updated.
    /// </summary>
    void OnAfterRender();
    
    /// <summary>
    /// Requests that this component be re-rendered on the next render cycle.
    /// </summary>
    void RequestRender();
    
    /// <summary>
    /// Occurs when the component requests to be re-rendered.
    /// </summary>
    event EventHandler? RenderRequested;
    
    /// <summary>
    /// Occurs when the component's state changes.
    /// </summary>
    event EventHandler? StateChanged;
}