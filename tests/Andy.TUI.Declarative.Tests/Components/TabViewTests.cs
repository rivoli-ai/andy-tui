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

public class TabViewTests
{
    [Fact]
    public void TabView_ShouldRenderHeaders()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));
        tabView.Add("Tab 3", new Text("Content 3"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview");

        // Act
        // First need to calculate layout
        var constraints = LayoutConstraints.Loose(80, 24);
        instance.CalculateLayout(constraints);

        var rendered = instance.Render();

        // Assert
        Assert.NotNull(rendered);

        // Convert to string to check content
        var output = VirtualNodeToString(rendered);

        // Debug output
        if (string.IsNullOrEmpty(output))
        {
            // Check if instance has layout
            Assert.True(instance.Layout.Width > 0, "Layout width should be > 0");
            Assert.True(instance.Layout.Height > 0, "Layout height should be > 0");
        }

        // Should contain tab headers
        Assert.Contains("Tab 1", output);
        Assert.Contains("Tab 2", output);
        Assert.Contains("Tab 3", output);

        // Should contain selected content
        Assert.Contains("Content 1", output);

        // Should not contain non-selected content
        Assert.DoesNotContain("Content 2", output);
        Assert.DoesNotContain("Content 3", output);
    }

    [Fact]
    public void TabView_ShouldSwitchTabsOnArrowKeys()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));
        tabView.Add("Tab 3", new Text("Content 3"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview") as TabViewInstance;
        Assert.NotNull(instance);

        // Focus the TabView
        instance.OnGotFocus();

        // Calculate layout
        var constraints = LayoutConstraints.Loose(80, 24);
        instance.CalculateLayout(constraints);

        // Act - Press right arrow
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));

        // Assert
        Assert.True(handled);

        // Re-render after key press
        var rendered = instance.Render();
        var output = VirtualNodeToString(rendered);

        // Should now show second tab content
        Assert.Contains("Content 2", output);
        Assert.DoesNotContain("Content 1", output);
        Assert.DoesNotContain("Content 3", output);
    }

    [Fact]
    public void TabView_ShouldWrapAroundAtEnds()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 2); // Start at last tab
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));
        tabView.Add("Tab 3", new Text("Content 3"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview") as TabViewInstance;
        Assert.NotNull(instance);

        // Focus the TabView
        instance.OnGotFocus();

        // Calculate layout
        var constraints = LayoutConstraints.Loose(80, 24);
        instance.CalculateLayout(constraints);

        // Act - Press right arrow (should wrap to first tab)
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));

        // Assert
        Assert.True(handled);

        var rendered = instance.Render();
        var output = VirtualNodeToString(rendered);

        // Should wrap to first tab
        Assert.Contains("Content 1", output);
        Assert.DoesNotContain("Content 2", output);
        Assert.DoesNotContain("Content 3", output);
    }

    [Fact]
    public void TabView_ShouldHaveCorrectFocusProperties()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview") as TabViewInstance;
        Assert.NotNull(instance);

        // Assert
        Assert.True(instance.CanReceiveFocus);
        Assert.True(instance.CanFocus);
        Assert.False(instance.IsFocused);

        // Act
        instance.OnGotFocus();

        // Assert
        Assert.True(instance.IsFocused);
    }

    [Fact]
    public void TabView_ShouldNotReceiveFocusWhenEmpty()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview") as TabViewInstance;
        Assert.NotNull(instance);

        // Assert
        Assert.False(instance.CanReceiveFocus);
        Assert.False(instance.CanFocus);
    }

    [Fact]
    public void TabView_ShouldHandleTabSelection()
    {
        // Arrange
        var selectedIndex = -1;
        var tabView = new TabView(selectedIndex: 0, onTabSelected: idx => selectedIndex = idx);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var instance = manager.GetOrCreateInstance(tabView, "test-tabview") as TabViewInstance;
        Assert.NotNull(instance);

        instance.OnGotFocus();

        // Act
        instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));

        // Assert
        Assert.Equal(1, selectedIndex);
    }

    private string VirtualNodeToString(VirtualNode node)
    {
        if (node is TextNode text)
            return text.Content;

        if (node is ElementNode element)
        {
            var result = "";
            foreach (var child in element.Children)
            {
                result += VirtualNodeToString(child);
            }
            return result;
        }

        if (node is FragmentNode fragment)
        {
            var result = "";
            foreach (var child in fragment.Children)
            {
                result += VirtualNodeToString(child);
            }
            return result;
        }

        return "";
    }
}