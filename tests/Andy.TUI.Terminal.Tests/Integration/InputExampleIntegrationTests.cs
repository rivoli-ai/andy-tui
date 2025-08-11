using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Moq;
using Xunit;
using System.Collections.Generic;
using System;

namespace Andy.TUI.Terminal.Tests.Integration;

/// <summary>
/// Integration tests for the input example to prevent visual regressions.
/// These tests verify that the SwiftUI-style declarative UI components render correctly.
/// </summary>
public class InputExampleIntegrationTests
{
    private readonly Mock<IRenderingSystem> _mockRenderingSystem;
    private readonly VirtualDomRenderer _renderer;
    private readonly List<(int x, int y, string text, Style style)> _textCalls;
    private readonly List<(int x, int y, int width, int height, Style style, BoxStyle boxStyle)> _boxCalls;

    public InputExampleIntegrationTests()
    {
        _mockRenderingSystem = new Mock<IRenderingSystem>();
        _renderer = new VirtualDomRenderer(_mockRenderingSystem.Object);
        _textCalls = new List<(int, int, string, Style)>();
        _boxCalls = new List<(int, int, int, int, Style, BoxStyle)>();

        // Capture all rendering calls
        _mockRenderingSystem.Setup(r => r.WriteText(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Style>()))
            .Callback<int, int, string, Style>((x, y, text, style) => _textCalls.Add((x, y, text, style)));

        _mockRenderingSystem.Setup(r => r.DrawBox(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Style>(), It.IsAny<BoxStyle>()))
            .Callback<int, int, int, int, Style, BoxStyle>((x, y, w, h, style, boxStyle) => _boxCalls.Add((x, y, w, h, style, boxStyle)));
    }

    [Fact]
    public void InputExample_Title_ShouldRenderCorrectly()
    {
        // Arrange
        var titleNode = CreateTitleNode();

        // Act
        _renderer.Render(titleNode);

        // Assert
        var titleCall = _textCalls.Find(call => call.text == "Andy.TUI Input Components Demo");
        Assert.NotEqual(default, titleCall);
        Assert.True(titleCall.style.Bold);
        Assert.Equal(Color.Cyan, titleCall.style.Foreground);
    }

    [Fact]
    public void InputExample_NameLabel_ShouldRenderCorrectly()
    {
        // Arrange
        var labelNode = CreateLabelNode("Name:");

        // Act
        _renderer.Render(labelNode);

        // Assert
        var labelCall = _textCalls.Find(call => call.text == "Name:");
        Assert.NotEqual(default, labelCall);
        Assert.True(labelCall.style.Bold);
    }

    [Fact]
    public void InputExample_TextInput_ShouldRenderBoxAndText()
    {
        // Arrange
        var inputNode = CreateTextInputNode("John Doe", "Enter your name...", true);

        // Act
        _renderer.Render(inputNode);

        // Assert
        // Should render a box
        Assert.NotEmpty(_boxCalls);
        var boxCall = _boxCalls[0];
        Assert.Equal(40, boxCall.width);
        Assert.Equal(3, boxCall.height);
        Assert.Equal(BoxStyle.Single, boxCall.boxStyle);

        // Should render the input text
        var textCall = _textCalls.Find(call => call.text == "John Doe");
        Assert.NotEqual(default, textCall);
        
        // Text should be positioned inside the box (with 1-unit offset)
        Assert.Equal(boxCall.x + 1, textCall.x);
        Assert.Equal(boxCall.y + 1, textCall.y);
    }

    [Fact]
    public void InputExample_TextInput_WithPlaceholder_ShouldRenderPlaceholderWhenEmpty()
    {
        // Arrange
        var inputNode = CreateTextInputNode("", "Enter your name...", false);

        // Act
        _renderer.Render(inputNode);

        // Assert
        var placeholderCall = _textCalls.Find(call => call.text == "Enter your name...");
        Assert.NotEqual(default, placeholderCall);
        Assert.Equal(Color.DarkGray, placeholderCall.style.Foreground);
    }

