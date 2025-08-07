using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Bindings;
using System;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Example demonstrating modal dialogs with proper z-index layering.
/// </summary>
public static class ModalExample
{
    public static ISimpleComponent Create()
    {
        var showModal = new Binding<bool>(false);
        var showNestedModal = new Binding<bool>(false);
        var selectedTab = new Binding<int>(0);
        
        return ZStack
        {
            // Background content (z=0)
            VStack
            {
                Text("Modal Z-Index Example").Bold(),
                Spacer(height: 1),
                
                // Background panel with some content
                Box
                {
                    VStack
                    {
                        Text("Background Content (z=0)"),
                        Spacer(height: 1),
                        Text("This content is behind the modal"),
                        Button("Show Modal", () => showModal.Value = true)
                    }
                }.Padding(1).Width(40).Height(10),
                
                Spacer(height: 1),
                Text("Press ESC to close modals").Dim()
            },
            
            // Modal overlay (z=1000)
            If(showModal.Value, () => 
                Modal(
                    onClose: () => showModal.Value = false,
                    zIndex: 1000,
                    content: VStack
                    {
                        Text("Modal Dialog (z=1000)").Bold(),
                        Spacer(height: 1),
                        
                        // TabView inside modal
                        Text("TabView inside Modal:"),
                        new TabView(
                            selectedIndex: selectedTab.Value,
                            onTabSelected: idx => selectedTab.Value = idx
                        )
                        {
                            new Tab("Info", Text("Tab content at z=1002")),
                            new Tab("Options", VStack
                            {
                                Text("Nested content"),
                                Button("Show Nested Modal", 
                                    () => showNestedModal.Value = true)
                            })
                        },
                        
                        Spacer(height: 1),
                        Button("Close", () => showModal.Value = false)
                    }
                )
            ),
            
            // Nested modal (z=1100)
            If(showNestedModal.Value, () =>
                Modal(
                    onClose: () => showNestedModal.Value = false,
                    zIndex: 1100,
                    content: VStack
                    {
                        Text("Nested Modal (z=1100)").Bold(),
                        Text("This appears above the parent modal"),
                        Spacer(height: 1),
                        Button("Close", () => showNestedModal.Value = false)
                    }
                )
            )
        };
    }
    
    private static ISimpleComponent Modal(
        Action onClose, 
        int zIndex, 
        ISimpleComponent content)
    {
        return ZStack
        {
            // Semi-transparent overlay
            Box
            {
                Spacer()
            }.Fill().BackgroundColor(Color.Black).Opacity(0.5),
            
            // Modal content
            Center
            {
                Box
                {
                    content
                }.Padding(2).BackgroundColor(Color.DarkGray)
            }
        }.ZIndex(zIndex).OnKeyPress(key =>
        {
            if (key.Key == ConsoleKey.Escape)
            {
                onClose();
                return true;
            }
            return false;
        });
    }
}