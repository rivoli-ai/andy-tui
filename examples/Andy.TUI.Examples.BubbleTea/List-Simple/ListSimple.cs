namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class ListSimple
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new ListSimpleApp();
        renderer.Run(() => app.Render());
    }

    private class ListSimpleApp
    {
        private readonly List<string> _items = Enumerable.Range(1, 20).Select(i => $"Item {i}").ToList();
        private Optional<string> _selected = Optional<string>.None;

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("List (Up/Down to move, Enter to select, Ctrl+C to quit)").Bold(),
                new SelectInput<string>(_items, new Binding<Optional<string>>(() => _selected, v => _selected = v), s => s, 10, "Pick an item")
                    .VisibleItems(10),
                new Text(_selected.HasValue ? $"Selected: {_selected.Value}" : "(none)")
                    .Color(_selected.HasValue ? Color.Green : Color.DarkGray)
            };
        }
    }
}
