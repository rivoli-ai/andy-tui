using System;
using System.Linq;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Layout;

namespace Andy.TUI.Declarative.Tests.Components;

public class TabViewOverlapTest
{
    [Fact]
    public void TabView_ShouldNotHaveOverlappingText()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content for Tab 1"));
        tabView.Add("Tab 2", new Text("Content for Tab 2"));
        tabView.Add("Tab 3", new Text("Content for Tab 3"));

        var container = new VStack(spacing: 1) {
            new Text("Example"),
            new Text("Use arrow keys to switch tabs"),
            new Text("Press Ctrl+C to exit"),
            tabView
        };

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var rootInstance = manager.GetOrCreateInstance(container, "root");

        // Calculate layout
        var constraints = LayoutConstraints.Loose(80, 24);
        rootInstance.CalculateLayout(constraints);

        // Set absolute positions
        rootInstance.Layout.AbsoluteX = 0;
        rootInstance.Layout.AbsoluteY = 0;
        rootInstance.UpdateAbsoluteZIndex(0);

        // Render
        var rendered = rootInstance.Render();

        // Extract all text elements with positions
        var textElements = ExtractTextElements(rendered);

        // Check for overlaps
        foreach (var elem1 in textElements)
        {
            foreach (var elem2 in textElements)
            {
                if (elem1 != elem2 && elem1.Y == elem2.Y)
                {
                    // Check if text overlaps on same line
                    var elem1End = elem1.X + elem1.Content.Length;
                    var elem2End = elem2.X + elem2.Content.Length;

                    bool overlaps = (elem1.X >= elem2.X && elem1.X < elem2End) ||
                                   (elem2.X >= elem1.X && elem2.X < elem1End);

                    Assert.False(overlaps,
                        $"Text overlap detected: '{elem1.Content}' at ({elem1.X},{elem1.Y}) " +
                        $"overlaps with '{elem2.Content}' at ({elem2.X},{elem2.Y})");
                }
            }
        }

        // Verify tab headers and content are on different lines
        var tabHeaders = textElements.Where(t => t.Content.StartsWith("Tab ")).ToList();
        var tabContent = textElements.Where(t => t.Content.StartsWith("Content for")).ToList();

        if (tabHeaders.Any() && tabContent.Any())
        {
            var headerY = tabHeaders.First().Y;
            var contentY = tabContent.First().Y;
            Assert.NotEqual(headerY, contentY);
            Assert.True(contentY > headerY, "Tab content should be below headers");
        }
    }

    private class TextElement
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Content { get; set; } = "";
        public int ZIndex { get; set; }
    }

    private List<TextElement> ExtractTextElements(VirtualNode node, List<TextElement>? elements = null)
    {
        elements ??= new List<TextElement>();

        if (node is ElementNode elem && elem.TagName == "text")
        {
            var x = elem.Props.TryGetValue("x", out var xVal) && xVal is int xi ? xi : 0;
            var y = elem.Props.TryGetValue("y", out var yVal) && yVal is int yi ? yi : 0;
            var z = elem.Props.TryGetValue("z-index", out var zVal) && zVal is int zi ? zi : 0;

            var textContent = "";
            foreach (var child in elem.Children)
            {
                if (child is TextNode text)
                {
                    textContent += text.Content;
                }
            }

            if (!string.IsNullOrEmpty(textContent))
            {
                elements.Add(new TextElement { X = x, Y = y, Content = textContent, ZIndex = z });
            }
        }

        foreach (var child in node.Children)
        {
            ExtractTextElements(child, elements);
        }

        return elements;
    }
}