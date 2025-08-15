using System;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for memoizing callbacks to prevent unnecessary re-renders.
/// </summary>
/// <typeparam name="T">The delegate type of the callback.</typeparam>
public class UseCallbackHook<T> : UseMemoHook<T> where T : Delegate
{
    // UseCallback is essentially UseMemo for functions
    // It inherits all the memoization logic from UseMemo
}