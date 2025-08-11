using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Tests.Integration;

public class ClippingDebugTest : TestBase
{
    private readonly ITestOutputHelper _testOutput;
    
    public ClippingDebugTest(ITestOutputHelper output) : base(output)
    {
        _testOutput = output;
    }
    
    [Fact]
    public void SimpleBox_WithoutClipping_ShouldRenderCorrectly()
    {
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new VirtualDomRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        // Create a simple tree with a box (no clipping)
        var tree = new ElementNode("fragment");
        
        // Add header at (0, 0)
        var header = new ElementNode("text");
        header.Props["x"] = 0;
        header.Props["y"] = 0;
        header.AddChild(new TextNode("Header"));
        tree.AddChild(header);
        
        // Add box content at (0, 2) - simulating what Box would do WITHOUT clipping
        var boxContent = new ElementNode("text");
        boxContent.Props["x"] = 0;
        boxContent.Props["y"] = 2;
        boxContent.AddChild(new TextNode("Box content"));
        tree.AddChild(boxContent);
        
        // Add footer at (0, 4)
        var footer = new ElementNode("text");
        footer.Props["x"] = 0;
        footer.Props["y"] = 4;
        footer.AddChild(new TextNode("Footer"));
        tree.AddChild(footer);
        
        renderer.Render(tree);
        renderingSystem.Render();
        Thread.Sleep(100);
        
        _testOutput.WriteLine("Without clipping:");
        for (int y = 0; y < 6; y++)
        {
            _testOutput.WriteLine($"Line {y}: |{terminal.GetLine(y)}|");
        }
        
        Assert.Equal("Header", terminal.GetLine(0).TrimEnd());
        Assert.Equal("Box content", terminal.GetLine(2).TrimEnd());
        Assert.Equal("Footer", terminal.GetLine(4).TrimEnd());
    }
    
    [Fact]
    public void SimpleBox_WithClipping_ShouldRenderCorrectly()
    {
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new VirtualDomRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        // Create a tree with clipping
        var tree = new ElementNode("fragment");
        
        // Add header at (0, 0)
        var header = new ElementNode("text");
        header.Props["x"] = 0;
        header.Props["y"] = 0;
        header.AddChild(new TextNode("Header"));
        tree.AddChild(header);
        
        // Add clipped box at position (0, 2) with size 20x3
        // Box content inside clipping area - positions should be ABSOLUTE
        var boxContent1 = new ElementNode("text");
        boxContent1.Props["x"] = 0;  // Absolute position
        boxContent1.Props["y"] = 2;  // Absolute position (same as clip Y)
        boxContent1.AddChild(new TextNode("Box line 1"));
        
        var boxContent2 = new ElementNode("text");
        boxContent2.Props["x"] = 0;
        boxContent2.Props["y"] = 3;  // Next line
        boxContent2.AddChild(new TextNode("Box line 2"));
        
        var clipNode = new ClippingNode(0, 2, 20, 3, boxContent1, boxContent2);
        
        tree.AddChild(clipNode);
        
        // Add footer at (0, 6)
        var footer = new ElementNode("text");
        footer.Props["x"] = 0;
        footer.Props["y"] = 6;
        footer.AddChild(new TextNode("Footer"));
        tree.AddChild(footer);
        
        renderer.Render(tree);
        renderingSystem.Render();
        Thread.Sleep(100);
        
        _testOutput.WriteLine("With clipping:");
        for (int y = 0; y < 8; y++)
        {
            _testOutput.WriteLine($"Line {y}: |{terminal.GetLine(y)}|");
        }
        
        Assert.Equal("Header", terminal.GetLine(0).TrimEnd());
        Assert.Equal("Box line 1", terminal.GetLine(2).TrimEnd());
        Assert.Equal("Box line 2", terminal.GetLine(3).TrimEnd());
        Assert.Equal("Footer", terminal.GetLine(6).TrimEnd());
    }
    
    [Fact]
    public void Clipping_ShouldPreventRenderingOutsideBounds()
    {
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new VirtualDomRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        var tree = new ElementNode("fragment");
        
        // Add text that should be visible (inside bounds)
        var inside = new ElementNode("text");
        inside.Props["x"] = 5;
        inside.Props["y"] = 5;
        inside.AddChild(new TextNode("Inside"));
        
        // Add text that should be clipped (outside bounds)
        var outside = new ElementNode("text");
        outside.Props["x"] = 5;
        outside.Props["y"] = 8;  // Outside clip area (5+2=7 is the limit)
        outside.AddChild(new TextNode("Outside"));
        
        // Create clipping area at (5, 5) with size 10x2
        var clipNode = new ClippingNode(5, 5, 10, 2, inside, outside);
        
        tree.AddChild(clipNode);
        
        renderer.Render(tree);
        renderingSystem.Render();
        Thread.Sleep(100);
        
        _testOutput.WriteLine("Clipping bounds test:");
        for (int y = 4; y < 10; y++)
        {
            _testOutput.WriteLine($"Line {y}: |{terminal.GetLine(y)}|");
        }
        
        // Text at line 5 should be visible
        Assert.Contains("Inside", terminal.GetLine(5));
        
        // Text at line 8 should be clipped (not visible)
        Assert.DoesNotContain("Outside", terminal.GetLine(8));
    }
}