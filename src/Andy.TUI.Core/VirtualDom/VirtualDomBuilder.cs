namespace Andy.TUI.Core.VirtualDom;

/// <summary>
/// Provides a fluent API for building virtual DOM trees.
/// </summary>
public static class VirtualDomBuilder
{
    /// <summary>
    /// Creates a text node.
    /// </summary>
    public static TextNode Text(string content) => new(content);
    
    /// <summary>
    /// Creates an element node.
    /// </summary>
    public static ElementBuilder Element(string tagName) => new(tagName);
    
    /// <summary>
    /// Creates a fragment node.
    /// </summary>
    public static FragmentNode Fragment(params VirtualNode[] children) => new(children);
    
    /// <summary>
    /// Creates a fragment node from an enumerable.
    /// </summary>
    public static FragmentNode Fragment(IEnumerable<VirtualNode> children) => new(children);
    
    /// <summary>
    /// Creates a component node.
    /// </summary>
    public static ComponentNode Component<TComponent>(Dictionary<string, object?>? props = null) => 
        ComponentNode.Create<TComponent>(props);
    
    /// <summary>
    /// Creates a component node.
    /// </summary>
    public static ComponentNode Component(Type componentType, Dictionary<string, object?>? props = null) => 
        new(componentType, props);
    
    // Common element shortcuts
    public static ElementBuilder Div() => Element("div");
    public static ElementBuilder Span() => Element("span");
    public static ElementBuilder Box() => Element("box");
    public static ElementBuilder VBox() => Element("vbox");
    public static ElementBuilder HBox() => Element("hbox");
    public static ElementBuilder Button() => Element("button");
    public static ElementBuilder Input() => Element("input");
    public static ElementBuilder Label() => Element("label");
    public static ElementBuilder List() => Element("list");
    public static ElementBuilder ListItem() => Element("li");
    
    /// <summary>
    /// Creates an empty node.
    /// </summary>
    public static EmptyNode Empty() => new EmptyNode();
    
    /// <summary>
    /// Creates a clipping node that constrains children to a rectangular area.
    /// </summary>
    public static ClippingNode Clip(int x, int y, int width, int height, params VirtualNode[] children) 
        => new ClippingNode(x, y, width, height, children);
}

/// <summary>
/// Builder for creating element nodes with a fluent API.
/// </summary>
public class ElementBuilder
{
    private readonly string _tagName;
    private readonly Dictionary<string, object?> _props = new();
    private readonly List<VirtualNode> _children = new();
    private string? _key;
    
    internal ElementBuilder(string tagName)
    {
        _tagName = tagName;
    }
    
    /// <summary>
    /// Sets the key for the element.
    /// </summary>
    public ElementBuilder WithKey(string key)
    {
        _key = key;
        return this;
    }
    
    /// <summary>
    /// Adds a property to the element.
    /// </summary>
    public ElementBuilder WithProp(string name, object? value)
    {
        _props[name] = value;
        return this;
    }
    
    /// <summary>
    /// Adds multiple properties to the element.
    /// </summary>
    public ElementBuilder WithProps(Dictionary<string, object?> props)
    {
        foreach (var kvp in props)
        {
            _props[kvp.Key] = kvp.Value;
        }
        return this;
    }
    
    /// <summary>
    /// Adds a child node to the element.
    /// </summary>
    public ElementBuilder WithChild(VirtualNode child)
    {
        _children.Add(child);
        return this;
    }
    
    /// <summary>
    /// Adds multiple child nodes to the element.
    /// </summary>
    public ElementBuilder WithChildren(params VirtualNode[] children)
    {
        _children.AddRange(children);
        return this;
    }
    
    /// <summary>
    /// Adds multiple child nodes to the element.
    /// </summary>
    public ElementBuilder WithChildren(IEnumerable<VirtualNode> children)
    {
        _children.AddRange(children);
        return this;
    }
    
    /// <summary>
    /// Adds text content as a child.
    /// </summary>
    public ElementBuilder WithText(string text)
    {
        _children.Add(VirtualDomBuilder.Text(text));
        return this;
    }
    
    // Common property shortcuts
    public ElementBuilder WithClass(string className) => WithProp("class", className);
    public ElementBuilder WithId(string id) => WithProp("id", id);
    public ElementBuilder WithStyle(string style) => WithProp("style", style);
    public ElementBuilder WithWidth(int width) => WithProp("width", width);
    public ElementBuilder WithHeight(int height) => WithProp("height", height);
    public ElementBuilder WithFlex(int flex) => WithProp("flex", flex);
    public ElementBuilder WithPadding(int padding) => WithProp("padding", padding);
    public ElementBuilder WithMargin(int margin) => WithProp("margin", margin);
    public ElementBuilder WithBorder(string border) => WithProp("border", border);
    public ElementBuilder WithBackground(string color) => WithProp("background", color);
    public ElementBuilder WithForeground(string color) => WithProp("foreground", color);
    public ElementBuilder WithAlign(string alignment) => WithProp("align", alignment);
    public ElementBuilder WithJustify(string justification) => WithProp("justify", justification);
    
    // Event handlers
    public ElementBuilder OnClick(Action handler) => WithProp("onClick", handler);
    public ElementBuilder OnEnter(Action handler) => WithProp("onEnter", handler);
    public ElementBuilder OnFocus(Action handler) => WithProp("onFocus", handler);
    public ElementBuilder OnBlur(Action handler) => WithProp("onBlur", handler);
    
    /// <summary>
    /// Builds the element node.
    /// </summary>
    public ElementNode Build()
    {
        var element = new ElementNode(_tagName, _props, _children.ToArray());
        if (_key != null)
        {
            element = new ElementNode(_tagName, _props, _children.ToArray()) { Key = _key };
        }
        return element;
    }
    
    /// <summary>
    /// Implicitly converts the builder to an ElementNode.
    /// </summary>
    public static implicit operator ElementNode(ElementBuilder builder) => builder.Build();
    
    /// <summary>
    /// Implicitly converts the builder to a VirtualNode.
    /// </summary>
    public static implicit operator VirtualNode(ElementBuilder builder) => builder.Build();
}