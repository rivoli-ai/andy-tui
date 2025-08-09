namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class ListDefault
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new ListDefaultApp();
        renderer.Run(() => app.Render());
    }

    private class ListDefaultApp
    {
        private readonly List<string> _songs = new()
        {
            "Red Right Hand","Where Is My Mind?","All My Friends","Hoppípolla","Reckoner",
            "Nikes","Dreams","The Less I Know The Better","Blue Monday","Time"
        };
        private Optional<string> _selected = Optional<string>.None;

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("Default List (Up/Down Enter, Ctrl+C to quit)").Bold(),
                new SelectInput<string>(_songs, new Binding<Optional<string>>(() => _selected, v => _selected = v), s => s, 8, "Choose a song")
                    .VisibleItems(8),
                new Text(_selected.HasValue ? $"▶ {_selected.Value}" : "(none)")
                    .Color(_selected.HasValue ? Color.Cyan : Color.DarkGray)
            };
        }
    }
}
