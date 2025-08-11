namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class ListFancy
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new ListFancyApp();
        renderer.Run(() => app.Render());
    }

    private class ListFancyApp
    {
        private readonly List<(string Title, string Artist)> _tracks = new()
        {
            ("Everything in Its Right Place","Radiohead"),
            ("Midnight City","M83"),
            ("Lose Yourself to Dance","Daft Punk"),
            ("Motion Picture Soundtrack","Radiohead"),
            ("Seigfried","Frank Ocean"),
            ("Porcelain","Moby"),
            ("Roygbiv","Boards of Canada"),
            ("Breathe","Télépopmusik"),
            ("Neon Bible","Arcade Fire"),
            ("Oblivion","Grimes")
        };

        private Optional<int> _selectedIndex = Optional<int>.None;

        public ISimpleComponent Render()
        {
            var items = _tracks.Select((t, i) => (ISimpleComponent)
                new HStack(spacing: 1) {
                    new Text(i < 9 ? $" 0{i+1}." : $" {i+1}. ").Dim(),
                    new Text($"{t.Title}").Bold(),
                    new Text("—").Dim(),
                    new Text(t.Artist).Color(Color.DarkGray)
                }
            ).ToList();

            var listBox = new Box().WithWidth(60).WithHeight(12).WithOverflow(Overflow.Scroll);
            foreach (var item in items) listBox.Add(item);

            return new VStack(spacing: 1) {
                new Text("Fancy List (arrows + enter)").Bold(),
                listBox,
                new Text(_selectedIndex.HasValue
                    ? $"Playing: {_tracks[_selectedIndex.Value].Title}"
                    : "Select a track...")
                    .Color(_selectedIndex.HasValue ? Color.Green : Color.DarkGray)
            };
        }
    }
}