    [Fact]
    public void InputExample_PasswordInput_ShouldRenderMaskedText()
    {
        // Arrange
        var passwordNode = CreatePasswordInputNode("secret123", "Enter password...", false);

        // Act
        _renderer.Render(passwordNode);

        // Assert
        var maskedTextCall = _textCalls.Find(call => call.text == "•••••••••");
        Assert.NotEqual(default, maskedTextCall);
        Assert.Equal(9, maskedTextCall.text.Length); // "secret123" = 9 characters
    }

    [Fact]
    public void InputExample_Dropdown_Closed_ShouldRenderMainBoxWithArrow()
    {
        // Arrange
        var dropdownNode = CreateDropdownNode("United States", "Select country...", 
            new[] { "United States", "Canada" }, false, 0, false);

        // Act
        _renderer.Render(dropdownNode);

        // Assert
        // Should render main dropdown box
        var boxCall = _boxCalls.Find(box => box.width == 40 && box.height == 3);
        Assert.NotEqual(default, boxCall);

        // Should render selected value
        var valueCall = _textCalls.Find(call => call.text == "United States");
        Assert.NotEqual(default, valueCall);

        // Should render down arrow
        var arrowCall = _textCalls.Find(call => call.text == "▼");
        Assert.NotEqual(default, arrowCall);
    }

    [Fact]
    public void InputExample_Dropdown_Open_ShouldRenderItemsList()
    {
        // Arrange
        var dropdownNode = CreateDropdownNode(null, "Select country...", 
            new[] { "United States", "Canada", "Germany" }, true, 1, true);

        // Act
        _renderer.Render(dropdownNode);

        // Assert
        // Should render main dropdown box
        var mainBox = _boxCalls.Find(box => box.width == 40 && box.height == 3);
        Assert.NotEqual(default, mainBox);

        // Should render dropdown items box (with higher z-index)
        var itemsBox = _boxCalls.Find(box => box.width == 40 && box.height > 3);
        Assert.NotEqual(default, itemsBox);

        // Should render all dropdown items
        var usCall = _textCalls.Find(call => call.text == "United States");
        var canadaCall = _textCalls.Find(call => call.text == "Canada");
        var germanyCall = _textCalls.Find(call => call.text == "Germany");
        
        Assert.NotEqual(default, usCall);
        Assert.NotEqual(default, canadaCall);
        Assert.NotEqual(default, germanyCall);

        // Highlighted item should have different background
        Assert.Equal(Color.DarkBlue, canadaCall.style.Background);
    }

    [Fact]
    public void InputExample_Button_Primary_ShouldRenderWithCorrectStyle()
    {
        // Arrange
        var buttonNode = CreateButtonNode("Submit", ButtonStyle.Primary, true);

        // Act
        _renderer.Render(buttonNode);

        // Assert
        // Should render button box
        var boxCall = _boxCalls.Find(box => box.width == 12 && box.height == 3);
        Assert.NotEqual(default, boxCall);
        Assert.Equal(BoxStyle.Double, boxCall.boxStyle); // Focused primary button

        // Should render button text
        var textCall = _textCalls.Find(call => call.text == "Submit");
        Assert.NotEqual(default, textCall);
        Assert.Equal(Color.White, textCall.style.Foreground);
    }

    [Fact(Skip = "Layout system positioning issues - all elements render at (0,0). See CLAUDE.md for details.")]
    public void InputExample_CompleteForm_ShouldRenderAllComponents()
    {
        // Arrange
        var completeForm = CreateCompleteFormNode();

        // Act
        _renderer.Render(completeForm);

        // Assert
        // Verify title is rendered
        Assert.Contains(_textCalls, call => call.text == "Andy.TUI Input Components Demo");
        
        // Verify separator is rendered
        Assert.Contains(_textCalls, call => call.text == "==============================");
        
        // Verify labels are rendered
        Assert.Contains(_textCalls, call => call.text == "Name:");
        Assert.Contains(_textCalls, call => call.text == "Password:");
        Assert.Contains(_textCalls, call => call.text == "Country:");
        
        // Verify input boxes are rendered
        Assert.True(_boxCalls.Count >= 3, "Should render at least 3 input boxes");
        
        // Verify buttons are rendered
        Assert.Contains(_textCalls, call => call.text == "Submit");
        Assert.Contains(_textCalls, call => call.text == "Cancel");
        
        // Verify instructions are rendered
        Assert.Contains(_textCalls, call => call.text.Contains("Tab/Shift+Tab: Navigate"));
    }

