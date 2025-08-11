using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Diagnostic test to understand focus order behavior
/// </summary>
public class FocusOrderDiagnosticTest
{
    private readonly ITestOutputHelper _output;
    
    public FocusOrderDiagnosticTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ThreeFields_TraceFocusOrder()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string field1 = string.Empty;
        string field2 = string.Empty;
        string field3 = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Focus Order Test"),
                new TextField("First", new Binding<string>(() => field1, v => field1 = v)),
                new TextField("Second", new Binding<string>(() => field2, v => field2 = v)),
                new TextField("Third", new Binding<string>(() => field3, v => field3 = v))
            };
        }
        
        renderingSystem.Initialize();
        
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        Thread.Sleep(200);
        
        _output.WriteLine("=== Starting focus test ===");
        
        // First TAB
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);
        _output.WriteLine($"After 1st TAB + '1': field1='{field1}', field2='{field2}', field3='{field3}'");
        
        // Second TAB
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);
        _output.WriteLine($"After 2nd TAB + '2': field1='{field1}', field2='{field2}', field3='{field3}'");
        
        // Third TAB
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('3', ConsoleKey.D3);
        Thread.Sleep(50);
        _output.WriteLine($"After 3rd TAB + '3': field1='{field1}', field2='{field2}', field3='{field3}'");
        
        // Fourth TAB (should cycle back)
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('4', ConsoleKey.D4);
        Thread.Sleep(50);
        _output.WriteLine($"After 4th TAB + '4': field1='{field1}', field2='{field2}', field3='{field3}'");
        
        // Analysis
        _output.WriteLine("\n=== Analysis ===");
        if (field1 == "14") {
            _output.WriteLine("Focus order: Third -> First -> Second -> Third (cycles correctly but starts at Third)");
        } else if (field1 == "41") {
            _output.WriteLine("Focus order: Reverse - Third -> Second -> First -> cycle back");
        } else if (field2 == "14") {
            _output.WriteLine("Focus order: First -> Second -> Third -> First (correct order but starts at wrong field)");
        } else if (field3 == "14") {
            _output.WriteLine("Focus order: Second -> Third -> First -> Second (rotated)");
        } else {
            _output.WriteLine($"Unexpected pattern - needs investigation");
        }
        
        // This test is diagnostic, so we just want to see the output
        // Not asserting anything specific
        Assert.True(true);
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
}