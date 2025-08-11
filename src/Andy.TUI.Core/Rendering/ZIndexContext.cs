using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Core.Rendering;

/// <summary>
/// Manages hierarchical z-index resolution for nested UI components.
/// Tracks the cumulative z-index as we traverse the component tree,
/// converting relative z-indices to absolute values for rendering.
/// </summary>
public class ZIndexContext
{
    private readonly Stack<int> _zIndexStack = new();
    private readonly Stack<string> _componentStack = new(); // For debugging

    /// <summary>
    /// Gets the current absolute z-index based on the accumulated stack.
    /// </summary>
    public int CurrentAbsoluteZ => _zIndexStack.Count > 0 ? _zIndexStack.Sum() : 0;

    /// <summary>
    /// Gets the current nesting depth.
    /// </summary>
    public int Depth => _zIndexStack.Count;

    /// <summary>
    /// Enters a new component context with the given relative z-index.
    /// </summary>
    /// <param name="relativeZIndex">The component's z-index relative to its parent.</param>
    /// <param name="componentName">Optional component name for debugging.</param>
    public void EnterComponent(int relativeZIndex, string? componentName = null)
    {
        _zIndexStack.Push(relativeZIndex);
        _componentStack.Push(componentName ?? $"Component{_componentStack.Count}");
    }

    /// <summary>
    /// Exits the current component context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no context to exit.</exception>
    public void ExitComponent()
    {
        if (_zIndexStack.Count == 0)
            throw new InvalidOperationException("No component context to exit.");
            
        _zIndexStack.Pop();
        _componentStack.Pop();
    }

    /// <summary>
    /// Resolves a relative z-index to its absolute value in the current context.
    /// </summary>
    /// <param name="relativeZIndex">The relative z-index to resolve.</param>
    /// <returns>The absolute z-index value.</returns>
    public int ResolveAbsolute(int relativeZIndex)
    {
        return CurrentAbsoluteZ + relativeZIndex;
    }

    /// <summary>
    /// Gets the component hierarchy for debugging purposes.
    /// </summary>
    /// <returns>A string representation of the component stack.</returns>
    public string GetComponentPath()
    {
        return string.Join(" > ", _componentStack.Reverse());
    }

    /// <summary>
    /// Resets the context to its initial state.
    /// </summary>
    public void Reset()
    {
        _zIndexStack.Clear();
        _componentStack.Clear();
    }

    /// <summary>
    /// Creates a snapshot of the current context state.
    /// </summary>
    /// <returns>A snapshot that can be restored later.</returns>
    public ZIndexContextSnapshot CreateSnapshot()
    {
        return new ZIndexContextSnapshot(
            _zIndexStack.ToArray(), 
            _componentStack.ToArray()
        );
    }

    /// <summary>
    /// Restores the context from a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    public void RestoreSnapshot(ZIndexContextSnapshot snapshot)
    {
        _zIndexStack.Clear();
        _componentStack.Clear();
        
        // Restore in reverse order since we're pushing onto stacks
        for (int i = snapshot.ZIndexStack.Length - 1; i >= 0; i--)
        {
            _zIndexStack.Push(snapshot.ZIndexStack[i]);
            _componentStack.Push(snapshot.ComponentStack[i]);
        }
    }
}

/// <summary>
/// Represents a snapshot of a ZIndexContext state.
/// </summary>
public class ZIndexContextSnapshot
{
    public int[] ZIndexStack { get; }
    public string[] ComponentStack { get; }

    public ZIndexContextSnapshot(int[] zIndexStack, string[] componentStack)
    {
        ZIndexStack = zIndexStack;
        ComponentStack = componentStack;
    }
}