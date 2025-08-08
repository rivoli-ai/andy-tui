using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Core.Diagnostics;

class TestSimpleUpdate
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.Initialize();

        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);

        renderingSystem.Initialize();

        var counter = 0;

        renderer.Run(() =>
            new VStack(spacing: 2) {
                new Text("Simple Visual Update Test").Bold(),
                new Text($"Counter: {counter}"),
                new Button("Increment", () => {
                    counter++;
                    // Console.Error.WriteLine($"[DEBUG] Button clicked, counter is now: {counter}");
                }),
                new Text("Press SPACE or ENTER to click button"),
                new Text("If the counter doesn't update visually, we have a rendering issue"),
                new Text("Press Ctrl+C to exit")
            }
        );
    }
}