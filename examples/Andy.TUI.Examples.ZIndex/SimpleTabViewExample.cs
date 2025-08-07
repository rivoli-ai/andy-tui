using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Terminal;
using System;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Simple example demonstrating TabView functionality.
/// </summary>
public static class SimpleTabViewExample
{
    public static ISimpleComponent Create()
    {
        var tabView = new TabView(selectedIndex: 0);
        
        // Add tabs
        tabView.Add("Home", CreateHomeTab());
        tabView.Add("Settings", CreateSettingsTab());
        tabView.Add("About", CreateAboutTab());
        
        return new Box {
            tabView
        };
    }
    
    private static ISimpleComponent CreateHomeTab()
    {
        return new Box {
            new Text("Welcome to the Home Tab!")
        };
    }
    
    private static ISimpleComponent CreateSettingsTab()
    {
        return new Box {
            new Text("Settings Tab Content")
        };
    }
    
    private static ISimpleComponent CreateAboutTab()
    {
        return new Box {
            new Text("About Tab - Z-Index Demo")
        };
    }
}