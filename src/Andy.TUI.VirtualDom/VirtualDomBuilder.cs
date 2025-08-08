using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Core.VirtualDom;

public static class VirtualDomBuilder
{
    public static TextNode Text(string content) => new(content);
    public static ElementBuilder Element(string tagName) => new(tagName);
    public static FragmentNode Fragment(params VirtualNode[] children) => new(children);
    public static FragmentNode Fragment(IEnumerable<VirtualNode> children) => new(children);
    public static ComponentNode Component<TComponent>(Dictionary<string, object?>? props = null) => ComponentNode.Create<TComponent>(props);
    public static ComponentNode Component(Type componentType, Dictionary<string, object?>? props = null) => new(componentType, props);
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
    public static EmptyNode Empty() => new EmptyNode();
    public static ClippingNode Clip(int x, int y, int width, int height, params VirtualNode[] children) => new ClippingNode(x, y, width, height, children);
}

public class ElementBuilder
{
    private readonly string _tagName;
    private readonly Dictionary<string, object?> _props = new();
    private readonly List<VirtualNode> _children = new();
    private string? _key;

    internal ElementBuilder(string tagName) { _tagName = tagName; }
    public ElementBuilder WithKey(string key) { _key = key; return this; }
    public ElementBuilder WithProp(string name, object? value) { _props[name] = value; return this; }
    public ElementBuilder WithProps(Dictionary<string, object?> props) { foreach (var kvp in props) { _props[kvp.Key] = kvp.Value; } return this; }
    public ElementBuilder WithChild(VirtualNode child) { _children.Add(child); return this; }
    public ElementBuilder WithChildren(params VirtualNode[] children) { _children.AddRange(children); return this; }
    public ElementBuilder WithChildren(IEnumerable<VirtualNode> children) { _children.AddRange(children); return this; }
    public ElementBuilder WithText(string text) { _children.Add(VirtualDomBuilder.Text(text)); return this; }
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
    public ElementBuilder OnClick(Action handler) => WithProp("onClick", handler);
    public ElementBuilder OnEnter(Action handler) => WithProp("onEnter", handler);
    public ElementBuilder OnFocus(Action handler) => WithProp("onFocus", handler);
    public ElementBuilder OnBlur(Action handler) => WithProp("onBlur", handler);
    public ElementNode Build()
    {
        var element = new ElementNode(_tagName, _props, _children.ToArray());
        if (_key != null) { element = new ElementNode(_tagName, _props, _children.ToArray()) { Key = _key }; }
        return element;
    }
    public static implicit operator ElementNode(ElementBuilder builder) => builder.Build();
    public static implicit operator VirtualNode(ElementBuilder builder) => builder.Build();
}


