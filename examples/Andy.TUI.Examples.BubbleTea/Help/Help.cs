namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class Help
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        renderer.Run(() => new VStack(spacing: 1) {
            new Text("Help").Bold(),
            new Text("q: quit  |  ↑/↓: move  |  enter: select  |  tab: next focus").Dim()
        });
    }
}
