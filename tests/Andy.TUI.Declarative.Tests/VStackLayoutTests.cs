using System;
using Xunit;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

namespace Andy.TUI.Declarative.Tests;

public class VStackLayoutTests
{
    [Fact]
    public void VStackInstance_ShouldPropagateAbsolutePositions()
    {
        // Arrange
        var vstack = new VStack(spacing: 2) 
        { 
            new Text("Line 1"),
            new Box { new Text("Box content") }.WithHeight(3).WithPadding(1),
            new Text("Line 3")
        };
        
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        // Act
        var instance = manager.GetOrCreateInstance(vstack, "test-vstack");
        instance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        
        // Simulate what DeclarativeRenderer does
        instance.Layout.AbsoluteX = 0;
        instance.Layout.AbsoluteY = 0;
        
        // Render to trigger absolute position calculation
        var rendered = instance.Render();
        
        // Assert
        Assert.IsType<VStackInstance>(instance);
        var vstackInstance = (VStackInstance)instance;
        var children = vstackInstance.GetChildInstances();
        
        Assert.Equal(3, children.Count);
        
        // First child (Text) should be at position 0,0
        Assert.Equal(0, children[0].Layout.AbsoluteX);
        Assert.Equal(0, children[0].Layout.AbsoluteY);
        
        // Second child (Box) should be at position 0,3 (line 1 height + spacing)
        Assert.Equal(0, children[1].Layout.AbsoluteX);
        Assert.Equal(3, children[1].Layout.AbsoluteY); // 1 (text height) + 2 (spacing)
        
        // Third child should be further down
        Assert.Equal(0, children[2].Layout.AbsoluteX);
        Assert.True(children[2].Layout.AbsoluteY > children[1].Layout.AbsoluteY);
    }
}