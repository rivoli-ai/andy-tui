namespace Andy.TUI.Examples.BubbleTea;

using System.Threading;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class Debounce
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new DebounceApp(renderer);
        renderer.Run(() => app.Render());
    }

    private class DebounceApp
    {
        private readonly DeclarativeRenderer _renderer;
        private string _query = string.Empty;
        private string _debounced = string.Empty;
        private Timer? _timer;

        public DebounceApp(DeclarativeRenderer renderer) { _renderer = renderer; }

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("Debounce: type below (updates after 500ms)").Bold(),
                new TextField("Type...", new Binding<string>(() => _query, v => OnInput(v))),
                new Text($"Debounced: {_debounced}")
            };
        }

        private void OnInput(string value)
        {
            _query = value;
            _timer?.Dispose();
            _timer = new Timer(_ => { _debounced = _query; _renderer.Render(new VStack()); }, null, 500, Timeout.Infinite);
        }
    }
}
