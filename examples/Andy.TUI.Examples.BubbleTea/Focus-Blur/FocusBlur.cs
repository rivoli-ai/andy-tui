namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class FocusBlur
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new FocusBlurApp();
        renderer.Run(() => app.Render());
    }

    private class FocusBlurApp
    {
        private string _left = string.Empty;
        private string _right = string.Empty;

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("Focus/Blur: Tab to switch focus, Ctrl+C to quit").Bold(),
                new HStack(spacing: 2) {
                    new Text("Left:"),
                    new TextField("Type...", new Binding<string>(() => _left, v => _left = v)),
                    new Text("Right:"),
                    new TextField("Type...", new Binding<string>(() => _right, v => _right = v))
                }
            };
        }
    }
}
