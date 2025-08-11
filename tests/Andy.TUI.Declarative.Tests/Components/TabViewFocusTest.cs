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
using Andy.TUI.Declarative.Focus;

namespace Andy.TUI.Declarative.Tests.Components;

public class TabViewFocusTest
{
    [Fact]
    public void TabView_ShouldReceiveFocus_InFocusManager()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));

        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var focusManager = context.FocusManager;

        // Create a container to hold the TabView
        var container = new VStack {
            new Text("Header"),
            tabView
        };

        var rootInstance = manager.GetOrCreateInstance(container, "root");

        // Calculate layout
        var constraints = LayoutConstraints.Loose(80, 24);
        rootInstance.CalculateLayout(constraints);

        // Act - The TabView should have been registered during instance creation
        // Move focus to the first focusable element
        focusManager.MoveFocus(FocusDirection.Next);

        // Assert
        var focusedComponent = focusManager.FocusedComponent;
        Assert.NotNull(focusedComponent);

        // The focused component should be the TabView
        var tabViewInstance = focusedComponent as TabViewInstance;
        Assert.NotNull(tabViewInstance);
        Assert.True(tabViewInstance.CanReceiveFocus);
        Assert.True(tabViewInstance.IsFocused);

        // Test keyboard navigation
        var handled = tabViewInstance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.RightArrow, false, false, false));
        Assert.True(handled);
    }
}