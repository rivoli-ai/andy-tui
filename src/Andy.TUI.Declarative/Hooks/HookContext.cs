using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Context for managing hook state within a component.
/// Each component instance has its own HookContext to track hook calls and state.
/// </summary>
public class HookContext
{
    private readonly List<IHook> _hooks = new();
    private int _currentHookIndex = 0;
    private bool _isInitialized = false;
    private readonly string _componentId;

    /// <summary>
    /// Gets the component ID this context belongs to.
    /// </summary>
    public string ComponentId => _componentId;

    /// <summary>
    /// Gets whether this context has been initialized (first render completed).
    /// </summary>
    public bool IsInitialized => _isInitialized;

    public HookContext(string componentId)
    {
        _componentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
    }

    /// <summary>
    /// Begins a render cycle, resetting the hook index.
    /// </summary>
    public void BeginRender()
    {
        _currentHookIndex = 0;
    }

    /// <summary>
    /// Completes a render cycle, marking the context as initialized.
    /// </summary>
    public void EndRender()
    {
        _isInitialized = true;
        RunPendingEffects();
    }

    /// <summary>
    /// List of effects to run after render completes.
    /// </summary>
    private readonly List<Action> _pendingEffects = new();

    /// <summary>
    /// Schedules an effect to run after the current render completes.
    /// </summary>
    public void ScheduleEffect(Action effect)
    {
        _pendingEffects.Add(effect);
    }

    /// <summary>
    /// Runs all pending effects after render completes.
    /// </summary>
    private void RunPendingEffects()
    {
        var effects = _pendingEffects.ToArray();
        _pendingEffects.Clear();
        foreach (var effect in effects)
        {
            effect();
        }
    }

    /// <summary>
    /// Gets or creates a hook at the current index position.
    /// </summary>
    /// <typeparam name="T">The type of hook to get or create.</typeparam>
    /// <param name="factory">Factory function to create the hook if it doesn't exist.</param>
    /// <returns>The hook instance.</returns>
    public T UseHook<T>(Func<T> factory) where T : IHook
    {
        if (_currentHookIndex >= _hooks.Count)
        {
            // First render - create the hook
            if (_isInitialized)
            {
                throw new InvalidOperationException(
                    $"Hook count mismatch in component {_componentId}. " +
                    "Hooks must be called in the same order on every render. " +
                    "Ensure hooks are not called conditionally.");
            }

            var hook = factory();
            _hooks.Add(hook);
            _currentHookIndex++;
            return hook;
        }
        else
        {
            // Subsequent renders - retrieve existing hook
            var existingHook = _hooks[_currentHookIndex];
            if (existingHook is not T typedHook)
            {
                throw new InvalidOperationException(
                    $"Hook type mismatch in component {_componentId} at index {_currentHookIndex}. " +
                    $"Expected {typeof(T).Name} but found {existingHook.GetType().Name}. " +
                    "Hooks must be called in the same order with the same types on every render.");
            }

            _currentHookIndex++;
            return typedHook;
        }
    }

    /// <summary>
    /// Validates that all hooks were called in this render cycle.
    /// </summary>
    public void ValidateHookOrder()
    {
        if (_isInitialized && _currentHookIndex != _hooks.Count)
        {
            throw new InvalidOperationException(
                $"Hook count mismatch in component {_componentId}. " +
                $"Expected {_hooks.Count} hooks but {_currentHookIndex} were called. " +
                "Hooks must be called in the same order on every render.");
        }
    }

    /// <summary>
    /// Disposes all hooks and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        foreach (var hook in _hooks)
        {
            hook.Dispose();
        }
        _hooks.Clear();
    }

    /// <summary>
    /// Triggers an update for the component, causing a re-render.
    /// </summary>
    public Action<Action>? ScheduleUpdate { get; set; }

    /// <summary>
    /// Requests a re-render of the component.
    /// </summary>
    public void RequestUpdate()
    {
        ScheduleUpdate?.Invoke(() => { });
    }
}

/// <summary>
/// Base interface for all hooks.
/// </summary>
public interface IHook : IDisposable
{
    // IDisposable.Dispose is inherited
}