using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// A checkbox component for boolean input.
/// </summary>
public class Checkbox : ISimpleComponent
{
    private readonly string _label;
    private readonly Binding<bool> _isChecked;
    private readonly string _checkedMark;
    private readonly string _uncheckedMark;
    private readonly bool _labelFirst;

    public Checkbox(
        string label,
        Binding<bool> isChecked,
        string checkedMark = "[×]",
        string uncheckedMark = "[ ]",
        bool labelFirst = true)
    {
        _label = label ?? "";
        _isChecked = isChecked ?? throw new ArgumentNullException(nameof(isChecked));
        _checkedMark = checkedMark;
        _uncheckedMark = uncheckedMark;
        _labelFirst = labelFirst;
    }

    // Internal accessors for view instance
    internal string GetLabel() => _label;
    internal Binding<bool> GetIsCheckedBinding() => _isChecked;
    internal string GetCheckedMark() => _checkedMark;
    internal string GetUncheckedMark() => _uncheckedMark;
    internal bool GetLabelFirst() => _labelFirst;

    public VirtualNode Render()
    {
        throw new InvalidOperationException("Checkbox declarations should not be rendered directly. Use ViewInstanceManager.");
    }
}