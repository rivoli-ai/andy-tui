using System;
using System.Linq;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for managing side effects within a component.
/// </summary>
public class UseEffectHook : IHook
{
    private Action? _cleanup;
    private object[]? _dependencies;
    private bool _hasRun = false;
    private HookContext? _context;

    /// <summary>
    /// Sets the hook context for scheduling effects.
    /// </summary>
    public void SetContext(HookContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Sets up an effect with optional dependencies.
    /// </summary>
    /// <param name="effect">The effect to run.</param>
    /// <param name="dependencies">Optional dependencies that trigger re-execution when changed.</param>
    public void SetEffect(Action effect, object[]? dependencies = null)
    {
        SetEffect(() => { effect(); return null; }, dependencies);
    }

    /// <summary>
    /// Sets up an effect that returns a cleanup function with optional dependencies.
    /// </summary>
    /// <param name="effect">The effect to run that optionally returns a cleanup function.</param>
    /// <param name="dependencies">Optional dependencies that trigger re-execution when changed.</param>
    public void SetEffect(Func<Action?> effect, object[]? dependencies = null)
    {
        if (effect == null)
            throw new ArgumentNullException(nameof(effect));

        bool shouldRun = false;

        if (!_hasRun)
        {
            // First run - always execute
            shouldRun = true;
        }
        else if (dependencies == null)
        {
            // No dependencies - run on every render
            shouldRun = true;
        }
        else if (_dependencies == null)
        {
            // Had no dependencies before, now has - run
            shouldRun = true;
        }
        else if (_dependencies.Length != dependencies.Length)
        {
            // Different number of dependencies - run
            shouldRun = true;
        }
        else
        {
            // Check if any dependencies changed
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (!Equals(_dependencies[i], dependencies[i]))
                {
                    shouldRun = true;
                    break;
                }
            }
        }

        if (shouldRun)
        {
            // Store new dependencies
            _dependencies = dependencies?.ToArray(); // Create a copy
            _hasRun = true;

            // Schedule the effect to run after render
            _context?.ScheduleEffect(() =>
            {
                // Run cleanup from previous effect
                _cleanup?.Invoke();
                _cleanup = null;

                // Run the new effect and store cleanup
                _cleanup = effect();
            });
        }
    }

    public void Dispose()
    {
        // Run cleanup when component is disposed
        _cleanup?.Invoke();
        _cleanup = null;
        _dependencies = null;
    }
}