using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class TabNavigationDebugTest
{
    private readonly ITestOutputHelper _output;
    
    public TabNavigationDebugTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void TabNavigation_ShouldMoveFocusBetweenComponents()
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
            new TextField("Enter your name...", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => name = v)),
            new TextField("Enter password...", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => pass = v)).Secure(),
            new Button("Submit", () => { /* no-op */ })
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(100);

        // Tab to focus first input
        _output.WriteLine("Pressing first TAB");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        // Tab to second input
        _output.WriteLine("Pressing second TAB");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type 'A' into password field
        _output.WriteLine("Typing 'A' into password field");
        input.EmitKey('A', ConsoleKey.A);
        Thread.Sleep(50);

        // Check state
        _output.WriteLine($"Name: '{name}', Pass: '{pass}'");
        
        // Ensure app is still running and state updated
        Assert.True(thread.IsAlive);
        Assert.Equal(string.Empty, name);
        Assert.Equal("A", pass);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}