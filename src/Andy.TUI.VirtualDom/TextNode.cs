using System;

namespace Andy.TUI.VirtualDom;

public sealed class TextNode : VirtualNode
{
    public string Content { get; }
    public override VirtualNodeType Type => VirtualNodeType.Text;
    public TextNode(string content) { Content = content ?? string.Empty; }
    public override bool Equals(VirtualNode? other) => other is TextNode t && Content == t.Content && Key == t.Key;
    public override int GetHashCode() => HashCode.Combine(Type, Content, Key);
    public override VirtualNode Clone() => new TextNode(Content) { Key = Key };
    public override void Accept(IVirtualNodeVisitor visitor) => visitor.VisitText(this);
}


