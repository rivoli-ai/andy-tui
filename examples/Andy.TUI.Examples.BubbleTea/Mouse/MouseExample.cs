namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class MouseExample
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        // Note: current ConsoleInputHandler reports SupportsMouseInput=false in this environment
        renderer.Run(() => new VStack(spacing: 1) {
            new Text("Mouse demo (if supported by terminal)").Bold(),
            new Text("Move/click/wheel to see events. Ctrl+C to quit.").Dim()
        });
    }
}
