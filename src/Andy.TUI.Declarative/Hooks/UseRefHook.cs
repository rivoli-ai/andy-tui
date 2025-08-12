using System;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for maintaining a mutable reference that persists across renders.
/// </summary>
/// <typeparam name="T">The type of the referenced value.</typeparam>
public class UseRefHook<T> : IHook
{
    private T? _current;

    /// <summary>
    /// Gets or sets the current value of the reference.
    /// </summary>
    public T? Current
    {
        get => _current;
        set => _current = value;
    }

    public UseRefHook(T? initialValue = default)
    {
        _current = initialValue;
    }

    public void Dispose()
    {
        // Clear reference if it's disposable
        if (_current is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _current = default;
    }
}

/// <summary>
/// Reference wrapper for UseRef hook.
/// </summary>
public class RefObject<T>
{
    private readonly UseRefHook<T> _hook;

    public RefObject(UseRefHook<T> hook)
    {
        _hook = hook ?? throw new ArgumentNullException(nameof(hook));
    }

    /// <summary>
    /// Gets or sets the current value of the reference.
    /// </summary>
    public T? Current
    {
        get => _hook.Current;
        set => _hook.Current = value;
    }
}