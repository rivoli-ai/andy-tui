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
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Test to verify component positioning and layout calculations.
/// </summary>
public class ComponentPositioningTest : TestBase
{
    private readonly ITestOutputHelper _testOutput;

    public ComponentPositioningTest(ITestOutputHelper output) : base(output)
    {
        _testOutput = output;
    }

    [Fact]
    public void VStack_ShouldPositionChildren_Correctly()
    {
        // Arrange
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        // Create a simple VStack with known content
        ISimpleComponent CreateUI()
        {
            return new VStack(spacing: 0) {
                new Text("Line 1"),
                new Text("Line 2"),
                new Text("Line 3")
            };
        }

        // Act
        renderingSystem.Initialize();
        renderer.Render(CreateUI());
        renderingSystem.Render(); // Force flush
        Thread.Sleep(100);

        // Assert - Check positions
        _testOutput.WriteLine("VStack output:");
        for (int y = 0; y < 5; y++)
        {
            var line = terminal.GetLine(y);
            _testOutput.WriteLine($"Line {y}: |{line}|");
        }

        Assert.Equal("Line 1", terminal.GetLine(0).TrimEnd());
        Assert.Equal("Line 2", terminal.GetLine(1).TrimEnd());
        Assert.Equal("Line 3", terminal.GetLine(2).TrimEnd());
    }

    [Fact]
    public void VStack_WithSpacing_ShouldAddGaps()
    {
        // Arrange
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        // Create VStack with spacing
        ISimpleComponent CreateUI()
        {
            return new VStack(spacing: 2) {
                new Text("Line 1"),
                new Text("Line 2"),
                new Text("Line 3")
            };
        }

        // Act
        renderingSystem.Initialize();
        renderer.Render(CreateUI());
        renderingSystem.Render();
        Thread.Sleep(100);

        // Assert - With spacing=2, lines should be at y=0, y=3, y=6
        _testOutput.WriteLine("VStack with spacing output:");
        for (int y = 0; y < 8; y++)
        {
            var line = terminal.GetLine(y);
            _testOutput.WriteLine($"Line {y}: |{line}|");
        }

        Assert.Equal("Line 1", terminal.GetLine(0).TrimEnd());
        Assert.Equal("", terminal.GetLine(1).TrimEnd()); // Gap
        Assert.Equal("", terminal.GetLine(2).TrimEnd()); // Gap
        Assert.Equal("Line 2", terminal.GetLine(3).TrimEnd());
        Assert.Equal("", terminal.GetLine(4).TrimEnd()); // Gap
        Assert.Equal("", terminal.GetLine(5).TrimEnd()); // Gap
        Assert.Equal("Line 3", terminal.GetLine(6).TrimEnd());
    }

    [Fact]
    public void NestedContainers_ShouldCalculatePositions_Correctly()
    {
        // Arrange
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        // Create nested structure
        ISimpleComponent CreateUI()
        {
            var box = new Box { Height = 5, Width = 40, Overflow = Overflow.Hidden };
            box.Add(new VStack(spacing: 0) {
                new Text("Box Line 1"),
                new Text("Box Line 2")
            });

            return new VStack(spacing: 1) {
                new Text("Header"),
                box,
                new Text("Footer")
            };
        }

        // Act
        // Enable comprehensive logging for debugging
        Andy.TUI.Diagnostics.ComprehensiveLoggingInitializer.Initialize(isTestMode: true);

        renderingSystem.Initialize();
        renderer.Render(CreateUI());
        renderingSystem.Render();
        Thread.Sleep(100);

        // Assert
        _testOutput.WriteLine("Nested containers output:");
        for (int y = 0; y < 10; y++)
        {
            var line = terminal.GetLine(y);
            _testOutput.WriteLine($"Line {y}: |{line}|");
        }

        // Header at line 0
        Assert.Contains("Header", terminal.GetLine(0));

        // Box content starts at line 2 (header + spacing)
        // Box should contain its content
        bool foundBoxLine1 = false;
        bool foundBoxLine2 = false;
        for (int y = 1; y < 7; y++) // Box area
        {
            var line = terminal.GetLine(y);
            if (line.Contains("Box Line 1")) foundBoxLine1 = true;
            if (line.Contains("Box Line 2")) foundBoxLine2 = true;
        }
        Assert.True(foundBoxLine1, "Box Line 1 should be visible");
        Assert.True(foundBoxLine2, "Box Line 2 should be visible");

        // Footer should be after the box
        bool foundFooter = false;
        for (int y = 7; y < 10; y++)
        {
            if (terminal.GetLine(y).Contains("Footer"))
            {
                foundFooter = true;
                break;
            }
        }
        Assert.True(foundFooter, "Footer should be visible after the box");
    }

    [Fact]
    public void MultipleRenders_ShouldNotOverlap()
    {
        // Arrange
        var terminal = new MockTerminal(80, 30);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        // First render
        ISimpleComponent CreateUI1()
        {
            return new VStack(spacing: 0) {
                new Text("First render line 1"),
                new Text("First render line 2"),
                new Text("First render line 3")
            };
        }

        renderingSystem.Initialize();
        renderer.Render(CreateUI1());
        renderingSystem.Render();
        Thread.Sleep(100);

        _testOutput.WriteLine("After first render:");
        for (int y = 0; y < 4; y++)
        {
            _testOutput.WriteLine($"Line {y}: |{terminal.GetLine(y)}|");
        }

        // Second render with different content
        ISimpleComponent CreateUI2()
        {
            return new VStack(spacing: 0) {
                new Text("Second line 1"),
                new Text("Second line 2")
            };
        }

        renderer.Render(CreateUI2());
        renderingSystem.Render();
        Thread.Sleep(100);

        // Assert - No overlap, old content should be cleared
        _testOutput.WriteLine("\nAfter second render:");
        for (int y = 0; y < 4; y++)
        {
            var line = terminal.GetLine(y);
            _testOutput.WriteLine($"Line {y}: |{line}|");
        }

        Assert.Equal("Second line 1", terminal.GetLine(0).TrimEnd());
        Assert.Equal("Second line 2", terminal.GetLine(1).TrimEnd());
        Assert.Equal("", terminal.GetLine(2).TrimEnd()); // Should be cleared
        Assert.Equal("", terminal.GetLine(3).TrimEnd()); // Should be cleared
    }
}