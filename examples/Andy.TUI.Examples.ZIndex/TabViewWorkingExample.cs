using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Terminal;
using System;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Working TabView example with proper navigation support.
/// </summary>
public static class TabViewWorkingExample
{
    public static ISimpleComponent Create()
    {
        var selectedIndex = 0;
        var tabView = new TabView(
            selectedIndex: selectedIndex,
            onTabSelected: idx => {
                selectedIndex = idx;
                Console.WriteLine($"Tab selected: {idx}");
            }
        );
        
        // Add tabs
        tabView.Add("Home", CreateHomeTab());
        tabView.Add("Settings", CreateSettingsTab());
        tabView.Add("About", CreateAboutTab());
        
        return new VStack(spacing: 1) {
            new Text("TabView Example with Navigation").Bold(),
            new Text("Use ← → arrow keys to switch tabs"),
            new Text("Press Tab to move focus between components"),
            new Text("Press Ctrl+C to exit"),
            new Spacer(1),
            tabView
        };
    }
    
    private static ISimpleComponent CreateHomeTab()
    {
        return new VStack(spacing: 1) {
            new Text("Welcome to the Home Tab!").Bold(),
            new Spacer(1),
            new Text("This is the home tab content."),
            new Text("You can navigate between tabs using arrow keys.")
        };
    }
    
    private static ISimpleComponent CreateSettingsTab()
    {
        return new VStack(spacing: 1) {
            new Text("Settings").Bold(),
            new Spacer(1),
            new Text("Configure your preferences here:"),
            new Text("• Theme: Dark"),
            new Text("• Language: English"),
            new Text("• Notifications: Enabled")
        };
    }
    
    private static ISimpleComponent CreateAboutTab()
    {
        return new VStack(spacing: 1) {
            new Text("About").Bold(),
            new Spacer(1),
            new Text("TabView Z-Index Demo"),
            new Text("Version 1.0.0"),
            new Spacer(1),
            new Text("This example demonstrates:"),
            new Text("• Tab selection with proper z-ordering"),
            new Text("• Keyboard navigation between tabs"),
            new Text("• Focus management")
        };
    }
}