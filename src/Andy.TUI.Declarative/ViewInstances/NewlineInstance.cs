using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Newline view.
/// </summary>
public class NewlineInstance : ViewInstance
{
    private int _count = 1;

    public NewlineInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Newline newline)
            throw new ArgumentException("Expected Newline declaration");

        _count = newline.GetCount();
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Newline takes minimal width but consumes vertical space
        return new LayoutBox
        {
            Width = 0,
            Height = _count
        };
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        // Newline doesn't render any visible content
        // Its purpose is to consume vertical space in the layout
        return Fragment();
    }
}