using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Declarative.Components;
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
    
    public override VirtualNode Render()
    {
        return Element("text")
            .WithProp("style", _style)
            .WithProp("x", 0)
            .WithProp("y", 0)
            .WithChild(new TextNode(_content))
            .Build();
    }
}