using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class FocusDebugTest
{
    private readonly ITestOutputHelper _output;
    
    public FocusDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void FocusManager_ShouldHaveFocusableComponents()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string name = string.Empty;
        string pass = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Form").Bold(),
            new TextField("Enter your name...", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => { name = v; _output.WriteLine($"Name changed to: {v}"); })),
            new TextField("Enter password...", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => { pass = v; _output.WriteLine($"Pass changed to: {v}"); })).Secure(),
            new Button("Submit", () => _output.WriteLine("Button clicked"))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render and component registration
        Thread.Sleep(200);

        // Tab once - should focus first TextField
        _output.WriteLine("=== Pressing first TAB ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);
        
        // Type in first field
        _output.WriteLine("=== Typing 'N' in first field ===");
        input.EmitKey('N', ConsoleKey.N);
        Thread.Sleep(100);
        _output.WriteLine($"After typing N - Name: '{name}', Pass: '{pass}'");
        
        // Tab again - should focus second TextField
        _output.WriteLine("=== Pressing second TAB ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);

        // Type in second field
        _output.WriteLine("=== Typing 'P' in second field ===");
        input.EmitKey('P', ConsoleKey.P);
        Thread.Sleep(100);
        _output.WriteLine($"After typing P - Name: '{name}', Pass: '{pass}'");
        
        // Tab again - should focus Button
        _output.WriteLine("=== Pressing third TAB ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);
        
        // Press Enter on button
        _output.WriteLine("=== Pressing Enter on button ===");
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(100);

        // Check results
        _output.WriteLine($"=== Final state ===");
        _output.WriteLine($"Name: '{name}', Pass: '{pass}'");
        
        // We expect name to have 'N' and pass to have 'P'
        Assert.Equal("N", name);
        Assert.Equal("P", pass);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}