namespace Andy.TUI.Examples.BubbleTea;

using System.Net.Http;
using System.Threading.Tasks;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class HttpExample
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new HttpApp(renderer);
        renderer.Run(() => app.Render());
    }

    private class HttpApp
    {
        private readonly DeclarativeRenderer _renderer;
        private string _status = "Press [Fetch] to GET example.com";
        private string _body = "";
        private bool _loading = false;

        public HttpApp(DeclarativeRenderer renderer) { _renderer = renderer; }

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("HTTP Fetch").Bold(),
                new HStack(spacing: 1) {
                    _loading ? (ISimpleComponent)new Spinner(SpinnerStyle.Dots, label: "Loading...") : new Text(""),
                    new Text(_status).Color(Color.Cyan),
                },
                new Button(_loading ? "Fetching..." : "Fetch", () => { if (!_loading) _ = Fetch(); }),
                new Text(_body).Wrap(TextWrap.Word).MaxWidth(70)
            };
        }

        private async Task Fetch()
        {
            _status = "Fetching...";
            _loading = true;
            _renderer.Render(new VStack());
            try
            {
                using var client = new HttpClient();
                _body = await client.GetStringAsync("https://example.com");
                _status = "Done";
            }
            catch (Exception ex)
            {
                _status = $"Error: {ex.Message}";
            }
            _loading = false;
            _renderer.Render(new VStack());
        }
    }
}
