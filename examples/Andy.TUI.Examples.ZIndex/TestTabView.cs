using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Terminal;
using System;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Test TabView rendering.
/// </summary>
public static class TestTabView
{
    public static ISimpleComponent Create()
    {
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));
        
        // Create a box to contain everything
        return new Box {
            new VStack(spacing: 1) {
                new Text("TabView Test"),
                new Text("This should show tab headers above content"),
                new Box {
                    tabView
                }
            }
        };
    }
}