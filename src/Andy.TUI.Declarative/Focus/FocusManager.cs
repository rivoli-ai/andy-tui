using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Declarative.Focus;

/// <summary>
/// Manages keyboard focus for focusable components.
/// </summary>
public class FocusManager
{
    private readonly List<IFocusable> _focusableComponents = new();
    private IFocusable? _focusedComponent;
    
    /// <summary>
    /// Gets the currently focused component.
    /// </summary>
    public IFocusable? FocusedComponent => _focusedComponent;
    
    /// <summary>
    /// Registers a focusable component.
    /// </summary>
    public void RegisterFocusable(IFocusable component)
    {
        if (!_focusableComponents.Contains(component))
        {
            _focusableComponents.Add(component);
        }
    }
    
    /// <summary>
    /// Unregisters a focusable component.
    /// </summary>
    public void UnregisterFocusable(IFocusable component)
    {
        _focusableComponents.Remove(component);
        
        if (_focusedComponent == component)
        {
            // Move focus to next available component
            var next = GetNextFocusable(FocusDirection.Next);
            SetFocus(next);
        }
    }
    
    /// <summary>
    /// Sets focus to a specific component.
    /// </summary>
    public void SetFocus(IFocusable? component)
    {
        if (component == _focusedComponent)
            return;
            
        if (component != null && !component.CanFocus)
            return;
        
        _focusedComponent?.OnLostFocus();
        _focusedComponent = component;
        _focusedComponent?.OnGotFocus();
    }
    
    /// <summary>
    /// Moves focus in the specified direction.
    /// </summary>
    public void MoveFocus(FocusDirection direction)
    {
        var next = GetNextFocusable(direction);
        if (next != null)
        {
            SetFocus(next);
        }
    }
    
    /// <summary>
    /// Gets the next focusable component in the specified direction.
    /// </summary>
    private IFocusable? GetNextFocusable(FocusDirection direction)
    {
        var focusableList = _focusableComponents.Where(c => c.CanFocus).ToList();
        
        if (focusableList.Count == 0)
            return null;
            
        if (_focusedComponent == null)
            return focusableList.FirstOrDefault();
        
        var currentIndex = focusableList.IndexOf(_focusedComponent);
        if (currentIndex == -1)
            return focusableList.FirstOrDefault();
        
        int nextIndex = direction switch
        {
            FocusDirection.Next => (currentIndex + 1) % focusableList.Count,
            FocusDirection.Previous => (currentIndex - 1 + focusableList.Count) % focusableList.Count,
            _ => currentIndex
        };
        
        return focusableList[nextIndex];
    }
    
    /// <summary>
    /// Clears all registered components and focus.
    /// </summary>
    public void Clear()
    {
        _focusedComponent?.OnLostFocus();
        _focusedComponent = null;
        _focusableComponents.Clear();
    }
}

/// <summary>
/// Specifies the direction for focus navigation.
/// </summary>
public enum FocusDirection
{
    /// <summary>
    /// Move to the next focusable component (Tab).
    /// </summary>
    Next,
    
    /// <summary>
    /// Move to the previous focusable component (Shift+Tab).
    /// </summary>
    Previous
}