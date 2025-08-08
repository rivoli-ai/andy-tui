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
        // Arrange - create a minimal test case
        var ui = new VStack(spacing: 2) {
            new Text("Title"),
            new Text("Header"),
            new Box { 
                new Text("Box content") 
            }
            .WithWidth(40)
            .WithHeight(5)
            .WithPadding(1)
        };
        
        // Create a test terminal and rendering system
        var mockTerminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(mockTerminal);
        renderingSystem.Initialize();
        
        // Act
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        renderingSystem.Render(); // Force flush
        
        // Assert - check the rendered output
        var lines = mockTerminal.GetAllLines();
        
        // The rendering uses ANSI escape sequences. Let's check the raw output
        var allText = string.Join("\n", lines);
        
        // Verify that text is positioned correctly using ANSI codes
        // [y;xH positions cursor at row y, column x (1-based)
        Assert.Contains("[1;1H", allText); // Title at row 1
        Assert.Contains("[4;1H", allText); // Header at row 4 (1 + spacing 2 + 1)
        Assert.Contains("[8;2H", allText); // Box content at row 8, col 2 (inside box with padding)
        
        // The actual text is written character by character with style codes
        // Let's verify the structure is correct by checking positions
        Assert.True(allText.Contains("T") && allText.Contains("i") && allText.Contains("t") && allText.Contains("l") && allText.Contains("e"));
        Assert.True(allText.Contains("H") && allText.Contains("e") && allText.Contains("a") && allText.Contains("d") && allText.Contains("e") && allText.Contains("r"));
        Assert.True(allText.Contains("B") && allText.Contains("o") && allText.Contains("x"));
    }
    
    [Fact]
    public void TextWrapExample_SimpleCase_BoxContentShouldNotOverlap()
    {
        // Arrange - recreate the exact structure from the failing example
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
        };
        
        // Create a test terminal and rendering system
        var mockTerminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(mockTerminal);
        renderingSystem.Initialize();
        
        // Act
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Render(ui);
        renderingSystem.Render(); // Force flush
        
        // Get the rendered lines
        var lines = mockTerminal.GetAllLines();
        var textByRow = new Dictionary<int, string>();
        
        // Parse ANSI sequences to extract text at each row
        var ansiPattern = new System.Text.RegularExpressions.Regex(@"\[(\d+);(\d+)H([^\x1b]+)");
        var allText = string.Join("", lines);
        var matches = ansiPattern.Matches(allText);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var row = int.Parse(match.Groups[1].Value);
            var col = int.Parse(match.Groups[2].Value);
            var text = match.Groups[3].Value;
            
            if (!textByRow.ContainsKey(row))
                textByRow[row] = "";
            
            // Pad to column position if needed
            while (textByRow[row].Length < col - 1)
                textByRow[row] += " ";
            
            textByRow[row] += text;
        }
        
        // Debug output
        Console.WriteLine("=== Parsed text by row ===");
        foreach (var kvp in textByRow.OrderBy(k => k.Key))
        {
            Console.WriteLine($"Row {kvp.Key}: '{kvp.Value}'");
        }
        
        // Also print first few lines of raw output for debugging
        Console.WriteLine("\n=== First 500 chars of raw output ===");
        Console.WriteLine(allText.Substring(0, Math.Min(500, allText.Length)));
        
        // Assert - verify that content is not on the same line
        // The header "1. No Wrap (default):" should be on a different line than the box content
        var headerKvp = textByRow.FirstOrDefault(kvp => kvp.Value.Contains("1. No Wrap"));
        var contentKvp = textByRow.FirstOrDefault(kvp => kvp.Value.Contains("Lorem ipsum"));
        
        Assert.True(headerKvp.Value != null, "Header text '1. No Wrap' not found in rendered output");
        Assert.True(contentKvp.Value != null, "Content text 'Lorem ipsum' not found in rendered output");
        
        var headerRow = headerKvp.Key;
        var contentRow = contentKvp.Key;
        
        Assert.NotEqual(0, headerRow);
        Assert.NotEqual(0, contentRow);
        Assert.NotEqual(headerRow, contentRow);
        Assert.True(contentRow > headerRow, $"Content row {contentRow} should be below header row {headerRow}");
    }
    
    [Fact]
    public void TextWrapExample_ShouldLayoutCorrectly()
    {
        // Arrange - recreate the exact structure from TextWrapTest.cs
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
        };
        
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        
        // Act
        var rootInstance = manager.GetOrCreateInstance(ui, "root");
        rootInstance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        
        // Simulate what DeclarativeRenderer does
        rootInstance.Layout.AbsoluteX = 0;
        rootInstance.Layout.AbsoluteY = 0;
        
        // Render to get the virtual DOM
        var rendered = rootInstance.Render();
        
        // Assert
        Assert.IsType<VStackInstance>(rootInstance);
        var vstackInstance = (VStackInstance)rootInstance;
        var children = vstackInstance.GetChildInstances();
        
        Assert.Equal(3, children.Count);
        
        // Check the positions of each element
        var titleText = children[0];
        var headerText = children[1];
        var box = children[2];
        
        // Title should be at Y=0
        Assert.Equal(0, titleText.Layout.AbsoluteY);
        
        // Header should be at Y=3 (title height 1 + spacing 2)
        Assert.Equal(3, headerText.Layout.AbsoluteY);
        
        // Box should be at Y=6 (previous Y 3 + header height 1 + spacing 2)
        Assert.Equal(6, box.Layout.AbsoluteY);
        
        // Check that Box has proper content positioning
        Assert.IsType<BoxInstance>(box);
        var boxInstance = (BoxInstance)box;
        var boxChildren = boxInstance.GetChildInstances();
        Assert.Single(boxChildren);
        
        // The text inside the box should be offset by padding
        var textInBox = boxChildren[0];
        // Box is at Y=6, padding is 1, so text should be at Y=7
        Assert.Equal(7, textInBox.Layout.AbsoluteY);
        
        // Let's also check the rendered output
        var elements = CollectElements(rendered);
        
        // Find the text elements and check their positions
        var textElements = elements.Where(e => e.TagName == "text").ToList();
        Assert.True(textElements.Count >= 3, $"Expected at least 3 text elements, found {textElements.Count}");
        
        // Check Y positions of text elements
        var titleY = (int)(textElements[0].Props["y"] ?? 0);
        var headerY = (int)(textElements[1].Props["y"] ?? 0);
        
        // The box content text should be further down
        var boxContentTexts = textElements.Skip(2).ToList();
        Assert.NotEmpty(boxContentTexts);
        var boxContentY = (int)(boxContentTexts[0].Props["y"] ?? 0);
        
        // Verify spacing
        Assert.Equal(0, titleY);
        Assert.Equal(3, headerY);
        Assert.True(boxContentY >= 7, $"Box content should be at Y>=7, but was at Y={boxContentY}");
    }
    
    private List<ElementNode> CollectElements(VirtualNode node)
    {
        var result = new List<ElementNode>();
        
        if (node is ElementNode element)
        {
            result.Add(element);
            foreach (var child in element.Children)
            {
                result.AddRange(CollectElements(child));
            }
        }
        else if (node is FragmentNode fragment)
        {
            foreach (var child in fragment.Children)
            {
                result.AddRange(CollectElements(child));
            }
        }
        
        return result;
    }
}