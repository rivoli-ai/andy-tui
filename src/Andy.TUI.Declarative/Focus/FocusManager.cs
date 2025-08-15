using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Focus;

/// <summary>
/// Manages keyboard focus for focusable components.
/// </summary>
public class FocusManager
{
    private readonly List<IFocusable> _focusableComponents = new();
    private IFocusable? _focusedComponent;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the currently focused component.
    /// </summary>
    public IFocusable? FocusedComponent => _focusedComponent;

    /// <summary>
    /// Gets the number of registered focusable components.
    /// </summary>
    public int FocusableCount => _focusableComponents.Count;

    /// <summary>
    /// Exposes the current list of focusables (read-only) for diagnostics.
    /// </summary>
    public IReadOnlyList<IFocusable> Focusables => _focusableComponents.AsReadOnly();

    public FocusManager()
    {
        _logger = LogManager.GetLogger<FocusManager>();
        _logger.Info("FocusManager initialized");
    }

    private static void Guard(bool condition, string message)
    {
        if (!condition) throw new FocusInvariantViolationException(message);
    }

    private void ValidateInvariants(string where)
    {
        // Unique focusable entries
        Guard(_focusableComponents.Distinct().Count() == _focusableComponents.Count,
            $"Duplicate focusable registration detected ({where})");

        // Focused component must be either null or present in list
        if (_focusedComponent != null)
        {
            Guard(_focusableComponents.Contains(_focusedComponent),
                $"Focused component not in registry ({where})");
        }
    }

    /// <summary>
    /// Registers a focusable component.
    /// </summary>
    public void RegisterFocusable(IFocusable component)
    {
        if (!_focusableComponents.Contains(component))
        {
            _focusableComponents.Add(component);
            _logger.Debug($"Registered focusable: {component.GetType().Name} (Total: {_focusableComponents.Count})");
            ValidateInvariants("RegisterFocusable");
        }
    }

    /// <summary>
    /// Unregisters a focusable component.
    /// </summary>
    public void UnregisterFocusable(IFocusable component)
    {
        // Remember index before removal to choose a stable next focus target
        var originalIndex = _focusableComponents.IndexOf(component);
        _focusableComponents.Remove(component);
        _logger.Debug($"Unregistered focusable: {component.GetType().Name} (Remaining: {_focusableComponents.Count})");
        ValidateInvariants("UnregisterFocusable:post-remove");

        if (_focusedComponent == component)
        {
            var candidates = _focusableComponents.Where(c => c.CanFocus).ToList();
            if (candidates.Count == 0)
            {
                _logger.Debug("No focusable candidates after unregistering, clearing focus");
                SetFocus(null);
            }
            else
            {
                var nextIndex = Math.Min(Math.Max(originalIndex, 0), candidates.Count - 1);
                _logger.Debug($"Auto-focusing next component at index {nextIndex}");
                SetFocus(candidates[nextIndex]);
            }
        }
    }

    /// <summary>
    /// Sets focus to a specific component.
    /// </summary>
    public void SetFocus(IFocusable? component)
    {
        if (component == _focusedComponent)
        {
            _logger.Debug($"SetFocus: Already focused on {component?.GetType().Name ?? "null"}");
            return;
        }

        if (component != null && !component.CanFocus)
        {
            _logger.Warning($"SetFocus: Component {component.GetType().Name} cannot receive focus (CanFocus=false)");
            return;
        }

        var oldComponent = _focusedComponent;
        _logger.LogFocusChange(
            oldComponent?.GetType().Name,
            component?.GetType().Name,
            "SetFocus");

        _focusedComponent?.OnLostFocus();
        _focusedComponent = component;
        _focusedComponent?.OnGotFocus();
        ValidateInvariants("SetFocus");
    }

    /// <summary>
    /// Moves focus in the specified direction.
    /// </summary>
    public void MoveFocus(FocusDirection direction)
    {
        _logger.Debug($"MoveFocus: Direction={direction}");
        var next = GetNextFocusable(direction);
        if (next != null)
        {
            _logger.Debug($"MoveFocus: Found next focusable: {next.GetType().Name}");
            SetFocus(next);
        }
        else
        {
            _logger.Debug($"MoveFocus: No focusable component found in direction {direction}");
        }
    }

    /// <summary>
    /// Gets the next focusable component in the specified direction.
    /// </summary>
    private IFocusable? GetNextFocusable(FocusDirection direction)
    {
        var focusableList = _focusableComponents.Where(c => c.CanFocus).ToList();

        if (focusableList.Count == 0)
        {
            _logger.Debug("GetNextFocusable: No focusable components available");
            return null;
        }

        if (_focusedComponent == null)
        {
            _logger.Debug($"GetNextFocusable: No current focus, returning first focusable: {focusableList.FirstOrDefault()?.GetType().Name}");
            return focusableList.FirstOrDefault();
        }

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

    /// <summary>
    /// Sets the focusable components in a specific order.
    /// </summary>
    public void SetFocusableOrder(IEnumerable<IFocusable> orderedFocusables)
    {
        var currentFocus = _focusedComponent;
        _focusableComponents.Clear();
        _focusableComponents.AddRange(orderedFocusables);

        // Restore focus if the component still exists
        if (currentFocus != null && _focusableComponents.Contains(currentFocus))
        {
            _focusedComponent = currentFocus;
        }
        else if (_focusedComponent != null && !_focusableComponents.Contains(_focusedComponent))
        {
            // Lost focus, clear it
            _focusedComponent?.OnLostFocus();
            _focusedComponent = null;
        }

        _logger.Debug($"Set focusable order with {_focusableComponents.Count} components");
        ValidateInvariants("SetFocusableOrder");
    }
}

public sealed class FocusInvariantViolationException : System.Exception
{
    public FocusInvariantViolationException(string message) : base(message) { }
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