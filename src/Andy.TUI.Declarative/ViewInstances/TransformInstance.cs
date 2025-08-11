using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Transform view.
/// </summary>
public class TransformInstance : ViewInstance
{
    private string _text = "";
    private TextTransform _transform = TextTransform.None;
    private Style _style = Style.Default;
    private string _transformedText = "";

    public TransformInstance(string id) : base(id)
    {
    }

    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Transform transform)
            throw new ArgumentException("Expected Transform declaration");

        _text = transform.GetText();
        _transform = transform.GetTransform();
        _style = transform.GetStyle();

        // Apply transformation
        _transformedText = Transform.ApplyTransform(_text, _transform);
    }

    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox
        {
            Width = constraints.ConstrainWidth(_transformedText.Length),
            Height = constraints.ConstrainHeight(1)
        };

        return layout;
    }

    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (string.IsNullOrEmpty(_transformedText))
            return Fragment();

        // Truncate if necessary
        var displayText = _transformedText;
        if (displayText.Length > layout.Width)
        {
            displayText = displayText.Substring(0, (int)layout.Width);
        }

        return Element("text")
            .WithProp("x", layout.AbsoluteX)
            .WithProp("y", layout.AbsoluteY)
            .WithProp("style", _style)
            .WithChild(new TextNode(displayText))
            .Build();
    }
}