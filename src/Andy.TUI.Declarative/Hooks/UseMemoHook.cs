using System;
using System.Linq;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for memoizing expensive computations.
/// </summary>
/// <typeparam name="T">The type of the memoized value.</typeparam>
public class UseMemoHook<T> : IHook
{
    private T? _cachedValue;
    private object[]? _dependencies;
    private bool _hasComputed = false;

    /// <summary>
    /// Gets the memoized value, recomputing only when dependencies change.
    /// </summary>
    /// <param name="factory">Function to compute the value.</param>
    /// <param name="dependencies">Dependencies that trigger recomputation when changed.</param>
    /// <returns>The memoized value.</returns>
    public T GetValue(Func<T> factory, object[]? dependencies)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        bool shouldRecompute = false;

        if (!_hasComputed)
        {
            // First computation
            shouldRecompute = true;
        }
        else if (dependencies == null && _dependencies == null)
        {
            // No dependencies - never recompute after first time
            shouldRecompute = false;
        }
        else if (dependencies == null || _dependencies == null)
        {
            // One has dependencies, other doesn't - recompute
            shouldRecompute = true;
        }
        else if (_dependencies.Length != dependencies.Length)
        {
            // Different number of dependencies - recompute
            shouldRecompute = true;
        }
        else
        {
            // Check if any dependencies changed
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (!Equals(_dependencies[i], dependencies[i]))
                {
                    shouldRecompute = true;
                    break;
                }
            }
        }

        if (shouldRecompute)
        {
            _cachedValue = factory();
            _dependencies = dependencies?.ToArray(); // Create a copy
            _hasComputed = true;
        }

        return _cachedValue!;
    }

    public void Dispose()
    {
        // Clear cached value if it's disposable
        if (_cachedValue is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _cachedValue = default;
        _dependencies = null;
    }
}