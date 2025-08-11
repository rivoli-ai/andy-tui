using System;
using System.Threading;
using Xunit;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class InputExampleSmokeTests
{
    [Fact]
    public void TextField_TabBetweenNameAndPass_AndType_ShouldNotExitAndUpdatesState()
    {
        var terminal = new Andy.TUI.Declarative.Tests.TestHelpers.MockTerminal(80, 24);
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

        // Give time for initial render and component registration
        Thread.Sleep(200);

        // Initial focus is on the first TextField (name)
        // Tab once to move to the second TextField (password)
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type 'A' then 'B' into password field
        input.EmitKey('A', ConsoleKey.A);
        input.EmitKey('b', ConsoleKey.B);
        // Also ensure cursor beyond end is handled
        input.EmitKey('C', ConsoleKey.C);
        Thread.Sleep(50);

        // Ensure app is still running and state updated
        Assert.True(thread.IsAlive);
        Assert.Equal(string.Empty, name);
        Assert.StartsWith("Ab", pass);

        // Cleanup
        renderingSystem.Shutdown();
    }
}
