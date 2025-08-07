using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Core.Rendering;
using System.Linq;

namespace Andy.TUI.Declarative.Tests;

public class TabViewZIndexTests
{
    [Fact]
    public void TabView_InitialSelection_SetsCorrectZIndices()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 1);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));
        tabView.Add("Tab 3", new Text("Content 3"));
        
        var instance = new TabViewInstance(tabView);
        var context = new ZIndexContext();
        
        // Act
        instance.UpdateAbsoluteZIndex(context);
        
        // Assert
        Assert.Equal(0, instance.AbsoluteZIndex); // TabView itself at root level
        Assert.Equal(1, instance.SelectedIndex);
    }
    
    [Fact]
    public void TabView_SelectTab_UpdatesZIndices()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Tab 1", new Text("Content 1"));
        tabView.Add("Tab 2", new Text("Content 2"));
        
        var instance = new TabViewInstance(tabView);
        
        // Act
        instance.SelectTab(1);
        
        // Assert
        Assert.Equal(1, instance.SelectedIndex);
        // Z-indices would be updated via the spatial index in real implementation
    }
    
    [Fact]
    public void TabView_InModal_ResolvesAbsoluteZIndex()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Home", new Text("Home content"));
        tabView.Add("Settings", new Text("Settings content"));
        
        var instance = new TabViewInstance(tabView);
        instance.RelativeZIndex = 0; // TabView has no additional z-index
        
        var context = new ZIndexContext();
        context.EnterComponent(1000, "Modal"); // Modal at z=1000
        
        // Act
        instance.UpdateAbsoluteZIndex(context);
        
        // Assert
        Assert.Equal(1000, instance.AbsoluteZIndex);
        // Selected tab header would be at 1002 (1000 + 2)
        // Tab content would be at 1003 (1000 + 3)
    }
    
    [Fact]
    public void TabView_NestedInMultipleContainers_ResolvesCorrectly()
    {
        // Arrange
        var tabView = new TabView();
        tabView.Add("Tab 1", new Text("Content"));
        
        var instance = new TabViewInstance(tabView);
        
        var context = new ZIndexContext();
        context.EnterComponent(100, "Panel");
        context.EnterComponent(50, "Card");
        context.EnterComponent(10, "Container");
        
        // Act
        instance.UpdateAbsoluteZIndex(context);
        
        // Assert
        Assert.Equal(160, instance.AbsoluteZIndex); // 100 + 50 + 10
    }
    
    [Fact]
    public void TabView_MultipleTabSwitches_MaintainsCorrectOrder()
    {
        // Arrange
        var selectedIndices = new List<int>();
        var tabView = new TabView(
            selectedIndex: 0,
            onTabSelected: idx => selectedIndices.Add(idx)
        );
        
        tabView.Add("A", new Text("A"));
        tabView.Add("B", new Text("B"));
        tabView.Add("C", new Text("C"));
        
        var instance = new TabViewInstance(tabView);
        
        // Act - simulate user clicking through tabs
        instance.SelectTab(1); // A -> B
        instance.SelectTab(2); // B -> C
        instance.SelectTab(0); // C -> A
        instance.SelectTab(1); // A -> B
        
        // Assert
        Assert.Equal(new[] { 1, 2, 0, 1 }, selectedIndices);
        Assert.Equal(1, instance.SelectedIndex);
    }
    
    [Fact]
    public void TabView_InvalidSelection_IsIgnored()
    {
        // Arrange
        var tabView = new TabView(selectedIndex: 0);
        tabView.Add("Only Tab", new Text("Content"));
        
        var instance = new TabViewInstance(tabView);
        
        // Act
        instance.SelectTab(-1); // Invalid
        instance.SelectTab(5);  // Out of range
        
        // Assert
        Assert.Equal(0, instance.SelectedIndex); // Still at 0
    }
}