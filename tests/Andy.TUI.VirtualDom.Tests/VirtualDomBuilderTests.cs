using Andy.TUI.VirtualDom;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Core.Tests.VirtualDom;

public class VirtualDomBuilderTests
{
    [Fact]
    public void Text_CreatesTextNode()
    {
        var node = Text("Hello World");

        Assert.Equal("Hello World", node.Content);
        Assert.Equal(VirtualNodeType.Text, node.Type);
    }

    [Fact]
    public void Element_CreatesElementBuilder()
    {
        var element = Element("div")
            .WithProp("id", "test")
            .WithClass("container")
            .WithText("Content")
            .Build();

        Assert.Equal("div", element.TagName);
        Assert.Equal("test", element.Props["id"]);
        Assert.Equal("container", element.Props["class"]);
        Assert.Single(element.Children);
        Assert.IsType<TextNode>(element.Children[0]);
    }

    [Fact]
    public void Fragment_CreatesFragmentNode()
    {
        var fragment = Fragment(
            Text("Child 1"),
            Text("Child 2"),
            Text("Child 3")
        );

        Assert.Equal(VirtualNodeType.Fragment, fragment.Type);
        Assert.Equal(3, fragment.Children.Count);
    }

    [Fact]
    public void Component_CreatesComponentNode()
    {
        var props = new Dictionary<string, object?> { ["name"] = "Test" };
        var component = Component<string>(props);

        Assert.Equal(typeof(string), component.ComponentType);
        Assert.Equal("Test", component.Props["name"]);
    }

    [Fact]
    public void ElementBuilder_FluentApi_WorksCorrectly()
    {
        var element = Div()
            .WithKey("container")
            .WithId("main")
            .WithClass("container flex")
            .WithStyle("padding: 10px")
            .WithWidth(100)
            .WithHeight(50)
            .WithFlex(1)
            .WithPadding(10)
            .WithMargin(5)
            .WithBorder("solid")
            .WithBackground("blue")
            .WithForeground("white")
            .WithAlign("center")
            .WithJustify("space-between")
            .OnClick(() => { })
            .WithChildren(
                Span().WithText("Child 1"),
                Span().WithText("Child 2")
            )
            .Build();

        Assert.Equal("div", element.TagName);
        Assert.Equal("container", element.Key);
        Assert.Equal("main", element.Props["id"]);
        Assert.Equal("container flex", element.Props["class"]);
        Assert.Equal("padding: 10px", element.Props["style"]);
        Assert.Equal(100, element.Props["width"]);
        Assert.Equal(50, element.Props["height"]);
        Assert.Equal(1, element.Props["flex"]);
        Assert.Equal(10, element.Props["padding"]);
        Assert.Equal(5, element.Props["margin"]);
        Assert.Equal("solid", element.Props["border"]);
        Assert.Equal("blue", element.Props["background"]);
        Assert.Equal("white", element.Props["foreground"]);
        Assert.Equal("center", element.Props["align"]);
        Assert.Equal("space-between", element.Props["justify"]);
        Assert.NotNull(element.Props["onClick"]);
        Assert.Equal(2, element.Children.Count);
    }

    [Fact]
    public void ElementBuilder_ImplicitConversion_Works()
    {
        ElementNode element = Div().WithText("Test");
        VirtualNode node = Span().WithText("Test");

        Assert.Equal("div", element.TagName);
        Assert.Equal("span", ((ElementNode)node).TagName);
    }

    [Fact]
    public void ElementBuilder_CommonElements_CreateCorrectTags()
    {
        Assert.Equal("div", Div().Build().TagName);
        Assert.Equal("span", Span().Build().TagName);
        Assert.Equal("box", Box().Build().TagName);
        Assert.Equal("vbox", VBox().Build().TagName);
        Assert.Equal("hbox", HBox().Build().TagName);
        Assert.Equal("button", Button().Build().TagName);
        Assert.Equal("input", Input().Build().TagName);
        Assert.Equal("label", Label().Build().TagName);
        Assert.Equal("list", List().Build().TagName);
        Assert.Equal("li", ListItem().Build().TagName);
    }

    [Fact]
    public void ElementBuilder_WithProps_MergesProperties()
    {
        var props = new Dictionary<string, object?>
        {
            ["prop1"] = "value1",
            ["prop2"] = 42
        };

        var element = Element("div")
            .WithProp("prop3", true)
            .WithProps(props)
            .Build();

        Assert.Equal("value1", element.Props["prop1"]);
        Assert.Equal(42, element.Props["prop2"]);
        Assert.Equal(true, element.Props["prop3"]);
    }

    [Fact]
    public void ElementBuilder_EventHandlers_AreStored()
    {
        var clickHandled = false;
        var enterHandled = false;
        var focusHandled = false;
        var blurHandled = false;

        var element = Button()
            .OnClick(() => clickHandled = true)
            .OnEnter(() => enterHandled = true)
            .OnFocus(() => focusHandled = true)
            .OnBlur(() => blurHandled = true)
            .Build();

        // Invoke handlers
        (element.Props["onClick"] as Action)?.Invoke();
        (element.Props["onEnter"] as Action)?.Invoke();
        (element.Props["onFocus"] as Action)?.Invoke();
        (element.Props["onBlur"] as Action)?.Invoke();

        Assert.True(clickHandled);
        Assert.True(enterHandled);
        Assert.True(focusHandled);
        Assert.True(blurHandled);
    }

    [Fact]
    public void ComplexTree_BuildsCorrectly()
    {
        var tree = VBox()
            .WithClass("app")
            .WithChildren(
                // Header
                HBox()
                    .WithClass("header")
                    .WithHeight(3)
                    .WithChildren(
                        Label().WithText("My App"),
                        Span().WithFlex(1), // Spacer
                        Button().WithText("Settings")
                    ),

                // Content
                Box()
                    .WithClass("content")
                    .WithFlex(1)
                    .WithChildren(
                        List()
                            .WithChildren(
                                ListItem().WithKey("1").WithText("Item 1"),
                                ListItem().WithKey("2").WithText("Item 2"),
                                ListItem().WithKey("3").WithText("Item 3")
                            )
                    ),

                // Footer
                HBox()
                    .WithClass("footer")
                    .WithHeight(1)
                    .WithText("Status: Ready")
            )
            .Build();

        Assert.Equal("vbox", tree.TagName);
        Assert.Equal(3, tree.Children.Count);

        var header = (ElementNode)tree.Children[0];
        Assert.Equal("header", header.Props["class"]);
        Assert.Equal(3, header.Children.Count);

        var content = (ElementNode)tree.Children[1];
        Assert.Equal(1, content.Props["flex"]);

        var list = (ElementNode)content.Children[0];
        Assert.Equal(3, list.Children.Count);
        Assert.All(list.Children, child => Assert.NotNull(((ElementNode)child).Key));
    }
}