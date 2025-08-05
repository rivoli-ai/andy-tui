using System;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;

namespace Andy.TUI.Declarative.Tests;

public class TextWrapIssueRepro
{
    private readonly ITestOutputHelper _output;
    
    public TextWrapIssueRepro(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void BoxContent_ShouldNotAppearOnSameLineAsHeader()
    {
        // This reproduces the exact issue from the text wrapping example
        var longText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
        
        var ui = new VStack(spacing: 2) {
            new Text("Text Wrapping and Truncation Demo").Bold().Color(Color.Cyan),
            
            // No wrap (default)
            new Text("1. No Wrap (default):").Bold(),
            new Box { 
                new Text(longText).Color(Color.Green) 
            }
            .WithWidth(40)
            .WithPadding(1)
            // Note: Height is auto by default
        };
        
        // Let's trace through what should happen
        var context = new DeclarativeContext(() => { });
        var rootInstance = context.ViewInstanceManager.GetOrCreateInstance(ui, "root");
        
        // Calculate layout
        rootInstance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        
        // Set root absolute position like DeclarativeRenderer does
        rootInstance.Layout.AbsoluteX = 0;
        rootInstance.Layout.AbsoluteY = 0;
        
        // Render to trigger absolute position calculations
        rootInstance.Render();
        
        // Check the layout calculations
        Assert.IsType<VStackInstance>(rootInstance);
        var vstack = (VStackInstance)rootInstance;
        var children = vstack.GetChildInstances();
        
        _output.WriteLine($"VStack children count: {children.Count}");
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            _output.WriteLine($"Child {i}: Type={child.GetType().Name}, Y={child.Layout.Y}, Height={child.Layout.Height}");
            
            if (child is BoxInstance box)
            {
                var boxChildren = box.GetChildInstances();
                _output.WriteLine($"  Box has {boxChildren.Count} children");
                foreach (var boxChild in boxChildren)
                {
                    _output.WriteLine($"    BoxChild: Type={boxChild.GetType().Name}, Y={boxChild.Layout.Y}, Height={boxChild.Layout.Height}");
                    
                    // Re-calculate child to see what constraints it received
                    var testConstraints = LayoutConstraints.Loose(40, 10);
                    boxChild.CalculateLayout(testConstraints);
                    _output.WriteLine($"    After recalc with loose(40,10): Height={boxChild.Layout.Height}");
                }
            }
        }
        
        // The issue: Box height might be 0 or text might be positioned incorrectly
        var boxInstance = children[2] as BoxInstance;
        Assert.NotNull(boxInstance);
        
        // Log final layout after auto adjustment
        _output.WriteLine($"After layout calculation:");
        _output.WriteLine($"Box final height: {boxInstance.Layout.Height}");
        _output.WriteLine($"Box AbsoluteY: {boxInstance.Layout.AbsoluteY}");
        
        // Box should have proper height
        Assert.True(boxInstance.Layout.Height > 0 && !float.IsInfinity(boxInstance.Layout.Height), 
            $"Box height is {boxInstance.Layout.Height}, should be > 0 and not infinity");
        
        // Box should be positioned below the header with spacing
        Assert.Equal(6, boxInstance.Layout.Y); // Header Y=3 + header height 1 + spacing 2
    }
}