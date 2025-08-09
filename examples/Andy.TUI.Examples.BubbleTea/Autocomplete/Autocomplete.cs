namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class Autocomplete
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new AutocompleteApp();
        renderer.Run(() => app.Render());
    }

    private class AutocompleteApp
    {
        private readonly List<string> _all = new()
        {
            "apple","apricot","avocado","banana","blackberry","blueberry","cherry","clementine","coconut","cranberry",
            "date","dragonfruit","durian","fig","grape","grapefruit","guava","kiwi","kumquat","lemon","lime",
            "lychee","mango","nectarine","orange","papaya","peach","pear","pineapple","plum","pomegranate","raspberry","strawberry","tangerine","watermelon"
        };

        private string _query = string.Empty;
        private Optional<string> _selected = Optional<string>.None;

        public ISimpleComponent Render()
        {
            var filtered = _all
                .Where(s => string.IsNullOrEmpty(_query) || s.Contains(_query, StringComparison.OrdinalIgnoreCase))
                .Take(10)
                .ToList();

            return new VStack(spacing: 1) {
                new Text("Autocomplete (type to filter, Enter to select, Ctrl+C to quit)").Bold(),
                new HStack(spacing: 1) {
                    new Text("Search:"),
                    new TextField("Start typing...", new Binding<string>(() => _query, v => _query = v))
                },
                new SelectInput<string>(filtered, new Binding<Optional<string>>(() => _selected, v => _selected = v), s => s, 7, "No matches")
                    .VisibleItems(7),
                new Text(_selected.HasValue ? $"You selected: {_selected.Value}" : "(no selection)")
                    .Color(_selected.HasValue ? Color.Green : Color.DarkGray)
            };
        }
    }
}
