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
/// Simple test to verify basic text input in two fields
/// </summary>
public class SimpleTwoFieldsTest
{
    private readonly ITestOutputHelper _output;
    
    public SimpleTwoFieldsTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void TwoFields_SimpleInput_VerifyFieldOrder()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        // State for two fields
        string field1 = string.Empty;
        string field2 = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Test Two Fields"),
                new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
                new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v))
            };
        }
        
        renderingSystem.Initialize();
        
        // Run renderer in background
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        // Give time for initial render
        Thread.Sleep(200);
        
        // Act - First TAB should focus first field
        _output.WriteLine("=== TAB once ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Type '1' - should go to first field
        _output.WriteLine("=== Type '1' ===");
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);
        
        _output.WriteLine($"After first input: field1='{field1}', field2='{field2}'");
        
        // TAB to second field
        _output.WriteLine("=== TAB again ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Type '2' - should go to second field
        _output.WriteLine("=== Type '2' ===");
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);
        
        _output.WriteLine($"After second input: field1='{field1}', field2='{field2}'");
        
        // Assert
        Assert.Equal("1", field1);
        Assert.Equal("2", field2);
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
    
    [Fact]
    public void TwoFields_DirectTyping_VerifyContent()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string name = string.Empty;
        string email = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Simple Form"),
                new HStack(spacing: 1)
                {
                    new Text("Name:"),
                    new TextField("", new Binding<string>(() => name, v => name = v))
                },
                new HStack(spacing: 1)
                {
                    new Text("Email:"),
                    new TextField("", new Binding<string>(() => email, v => email = v))
                }
            };
        }
        
        renderingSystem.Initialize();
        
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        Thread.Sleep(200);
        
        // Focus first field and type
        _output.WriteLine("=== Focus name field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Type 'Bob' ===");
        input.EmitKey('B', ConsoleKey.B);
        Thread.Sleep(50);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        input.EmitKey('b', ConsoleKey.B);
        Thread.Sleep(50);
        
        _output.WriteLine($"Name after typing: '{name}'");
        
        // Move to email field
        _output.WriteLine("=== TAB to email field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Type 'bob@' ===");
        input.EmitKey('b', ConsoleKey.B);
        Thread.Sleep(50);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        input.EmitKey('b', ConsoleKey.B);
        Thread.Sleep(50);
        input.EmitKey('@', ConsoleKey.D2, ConsoleModifiers.Shift);
        Thread.Sleep(50);
        
        _output.WriteLine($"Final - Name: '{name}', Email: '{email}'");
        
        // Assert - we check that each field got some input
        Assert.NotEmpty(name);
        Assert.NotEmpty(email);
        Assert.Contains("b", name.ToLower());
        Assert.Contains("@", email);
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
}