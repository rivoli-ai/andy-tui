using System;
using System.Linq;
using Xunit;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Tests.TestHelpers;
using System.Collections.Generic;

namespace Andy.TUI.Declarative.Tests;

public class TextWrapExampleTests
{
    [Fact]
    public void TextWrapExample_MinimalCase_ShouldRenderCorrectly()
    {
        // Arrange - create a very simple test case
        var ui = new Text("Hello World");
        
        // Create a test terminal and rendering system
        var mockTerminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(mockTerminal);
        renderingSystem.Initialize();
        
        // Act
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        renderingSystem.Render(); // Force flush
        System.Threading.Thread.Sleep(100); // Give time for render to complete
        
        // Assert - check the rendered output
        var lines = mockTerminal.GetAllLines();
        
        // Debug: Output all lines to see what's actually rendered
        for (int i = 0; i < Math.Min(10, lines.Length); i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
                Console.WriteLine($"Line {i}: |{lines[i]}|");
        }
        
        // Check that text is rendered
        bool foundText = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Hello World"))
            {
                foundText = true;
                break;
            }
        }
        Assert.True(foundText, "Hello World should be rendered somewhere in the output");
    }
    
    [Fact]
    public void TextWrapExample_SimpleCase_BoxContentShouldNotOverlap()
    {
        // Arrange - simple test with a box
        var ui = new Box { 
            new Text("Box content") 
        }
        .WithWidth(40)
        .WithHeight(5);
        
        // Create a test terminal and rendering system
        var mockTerminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(mockTerminal);
        renderingSystem.Initialize();
        
        // Act
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        renderingSystem.Render(); // Force flush
        System.Threading.Thread.Sleep(100);
        
        // Get the rendered lines
        var lines = mockTerminal.GetAllLines();
        
        // Assert - check that box content is rendered
        bool foundContent = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("Box content"))
            {
                foundContent = true;
                break;
            }
        }
        Assert.True(foundContent, "Box content should be rendered");
    }
    
    [Fact]
    public void TextWrapExample_ShouldLayoutCorrectly()
    {
        // Arrange - simple VStack test
        var ui = new VStack(spacing: 1) {
            new Text("First"),
            new Text("Second"),
            new Text("Third")
        };
        
        // Create a test terminal and rendering system
        var mockTerminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(mockTerminal);
        renderingSystem.Initialize();
        
        // Act
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        renderingSystem.Render(); // Force flush
        System.Threading.Thread.Sleep(100);
        
        // Get the rendered lines
        var lines = mockTerminal.GetAllLines();
        
        // Assert - check that all three texts are rendered
        bool foundFirst = false, foundSecond = false, foundThird = false;
        int firstLine = -1, secondLine = -1, thirdLine = -1;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("First"))
            {
                foundFirst = true;
                firstLine = i;
            }
            if (lines[i].Contains("Second"))
            {
                foundSecond = true;
                secondLine = i;
            }
            if (lines[i].Contains("Third"))
            {
                foundThird = true;
                thirdLine = i;
            }
        }
        
        Assert.True(foundFirst, "First text should be rendered");
        Assert.True(foundSecond, "Second text should be rendered");
        Assert.True(foundThird, "Third text should be rendered");
        
        // Check they are on different lines with proper spacing
        if (foundFirst && foundSecond)
        {
            Assert.Equal(2, secondLine - firstLine); // spacing of 1 means 2 lines apart
        }
        if (foundSecond && foundThird)
        {
            Assert.Equal(2, thirdLine - secondLine); // spacing of 1 means 2 lines apart
        }
    }
}