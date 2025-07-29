namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Represents a text node in the virtual DOM.
/// </summary>
public sealed class TextNode : VirtualNode
{
    /// <summary>
    /// Gets the text content.
    /// </summary>
    public string Content { get; }
    
    public override VirtualNodeType Type => VirtualNodeType.Text;
    
    /// <summary>
    /// Initializes a new instance of the TextNode class.
    /// </summary>
    /// <param name="content">The text content.</param>
    public TextNode(string content)
    {
        Content = content ?? string.Empty;
    }
    
    public override bool Equals(VirtualNode? other)
    {
        return other is TextNode textNode && 
               Content == textNode.Content &&
               Key == textNode.Key;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Content, Key);
    }
    
    public override VirtualNode Clone()
    {
        return new TextNode(Content) { Key = Key };
    }
    
    public override void Accept(IVirtualNodeVisitor visitor)
    {
        visitor.VisitText(this);
    }
    
    public override string ToString()
    {
        return $"Text: \"{Content}\"";
    }
}