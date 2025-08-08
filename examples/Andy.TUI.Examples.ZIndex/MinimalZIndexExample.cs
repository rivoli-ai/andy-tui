using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Terminal;
using System;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Minimal example demonstrating Z-index functionality.
/// </summary>
public static class MinimalZIndexExample
{
    public static ISimpleComponent Create()
    {
        // For now, just create a simple TabView with basic content
        var tabView = new TabView(selectedIndex: 0);
        
        // Add simple tabs
        tabView.Add("Tab 1", new Text("Content for Tab 1"));
        tabView.Add("Tab 2", new Text("Content for Tab 2"));
        tabView.Add("Tab 3", new Text("Content for Tab 3"));
        
        return new VStack(spacing: 1) {
            new Text("Minimal Z-Index Example").Bold(),
            new Text("Use arrow keys to switch tabs"),
            new Text("Press Ctrl+C to exit"),
            new Spacer(1),
            tabView
        };
    }
}