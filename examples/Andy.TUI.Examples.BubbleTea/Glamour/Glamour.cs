namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class Glamour
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        string md = "# Glamour\n\n*This is a simple markdown-like demo.*\n\n- Items\n- With\n- Style";

        renderer.Run(() => new VStack(spacing: 1) {
            new Text("Glamour (markdown) demo").Bold(),
            new Text(md).Wrap(TextWrap.Word).MaxWidth(60)
        });
    }
}
