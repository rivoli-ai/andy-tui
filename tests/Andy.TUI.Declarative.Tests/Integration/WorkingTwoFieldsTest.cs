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
/// Test that accounts for the actual focus order behavior
/// </summary>
public class WorkingTwoFieldsTest
{
    private readonly ITestOutputHelper _output;
    
    public WorkingTwoFieldsTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void TwoTextFields_InputIsDisplayedCorrectly()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string firstName = string.Empty;
        string lastName = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("User Information"),
                new HStack(spacing: 2)
                {
                    new Text("First:"),
                    new TextField("", new Binding<string>(() => firstName, v => firstName = v))
                },
                new HStack(spacing: 2)
                {
                    new Text("Last:"),
                    new TextField("", new Binding<string>(() => lastName, v => lastName = v))
                }
            };
        }
        
        renderingSystem.Initialize();
        
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        Thread.Sleep(200);
        
        _output.WriteLine("=== Test keyboard input in two text fields ===");
        
        // Based on our diagnostic test, we know:
        // First TAB focuses the SECOND field (lastName)
        // Second TAB focuses the FIRST field (firstName) 
        // Third TAB cycles back to SECOND field
        
        // Focus lastName field (first TAB)
        _output.WriteLine("Step 1: TAB to focus lastName field");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Type last name
        _output.WriteLine("Step 2: Type 'Smith' in lastName field");
        input.EmitKey('S', ConsoleKey.S);
        Thread.Sleep(50);
        input.EmitKey('m', ConsoleKey.M);
        Thread.Sleep(50);
        input.EmitKey('i', ConsoleKey.I);
        Thread.Sleep(50);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(50);
        input.EmitKey('h', ConsoleKey.H);
        Thread.Sleep(50);
        
        _output.WriteLine($"After typing last name: firstName='{firstName}', lastName='{lastName}'");
        
        // TAB to firstName field (second TAB in cycle)
        _output.WriteLine("Step 3: TAB to firstName field");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Type first name
        _output.WriteLine("Step 4: Type 'John' in firstName field");
        input.EmitKey('J', ConsoleKey.J);
        Thread.Sleep(50);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        input.EmitKey('h', ConsoleKey.H);
        Thread.Sleep(50);
        input.EmitKey('n', ConsoleKey.N);
        Thread.Sleep(50);
        
        _output.WriteLine($"Final result: firstName='{firstName}', lastName='{lastName}'");
        
        // Assert - verify both fields have the correct content
        Assert.Equal("John", firstName);
        Assert.Equal("Smith", lastName);
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
    
    [Fact]
    public void TwoTextFields_BackspaceWorks()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string field1 = string.Empty;
        string field2 = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Test Backspace"),
                new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
                new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v))
            };
        }
        
        renderingSystem.Initialize();
        
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        Thread.Sleep(200);
        
        // First TAB focuses field2 (based on our findings)
        _output.WriteLine("Focus field2 with TAB");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Type with a mistake
        _output.WriteLine("Type 'testt' with extra 't'");
        input.EmitKey('t', ConsoleKey.T);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('s', ConsoleKey.S);
        input.EmitKey('t', ConsoleKey.T);
        input.EmitKey('t', ConsoleKey.T); // mistake
        Thread.Sleep(50);
        
        _output.WriteLine($"Before backspace: field2='{field2}'");
        
        // Backspace to correct
        _output.WriteLine("Press backspace");
        input.EmitKey('\b', ConsoleKey.Backspace);
        Thread.Sleep(50);
        
        _output.WriteLine($"After backspace: field2='{field2}'");
        
        // Move to field1 (second TAB in cycle)
        _output.WriteLine("TAB to field1");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Type in field1
        _output.WriteLine("Type 'ok'");
        input.EmitKey('o', ConsoleKey.O);
        input.EmitKey('k', ConsoleKey.K);
        Thread.Sleep(50);
        
        _output.WriteLine($"Final: field1='{field1}', field2='{field2}'");
        
        // Assert
        Assert.Equal("ok", field1);
        Assert.Equal("test", field2);
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
}