using System;
using System.Collections.Generic;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A radio button group component for selecting one option from many.
/// </summary>
public class RadioGroup<T> : ISimpleComponent
{
    private readonly string _label;
    private readonly IReadOnlyList<T> _options;
    private readonly Binding<Optional<T>> _selectedOption;
    private readonly Func<T, string> _optionRenderer;
    private readonly string _selectedMark;
    private readonly string _unselectedMark;
    private readonly bool _vertical;
    
    public RadioGroup(
        string label,
        IReadOnlyList<T> options,
        Binding<Optional<T>> selectedOption,
        Func<T, string>? optionRenderer = null,
        string selectedMark = "(•)",
        string unselectedMark = "( )",
        bool vertical = true)
    {
        _label = label ?? "";
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _selectedOption = selectedOption ?? throw new ArgumentNullException(nameof(selectedOption));
        _optionRenderer = optionRenderer ?? (item => item?.ToString() ?? "");
        _selectedMark = selectedMark;
        _unselectedMark = unselectedMark;
        _vertical = vertical;
    }
    
    // Internal accessors for view instance
    internal string GetLabel() => _label;
    internal IReadOnlyList<T> GetOptions() => _options;
    internal Binding<Optional<T>> GetSelectedOptionBinding() => _selectedOption;
    internal Func<T, string> GetOptionRenderer() => _optionRenderer;
    internal string GetSelectedMark() => _selectedMark;
    internal string GetUnselectedMark() => _unselectedMark;
    internal bool GetVertical() => _vertical;
    
    public VirtualNode Render()
    {
        throw new InvalidOperationException("RadioGroup declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}