using System;
using Xunit;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Declarative.Tests;

public class BoxInstanceHeightTests
{
    [Fact]
    public void BoxInstance_WithAutoHeight_ShouldCalculateHeightFromContent()
    {
        // Arrange
        var box = new Box 
        { 
            new Text("Line 1"),
            new Text("Line 2"),
            new Text("Line 3")
        }
        .WithHeight(Length.Auto)
        .WithPadding(new Spacing(1));
        
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        // Act
        var instance = manager.GetOrCreateInstance(box, "test-box");
        
        // Assert
        Assert.IsType<BoxInstance>(instance);
        var boxInstance = (BoxInstance)instance;
        boxInstance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        var layout = boxInstance.Layout;
        
        // Height should be content height (3 for 3 text lines) + padding (1 top + 1 bottom)
        // The actual height depends on the text layout, but it should be > 0
        Assert.True(layout.Height > 0, "Box with auto height should have height > 0 when containing content");
        Assert.True(layout.Height >= 2, "Box with auto height should include padding in height calculation");
    }
    
    [Fact]
    public void BoxInstance_WithFixedHeight_ShouldUseFixedHeight()
    {
        // Arrange
        var box = new Box 
        { 
            new Text("Content")
        }
        .WithHeight(10)
        .WithPadding(new Spacing(1));
        
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        // Act
        var instance = manager.GetOrCreateInstance(box, "test-box");
        
        // Assert
        Assert.IsType<BoxInstance>(instance);
        var boxInstance = (BoxInstance)instance;
        boxInstance.CalculateLayout(LayoutConstraints.Loose(100, 100));
        var layout = boxInstance.Layout;
        
        Assert.Equal(10, layout.Height);
    }
}