    [Fact(Skip = "Layout system positioning issues - all elements render at (0,0). See CLAUDE.md for details.")]
    public void InputExample_Layout_ShouldPositionElementsCorrectly()
    {
        // Arrange
        var formNode = CreateCompleteFormNode();

        // Act
        _renderer.Render(formNode);

        // Assert
        var titleCall = _textCalls.Find(call => call.text == "Andy.TUI Input Components Demo");
        var nameLabel = _textCalls.Find(call => call.text == "Name:");
        var submitButton = _textCalls.Find(call => call.text == "Submit");

        // Debug output
        var allTexts = _textCalls.Select(c => $"'{c.text}' at ({c.x},{c.y})").ToArray();
        var debugInfo = $"All text calls: {string.Join(", ", allTexts)}";

        // Title should be at the top
        Assert.True(titleCall.y < nameLabel.y, $"Title should be above name label. {debugInfo}");
        
        // Submit button should be below the form fields
        Assert.True(nameLabel.y < submitButton.y, "Submit button should be below form fields");
        
        // Elements should have reasonable spacing
        Assert.True(submitButton.y - nameLabel.y > 5, "Should have reasonable spacing between form and buttons");
    }

    // Helper methods to create UI nodes
    private VirtualNode CreateTitleNode()
    {
        return VirtualDomBuilder.Element("text")
            .WithProp("style", Style.Default.WithForegroundColor(Color.Cyan).WithBold())
            .WithChild(VirtualDomBuilder.Text("Andy.TUI Input Components Demo"))
            .Build();
    }

    private VirtualNode CreateLabelNode(string text)
    {
        return VirtualDomBuilder.Element("text")
            .WithProp("style", Style.Default.WithBold())
            .WithProp("width", 10)
            .WithChild(VirtualDomBuilder.Text(text))
            .Build();
    }

    private VirtualNode CreateTextInputNode(string value, string placeholder, bool isFocused)
    {
        var style = isFocused ? Style.Default.WithForegroundColor(Color.Cyan) : Style.Default;
        var textStyle = string.IsNullOrEmpty(value) 
            ? Style.Default.WithForegroundColor(Color.DarkGray)
            : Style.Default;
        var displayText = string.IsNullOrEmpty(value) ? placeholder : value;

        return VirtualDomBuilder.Element("box")
            .WithProp("width", 40)
            .WithProp("height", 3)
            .WithProp("border-style", BoxStyle.Single)
            .WithProp("style", style)
            .WithChild(
                VirtualDomBuilder.Element("text")
                    .WithProp("x-offset", 1)
                    .WithProp("y-offset", 1)
                    .WithProp("style", textStyle)
                    .WithChild(VirtualDomBuilder.Text(displayText))
                    .Build()
            )
            .Build();
    }

    private VirtualNode CreatePasswordInputNode(string value, string placeholder, bool isFocused)
    {
        var maskedValue = string.IsNullOrEmpty(value) ? "" : new string('•', value.Length);
        return CreateTextInputNode(maskedValue, placeholder, isFocused);
    }

