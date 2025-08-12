using System;
using System.Collections.Generic;
using Andy.TUI.Declarative.Hooks;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.Terminal;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Base class for components that support React-style hooks.
/// </summary>
public abstract class HookableComponent : ISimpleComponent
{
    private HookContext? _hookContext;
    private bool _isRendering = false;
    
    /// <summary>
    /// Gets the unique ID for this component instance.
    /// </summary>
    protected virtual string ComponentId => GetType().Name + "_" + GetHashCode();

    /// <summary>
    /// Gets the hook context for this component.
    /// </summary>
    protected HookContext HookContext
    {
        get
        {
            if (_hookContext == null)
            {
                _hookContext = new HookContext(ComponentId);
                _hookContext.ScheduleUpdate = action =>
                {
                    // Trigger a re-render when hooks request an update
                    OnStateChanged?.Invoke();
                };
            }
            return _hookContext;
        }
    }

    /// <summary>
    /// Event raised when the component's state changes and needs re-rendering.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Creates the component tree. Override this to define your component structure.
    /// </summary>
    protected abstract ISimpleComponent Body();

    /// <summary>
    /// Renders the component, managing the hook lifecycle.
    /// </summary>
    public VirtualNode Render()
    {
        if (_isRendering)
        {
            throw new InvalidOperationException(
                $"Component {ComponentId} is already rendering. " +
                "This usually indicates a render loop or incorrect hook usage.");
        }

        try
        {
            _isRendering = true;
            HookContext.BeginRender();

            var result = Body();

            HookContext.ValidateHookOrder();
            HookContext.EndRender();

            // Convert ISimpleComponent to VirtualNode
            if (result is ISimpleComponent component)
            {
                return component.Render();
            }

            // If Body returns null or something else, return empty fragment
            return VirtualDomBuilder.Fragment();
        }
        finally
        {
            _isRendering = false;
        }
    }

    #region Hook Methods

    /// <summary>
    /// Creates or retrieves state for this component.
    /// </summary>
    /// <typeparam name="T">The type of state value.</typeparam>
    /// <param name="initialValue">The initial state value.</param>
    /// <returns>A state accessor providing the current value and setter.</returns>
    protected StateAccessor<T> UseState<T>(T initialValue)
    {
        var hook = HookContext.UseHook(() => new UseStateHook<T>(HookContext, initialValue));
        return new StateAccessor<T>(hook);
    }

    /// <summary>
    /// Performs a side effect after rendering.
    /// </summary>
    /// <param name="effect">The effect to perform.</param>
    /// <param name="dependencies">Optional dependencies that trigger re-execution.</param>
    protected void UseEffect(Action effect, object[]? dependencies = null)
    {
        var hook = HookContext.UseHook(() => 
        {
            var h = new UseEffectHook();
            h.SetContext(HookContext);
            return h;
        });
        hook.SetEffect(effect, dependencies);
    }

    /// <summary>
    /// Performs a side effect that returns a cleanup function.
    /// </summary>
    /// <param name="effect">The effect that returns a cleanup function.</param>
    /// <param name="dependencies">Optional dependencies that trigger re-execution.</param>
    protected void UseEffect(Func<Action?> effect, object[]? dependencies = null)
    {
        var hook = HookContext.UseHook(() => 
        {
            var h = new UseEffectHook();
            h.SetContext(HookContext);
            return h;
        });
        hook.SetEffect(effect, dependencies);
    }

    /// <summary>
    /// Memoizes an expensive computation.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    /// <param name="factory">Function to compute the value.</param>
    /// <param name="dependencies">Dependencies that trigger recomputation.</param>
    /// <returns>The memoized value.</returns>
    protected T UseMemo<T>(Func<T> factory, object[]? dependencies)
    {
        var hook = HookContext.UseHook(() => new UseMemoHook<T>());
        return hook.GetValue(factory, dependencies);
    }

    /// <summary>
    /// Memoizes a callback function.
    /// </summary>
    /// <typeparam name="T">The delegate type of the callback.</typeparam>
    /// <param name="callback">The callback to memoize.</param>
    /// <param name="dependencies">Dependencies that trigger recreation.</param>
    /// <returns>The memoized callback.</returns>
    protected T UseCallback<T>(T callback, object[]? dependencies) where T : Delegate
    {
        var hook = HookContext.UseHook(() => new UseCallbackHook<T>());
        return hook.GetValue(() => callback, dependencies);
    }

    /// <summary>
    /// Creates a mutable reference that persists across renders.
    /// </summary>
    /// <typeparam name="T">The type of the referenced value.</typeparam>
    /// <param name="initialValue">The initial value.</param>
    /// <returns>A reference object.</returns>
    protected RefObject<T> UseRef<T>(T? initialValue = default)
    {
        var hook = HookContext.UseHook(() => new UseRefHook<T>(initialValue));
        return new RefObject<T>(hook);
    }

    /// <summary>
    /// Sets up keyboard input handling for this component.
    /// </summary>
    /// <param name="handler">The handler for keyboard events.</param>
    /// <param name="inputHandler">Optional input handler to subscribe to.</param>
    protected void UseInput(Action<ConsoleKeyInfo> handler, IInputHandler? inputHandler = null)
    {
        var hook = HookContext.UseHook(() => new UseInputHook());
        hook.SetHandler(inputHandler ?? GetInputHandler(), handler);
    }

    /// <summary>
    /// Sets up focus management for this component.
    /// </summary>
    /// <param name="onFocus">Callback when component gains focus.</param>
    /// <param name="onBlur">Callback when component loses focus.</param>
    /// <param name="canFocus">Whether the component can receive focus.</param>
    /// <returns>A focus state object.</returns>
    protected FocusState UseFocus(Action? onFocus = null, Action? onBlur = null, bool canFocus = true)
    {
        var hook = HookContext.UseHook(() => new UseFocusHook());
        hook.SetupFocus(GetFocusManager(), onFocus, onBlur, canFocus);
        return new FocusState(hook);
    }

    /// <summary>
    /// Override to provide the input handler for UseInput hook.
    /// </summary>
    protected virtual IInputHandler? GetInputHandler() => null;

    /// <summary>
    /// Override to provide the focus manager for UseFocus hook.
    /// </summary>
    protected virtual FocusManager? GetFocusManager() => null;

    #endregion

    #region ISimpleComponent Implementation

    // HookableComponent implements ISimpleComponent.Render() directly

    #endregion

    /// <summary>
    /// Disposes the component and its hook context.
    /// </summary>
    public void Dispose()
    {
        _hookContext?.Dispose();
        _hookContext = null;
    }
}