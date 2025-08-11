using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace Andy.TUI.Terminal.Tests.Integration;

/// <summary>
/// Simple test to verify basic layout positioning works.
/// </summary>
public class SimpleLayoutTest
{
    [Fact]
    public void SimpleTextElement_ShouldRenderAtPosition()
    {
        // Arrange
        var mockRenderingSystem = new Mock<IRenderingSystem>();
        var renderer = new VirtualDomRenderer(mockRenderingSystem.Object);
        var textCalls = new List<(int x, int y, string text, Style style)>();

        mockRenderingSystem.Setup(r => r.WriteText(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Style>()))
            .Callback<int, int, string, Style>((x, y, text, style) => textCalls.Add((x, y, text, style)));

        var textNode = VirtualDomBuilder.Element("text")
            .WithProp("x", 10)
            .WithProp("y", 5)
            .WithProp("style", Style.Default)
            .WithChild(VirtualDomBuilder.Text("Hello World"))
            .Build();

        // Act
        renderer.Render(textNode);

        // Assert
        Assert.Single(textCalls);
        var call = textCalls[0];
        Assert.Equal(10, call.x);
        Assert.Equal(5, call.y);
        Assert.Equal("Hello World", call.text);
    }

    [Fact]
    public void TwoTextElements_ShouldRenderAtDifferentPositions()
    {
        // Arrange
        var mockRenderingSystem = new Mock<IRenderingSystem>();
        var renderer = new VirtualDomRenderer(mockRenderingSystem.Object);
        var textCalls = new List<(int x, int y, string text, Style style)>();

        mockRenderingSystem.Setup(r => r.WriteText(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Style>()))
            .Callback<int, int, string, Style>((x, y, text, style) => textCalls.Add((x, y, text, style)));

        var fragment = VirtualDomBuilder.Fragment(
            VirtualDomBuilder.Element("text")
                .WithProp("x", 2)
                .WithProp("y", 3)
                .WithProp("style", Style.Default)
                .WithChild(VirtualDomBuilder.Text("First"))
                .Build(),
            VirtualDomBuilder.Element("text")
                .WithProp("x", 2)
                .WithProp("y", 6)
                .WithProp("style", Style.Default)
                .WithChild(VirtualDomBuilder.Text("Second"))
                .Build()
        );

        // Act
        renderer.Render(fragment);

        // Assert
        Assert.Equal(2, textCalls.Count);

        var firstCall = textCalls.Find(call => call.text == "First");
        var secondCall = textCalls.Find(call => call.text == "Second");

        Assert.Equal(2, firstCall.x);
        Assert.Equal(3, firstCall.y);
        Assert.Equal(2, secondCall.x);
        Assert.Equal(6, secondCall.y);
    }
}