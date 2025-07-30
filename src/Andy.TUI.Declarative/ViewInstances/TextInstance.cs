using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Terminal;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative;

/// <summary>
/// Runtime instance of a Text view.
/// </summary>
public class TextInstance : ViewInstance
{
    private string _content = "";
    private Style _style = Style.Default;
    
    public TextInstance(string id) : base(id)
    {
    }
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not Text text)
            throw new ArgumentException("Expected Text declaration");
        
        _content = text.GetContent();
        _style = text.GetStyle();
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        // Text takes up only the space it needs
        var layout = new LayoutBox
        {
            Width = _content.Length,
            Height = 1
        };
        
        // Constrain to available space
        layout.Width = constraints.ConstrainWidth(layout.Width);
        layout.Height = constraints.ConstrainHeight(layout.Height);
        
        return layout;
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        return Element("text")
            .WithProp("style", _style)
            .WithProp("x", layout.AbsoluteX)
            .WithProp("y", layout.AbsoluteY)
            .WithChild(new TextNode(_content))
            .Build();
    }
}