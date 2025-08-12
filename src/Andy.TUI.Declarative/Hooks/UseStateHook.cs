using System;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for managing state within a component.
/// </summary>
/// <typeparam name="T">The type of state value.</typeparam>
public class UseStateHook<T> : IHook
{
    private T _value;
    private readonly HookContext _context;

    /// <summary>
    /// Gets the current state value.
    /// </summary>
    public T Value => _value;

    public UseStateHook(HookContext context, T initialValue)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _value = initialValue;
    }

    /// <summary>
    /// Sets a new state value and triggers a re-render.
    /// </summary>
    public void SetValue(T newValue)
    {
        if (!Equals(_value, newValue))
        {
            _value = newValue;
            _context.RequestUpdate();
        }
    }

    /// <summary>
    /// Sets a new state value using a function that receives the current value.
    /// </summary>
    public void SetValue(Func<T, T> updater)
    {
        if (updater == null)
            throw new ArgumentNullException(nameof(updater));

        SetValue(updater(_value));
    }

    public void Dispose()
    {
        // State hooks don't need cleanup
    }
}

/// <summary>
/// Helper class to provide a tuple-like API for useState.
/// </summary>
public class StateAccessor<T>
{
    private readonly UseStateHook<T> _hook;

    public StateAccessor(UseStateHook<T> hook)
    {
        _hook = hook ?? throw new ArgumentNullException(nameof(hook));
    }

    public T Value => _hook.Value;
    public Action<T> SetValue => _hook.SetValue;
    public Action<Func<T, T>> UpdateValue => _hook.SetValue;

    public void Deconstruct(out T value, out Action<T> setValue)
    {
        value = Value;
        setValue = SetValue;
    }

    public void Deconstruct(out T value, out Action<T> setValue, out Action<Func<T, T>> updateValue)
    {
        value = Value;
        setValue = SetValue;
        updateValue = UpdateValue;
    }

    public static implicit operator T(StateAccessor<T> accessor) => accessor.Value;
}