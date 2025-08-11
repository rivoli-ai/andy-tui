using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance for Checkbox component.
/// </summary>
public class CheckboxInstance : ViewInstance, IFocusable
{
    private string _label = "";
    private Binding<bool>? _isCheckedBinding;
    private string _checkedMark = "[×]";
    private string _uncheckedMark = "[ ]";
    private bool _labelFirst = true;

    public CheckboxInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Checkbox checkbox)
            throw new InvalidOperationException($"Expected Checkbox, got {viewDeclaration.GetType()}");

        _label = checkbox.GetLabel();
        _isCheckedBinding = checkbox.GetIsCheckedBinding();
        _checkedMark = checkbox.GetCheckedMark();
        _uncheckedMark = checkbox.GetUncheckedMark();
        _labelFirst = checkbox.GetLabelFirst();
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var mark = _isCheckedBinding?.Value ?? false ? _checkedMark : _uncheckedMark;
        var text = _labelFirst ? $"{_label} {mark}" : $"{mark} {_label}";

        var width = Math.Min(text.Length, constraints.MaxWidth);
        var height = 1;

        return new LayoutBox { Width = width, Height = height };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        var isChecked = _isCheckedBinding?.Value ?? false;
        var mark = isChecked ? _checkedMark : _uncheckedMark;
        var text = _labelFirst ? $"{_label} {mark}" : $"{mark} {_label}";

        var style = Style.Default;
        if (IsFocused)
        {
            style = style.WithForegroundColor(Color.Black).WithBackgroundColor(Color.Cyan);
        }

        return Element("text")
            .WithProp("style", style)
            .WithProp("x", (int)layout.AbsoluteX)
            .WithProp("y", (int)layout.AbsoluteY)
            .WithChild(new TextNode(text))
            .Build();
    }

    public bool IsFocused { get; private set; }
    public bool CanFocus => true;

    public void OnGotFocus()
    {
        IsFocused = true;
        InvalidateView();
    }

    public void OnLostFocus()
    {
        IsFocused = false;
        InvalidateView();
    }

    public bool HandleKeyPress(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Spacebar:
            case ConsoleKey.Enter:
                if (_isCheckedBinding != null)
                {
                    _isCheckedBinding.Value = !_isCheckedBinding.Value;
                    InvalidateView();
                }
                return true;

            default:
                return false;
        }
    }
}