using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Bindings;
using System;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Example demonstrating TabView with z-index management and tab switching.
/// </summary>
public static class TabViewExample
{
    public static ISimpleComponent Create()
    {
        var selectedTab = new Binding<int>(0);
        var taskCount = new Binding<int>(3);
        
        return VStack
        {
            Text("TabView Z-Index Example").Bold(),
            Spacer(height: 1),
            
            // Info panel
            Box
            {
                HStack
                {
                    Text($"Selected Tab: {selectedTab.Value}"),
                    Text(" | "),
                    Text($"Tasks: {taskCount.Value}")
                }
            },
            
            Spacer(height: 1),
            
            // Main TabView
            new TabView(
                selectedIndex: selectedTab.Value,
                onTabSelected: idx => selectedTab.Value = idx
            )
            {
                new Tab("Dashboard", CreateDashboardContent()),
                new Tab("Tasks", CreateTasksContent(taskCount)),
                new Tab("Settings", CreateSettingsContent()),
                new Tab("About", CreateAboutContent())
            },
            
            Spacer(height: 1),
            
            // Instructions
            Text("Use ← → arrows to switch tabs, TAB to navigate").Dim()
        };
    }
    
    private static ISimpleComponent CreateDashboardContent()
    {
        return Box
        {
            VStack
            {
                Text("Dashboard").Bold(),
                Spacer(height: 1),
                HStack
                {
                    Box
                    {
                        VStack
                        {
                            Text("Active Users"),
                            Text("1,234").Color(Color.Green)
                        }
                    }.Width(15),
                    
                    Spacer(width: 2),
                    
                    Box
                    {
                        VStack
                        {
                            Text("Revenue"),
                            Text("$12,345").Color(Color.Cyan)
                        }
                    }.Width(15)
                },
                Spacer(height: 1),
                Text("Recent Activity:"),
                Text("• User login at 10:45 AM").Dim(),
                Text("• New order #1234").Dim(),
                Text("• System update completed").Dim()
            }
        }.Padding(1);
    }
    
    private static ISimpleComponent CreateTasksContent(Binding<int> taskCount)
    {
        return Box
        {
            VStack
            {
                Text("Task Management").Bold(),
                Spacer(height: 1),
                
                // Task list
                VStack
                {
                    TaskItem("Implement z-index architecture", true),
                    TaskItem("Create TabView component", true),
                    TaskItem("Add spatial index support", false),
                    TaskItem("Write performance tests", false),
                    TaskItem("Update documentation", false)
                },
                
                Spacer(height: 1),
                
                Button("Add Task", () => taskCount.Value++)
            }
        }.Padding(1);
    }
    
    private static ISimpleComponent TaskItem(string text, bool completed)
    {
        return HStack
        {
            Text(completed ? "[✓]" : "[ ]").Color(completed ? Color.Green : Color.Gray),
            Text(" "),
            Text(text).Strikethrough(completed)
        };
    }
    
    private static ISimpleComponent CreateSettingsContent()
    {
        var theme = new Binding<string>("Dark");
        var notifications = new Binding<bool>(true);
        
        return Box
        {
            VStack
            {
                Text("Settings").Bold(),
                Spacer(height: 1),
                
                // Theme selection
                Text("Theme:"),
                RadioGroup(
                    options: new[] { "Light", "Dark", "Auto" },
                    selectedOption: theme
                ),
                
                Spacer(height: 1),
                
                // Notifications
                Checkbox(
                    label: "Enable notifications",
                    isChecked: notifications
                ),
                
                Spacer(height: 1),
                
                Button("Save Settings", () => { /* Save */ })
            }
        }.Padding(1);
    }
    
    private static ISimpleComponent CreateAboutContent()
    {
        return Box
        {
            VStack
            {
                Text("About").Bold(),
                Spacer(height: 1),
                Text("Andy.TUI TabView Example"),
                Text("Version 1.0.0"),
                Spacer(height: 1),
                Text("This example demonstrates:").Dim(),
                Text("• Tab switching with z-index management").Dim(),
                Text("• Nested components with proper layering").Dim(),
                Text("• Keyboard navigation support").Dim(),
                Spacer(height: 1),
                Text("Z-index ensures selected tabs appear above others").Color(Color.Yellow)
            }
        }.Padding(1);
    }
}