using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Complex example demonstrating multiple layers of z-indexed components.
/// </summary>
public static class ComplexLayeredExample
{
    public static ISimpleComponent Create()
    {
        var notifications = new Binding<List<string>>(new List<string>());
        var activePanel = new Binding<string>("dashboard");
        var showTooltip = new Binding<bool>(false);
        var tooltipPosition = new Binding<(int x, int y)>((20, 10));
        
        return ZStack
        {
            // Layer 0: Background grid
            BackgroundGrid(),
            
            // Layer 10: Main content panels
            MainContent(activePanel),
            
            // Layer 50: Floating panels
            FloatingPanels(activePanel),
            
            // Layer 100: Notifications
            NotificationStack(notifications),
            
            // Layer 200: Tooltip (highest)
            If(showTooltip.Value, () =>
                Tooltip(
                    "This tooltip appears above everything else",
                    tooltipPosition.Value.x,
                    tooltipPosition.Value.y
                )
            ),
            
            // Control panel (always on top)
            ControlPanel(activePanel, notifications, showTooltip)
        };
    }
    
    private static ISimpleComponent BackgroundGrid()
    {
        return Box
        {
            VStack
            {
                Text("Background Layer (z=0)").Dim(),
                Spacer(height: 1),
                
                // Grid pattern
                For(Enumerable.Range(0, 10), row =>
                    HStack
                    {
                        For(Enumerable.Range(0, 10), col =>
                            Text("·").Dim()
                        )
                    }
                )
            }
        }.ZIndex(0);
    }
    
    private static ISimpleComponent MainContent(Binding<string> activePanel)
    {
        return HStack
        {
            // Dashboard panel
            PanelContent(
                "dashboard",
                "Dashboard",
                activePanel,
                zIndex: activePanel.Value == "dashboard" ? 15 : 10
            ),
            
            Spacer(width: 2),
            
            // Analytics panel
            PanelContent(
                "analytics",
                "Analytics",
                activePanel,
                zIndex: activePanel.Value == "analytics" ? 15 : 10
            ),
            
            Spacer(width: 2),
            
            // Settings panel
            PanelContent(
                "settings",
                "Settings",
                activePanel,
                zIndex: activePanel.Value == "settings" ? 15 : 10
            )
        }.Padding(2);
    }
    
    private static ISimpleComponent PanelContent(
        string id,
        string title,
        Binding<string> activePanel,
        int zIndex)
    {
        var isActive = activePanel.Value == id;
        
        return Box
        {
            VStack
            {
                Text(title).Bold(),
                Spacer(height: 1),
                Text($"Z-Index: {zIndex}"),
                Text(isActive ? "Active" : "Inactive")
                    .Color(isActive ? Color.Green : Color.Gray),
                Spacer(height: 1),
                Button($"Activate", () => activePanel.Value = id)
            }
        }
        .Width(20)
        .Height(10)
        .BorderStyle(isActive ? BoxStyle.Double : BoxStyle.Single)
        .ZIndex(zIndex);
    }
    
    private static ISimpleComponent FloatingPanels(Binding<string> activePanel)
    {
        return Fragment
        {
            // Floating info panel
            Box
            {
                VStack
                {
                    Text("Floating Panel").Bold(),
                    Text("Z-Index: 50"),
                    Spacer(height: 1),
                    Text($"Active: {activePanel.Value}")
                }
            }
            .Position(40, 5)
            .Width(25)
            .Height(8)
            .ZIndex(50),
            
            // Another floating panel
            Box
            {
                VStack
                {
                    Text("Status Monitor"),
                    Text("Z-Index: 55"),
                    Spacer(height: 1),
                    Text($"Time: {DateTime.Now:HH:mm:ss}")
                }
            }
            .Position(45, 12)
            .Width(20)
            .Height(6)
            .ZIndex(55)
        };
    }
    
    private static ISimpleComponent NotificationStack(Binding<List<string>> notifications)
    {
        return VStack
        {
            For(notifications.Value.TakeLast(3), (notif, index) =>
                Box
                {
                    Text(notif).Color(Color.Yellow)
                }
                .Width(30)
                .Padding(1)
                .BackgroundColor(Color.DarkBlue)
                .ZIndex(100 + index) // Stack notifications
            )
        }.Position(50, 2);
    }
    
    private static ISimpleComponent Tooltip(string text, int x, int y)
    {
        return Box
        {
            Text(text).Color(Color.Black)
        }
        .Position(x, y)
        .Padding(1)
        .BackgroundColor(Color.Yellow)
        .ZIndex(200); // Highest z-index
    }
    
    private static ISimpleComponent ControlPanel(
        Binding<string> activePanel,
        Binding<List<string>> notifications,
        Binding<bool> showTooltip)
    {
        return Box
        {
            VStack
            {
                Text("Controls").Bold(),
                Spacer(height: 1),
                
                Button("Add Notification", () =>
                {
                    var list = new List<string>(notifications.Value);
                    list.Add($"Notification {list.Count + 1}");
                    notifications.Value = list;
                }),
                
                Button(showTooltip.Value ? "Hide Tooltip" : "Show Tooltip",
                    () => showTooltip.Value = !showTooltip.Value),
                
                Spacer(height: 1),
                Text("Z-Index Legend:").Underline(),
                Text("0: Background").Dim(),
                Text("10-15: Main panels"),
                Text("50-55: Floating panels"),
                Text("100+: Notifications"),
                Text("200: Tooltip").Color(Color.Yellow)
            }
        }
        .Position(2, 15)
        .Width(25)
        .Height(15)
        .ZIndex(150); // Control panel above most things
    }
}