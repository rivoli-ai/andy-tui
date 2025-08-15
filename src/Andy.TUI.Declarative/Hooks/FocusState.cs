using System;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Represents the focus state for a component using the UseFocus hook.
/// </summary>
public class FocusState
{
    private readonly UseFocusHook _hook;

    public FocusState(UseFocusHook hook)
    {
        _hook = hook ?? throw new ArgumentNullException(nameof(hook));
    }

    /// <summary>
    /// Gets whether the component currently has focus.
    /// </summary>
    public bool IsFocused => _hook.IsFocused;

    /// <summary>
    /// Gets or sets whether the component can receive focus.
    /// </summary>
    public bool CanFocus
    {
        get => _hook.CanFocus;
        set => _hook.CanFocus = value;
    }

}