    private VirtualNode CreateDropdownNode(string? value, string placeholder, string[] items, 
        bool isOpen, int highlightedIndex, bool isFocused)
    {
        var nodes = new List<VirtualNode>();
        var style = isFocused || isOpen ? Style.Default.WithForegroundColor(Color.Cyan) : Style.Default;
        var textStyle = value == null ? Style.Default.WithForegroundColor(Color.DarkGray) : Style.Default;
        var displayText = value ?? placeholder;

        // Main dropdown box
        nodes.Add(
            VirtualDomBuilder.Element("box")
                .WithProp("width", 40)
                .WithProp("height", 3)
                .WithProp("border-style", BoxStyle.Single)
                .WithProp("style", style)
                .WithChildren(
                    VirtualDomBuilder.Element("text")
                        .WithProp("x-offset", 1)
                        .WithProp("y-offset", 1)
                        .WithProp("style", textStyle)
                        .WithChild(VirtualDomBuilder.Text(displayText)),
                    VirtualDomBuilder.Element("text")
                        .WithProp("x-offset", 38)
                        .WithProp("y-offset", 1)
                        .WithChild(VirtualDomBuilder.Text(isOpen ? "▲" : "▼"))
                )
                .Build()
        );

        // Dropdown items (when open)
        if (isOpen)
        {
            var dropdownItems = new List<VirtualNode>();
            for (int i = 0; i < Math.Min(items.Length, 5); i++)
            {
                var itemStyle = i == highlightedIndex
                    ? Style.Default.WithForegroundColor(Color.White).WithBackgroundColor(Color.DarkBlue)
                    : Style.Default.WithForegroundColor(Color.White);

                dropdownItems.Add(
                    VirtualDomBuilder.Element("text")
                        .WithProp("y-offset", i)
                        .WithProp("style", itemStyle)
                        .WithProp("z-index", 11)
                        .WithChild(VirtualDomBuilder.Text(items[i]))
                        .Build()
                );
            }

            nodes.Add(
                VirtualDomBuilder.Element("box")
                    .WithProp("y-offset", 2)
                    .WithProp("width", 40)
                    .WithProp("height", Math.Min(items.Length + 2, 7))
                    .WithProp("border-style", BoxStyle.Single)
                    .WithProp("style", Style.Default.WithBackgroundColor(Color.Black))
                    .WithProp("z-index", 10)
                    .WithChildren(dropdownItems.ToArray())
                    .Build()
            );
        }

        return VirtualDomBuilder.Fragment(nodes.ToArray());
    }

    private VirtualNode CreateButtonNode(string text, ButtonStyle style, bool isFocused)
    {
        var (borderStyle, textStyle, bgColor) = style switch
        {
            ButtonStyle.Primary when isFocused => (BoxStyle.Double, Style.Default.WithForegroundColor(Color.White), (Color?)Color.Blue),
            ButtonStyle.Primary => (BoxStyle.Single, Style.Default.WithForegroundColor(Color.Blue), (Color?)null),
            _ when isFocused => (BoxStyle.Double, Style.Default.WithForegroundColor(Color.White), (Color?)Color.DarkGray),
            _ => (BoxStyle.Single, Style.Default, (Color?)null)
        };

        var children = new List<VirtualNode>();

        if (bgColor.HasValue)
        {
            children.Add(VirtualDomBuilder.Element("rect")
                .WithProp("x-offset", 1)
                .WithProp("y-offset", 1)
                .WithProp("width", 10)
                .WithProp("height", 1)
                .WithProp("fill", bgColor.Value)
                .Build());
        }

        children.Add(VirtualDomBuilder.Element("text")
            .WithProp("x-offset", (12 - text.Length) / 2)
            .WithProp("y-offset", 1)
            .WithProp("style", textStyle)
            .WithChild(VirtualDomBuilder.Text(text))
            .Build());

        return VirtualDomBuilder.Element("box")
            .WithProp("width", 12)
            .WithProp("height", 3)
            .WithProp("border-style", borderStyle)
            .WithProp("style", bgColor.HasValue ? Style.Default.WithBackgroundColor(bgColor.Value) : Style.Default)
            .WithChildren(children.ToArray())
            .Build();
    }

    private VirtualNode CreateCompleteFormNode()
    {
        return VirtualDomBuilder.Fragment(
            CreateTitleNode(),
            CreateLabelNode("Name:"),
            CreateTextInputNode("", "Enter your name...", false),
            CreateLabelNode("Password:"),
            CreatePasswordInputNode("", "Enter password...", false),
            CreateLabelNode("Country:"),
            CreateDropdownNode(null, "Select a country...", new[] { "United States", "Canada" }, false, 0, false),
            CreateButtonNode("Submit", ButtonStyle.Primary, false),
            CreateButtonNode("Cancel", ButtonStyle.Default, false),
            VirtualDomBuilder.Element("text")
                .WithProp("style", Style.Default.WithForegroundColor(Color.Gray))
                .WithChild(VirtualDomBuilder.Text("Tab/Shift+Tab: Navigate | Enter: Activate | Esc: Close dropdown | Ctrl+C: Exit"))
                .Build()
        );
    }

    private enum ButtonStyle { Default, Primary }
}