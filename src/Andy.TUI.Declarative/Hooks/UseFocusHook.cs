using System;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for managing focus state in a component.
/// </summary>
public class UseFocusHook : IHook
{
    private bool _isFocused = false;
    private bool _canFocus = true;
    private Action? _onFocus;
    private Action? _onBlur;
    private FocusManager? _focusManager;

    /// <summary>
    /// Gets whether this component currently has focus.
    /// </summary>
    public bool IsFocused => _isFocused;

    /// <summary>
    /// Gets or sets whether this component can receive focus.
    /// </summary>
    public bool CanFocus
    {
        get => _canFocus;
        set => _canFocus = value;
    }

    /// <summary>
    /// Sets up focus handling for the component.
    /// </summary>
    /// <param name="focusManager">The focus manager to work with.</param>
    /// <param name="onFocus">Callback when component gains focus.</param>
    /// <param name="onBlur">Callback when component loses focus.</param>
    /// <param name="canFocus">Whether the component can receive focus.</param>
    public void SetupFocus(
        FocusManager? focusManager,
        Action? onFocus = null,
        Action? onBlur = null,
        bool canFocus = true)
    {
        _focusManager = focusManager;
        _onFocus = onFocus;
        _onBlur = onBlur;
        _canFocus = canFocus;

        // Note: Actual focus registration should be handled by the component
        // that uses this hook, not the hook itself
    }

    /// <summary>
    /// Called when the component gains focus.
    /// </summary>
    public void OnGotFocus()
    {
        _isFocused = true;
        _onFocus?.Invoke();
    }

    /// <summary>
    /// Called when the component loses focus.
    /// </summary>
    public void OnLostFocus()
    {
        _isFocused = false;
        _onBlur?.Invoke();
    }

    public void Dispose()
    {
        _focusManager = null;
        _onFocus = null;
        _onBlur = null;
    }
}