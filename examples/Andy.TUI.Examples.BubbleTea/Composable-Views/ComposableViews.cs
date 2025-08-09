namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class ComposableViews
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var parent = new ParentModel();
        renderer.Run(() => parent.Render());
    }

    private class ParentModel
    {
        private readonly Counter _left = new("Left");
        private readonly Counter _right = new("Right");

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("Composable Views: independent counters").Bold(),
                new HStack(spacing: 4) {
                    _left.View(),
                    _right.View()
                }
            };
        }
    }

    private class Counter
    {
        private int _count = 0;
        private readonly string _name;

        public Counter(string name) { _name = name; }

        public ISimpleComponent View()
        {
            return new VStack(spacing: 1) {
                new Text($"{_name} Counter: {_count}")
                    .Color(_count % 2 == 0 ? Color.Cyan : Color.Magenta),
                new HStack(spacing: 2) {
                    new Button("+", () => _count++),
                    new Button("-", () => _count = Math.Max(0, _count - 1))
                }
            };
        }
    }
}
