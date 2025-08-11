using System;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Focus;

public class FocusRegistrationDebugTest
{
    private readonly ITestOutputHelper _output;
    
    public FocusRegistrationDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void ComponentsAreRegisteredWithFocusManager()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        // We need to create the renderer
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string name = string.Empty;
        string pass = string.Empty;

        ISimpleComponent Root()
        {
            // Capture the context from within the render method
            return new VStack(spacing: 1)
            {
                new Text("Form").Bold(),
                new TextField("Name", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => name = v)),
                new TextField("Pass", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => pass = v)),
                new Button("Submit", () => { })
            };
        }

        renderingSystem.Initialize();
        
        // Run renderer in background
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();
        
        // Give time for initial render
        Thread.Sleep(200);
        
        // Now we need to access the FocusManager somehow...
        // Since we can't directly access it, let's test through behavior
        
        // Emit TAB and see if any component receives focus  
        _output.WriteLine("=== Testing focus through TAB navigation ===");
        
        // First TAB
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);
        _output.WriteLine($"After first TAB + '1': name='{name}', pass='{pass}'");
        
        // Second TAB
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);
        _output.WriteLine($"After second TAB + '2': name='{name}', pass='{pass}'");
        
        // Third TAB (should go to button)
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('3', ConsoleKey.D3);
        Thread.Sleep(50);
        _output.WriteLine($"After third TAB + '3': name='{name}', pass='{pass}'");
        
        // Fourth TAB (should cycle back to first field)
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('4', ConsoleKey.D4); 
        Thread.Sleep(50);
        _output.WriteLine($"After fourth TAB + '4': name='{name}', pass='{pass}'");
        
        // Expected: name should have "14", pass should have "2", button shouldn't accept '3'
        // If TAB doesn't work, everything will go to the same field
        
        // Check if focus is moving or stuck
        if (name.Length > 0 && pass.Length > 0)
        {
            _output.WriteLine("SUCCESS: Focus moved between fields!");
        }
        else if (name.Length > 1 || pass.Length > 1)
        {
            _output.WriteLine("FAILURE: Focus stuck on one field!");
            Assert.Fail("Focus is not moving between fields");
        }
        else
        {
            _output.WriteLine("FAILURE: No input received!");
            Assert.Fail("No field is receiving input");
        }
        
        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}