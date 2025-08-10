namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

public static class Chat
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        using var rendering = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(rendering);
        rendering.Initialize();

        var app = new ChatApp();
        renderer.Run(() => app.Render());
    }

    private class ChatApp
    {
        private readonly List<string> _messages = new();
        private string _input = string.Empty;

        public ISimpleComponent Render()
        {
            return new VStack(spacing: 1) {
                new Text("Chat (Enter to send, Ctrl+C to quit)").Bold(),
                BuildMessagesBox(),
                new HStack(spacing: 1) {
                    new Text("> ").Dim(),
                    new TextField("Type message...", new Binding<string>(() => _input, v => _input = v)),
                    new Button("Send", OnSend)
                }
            };
        }

        private Box BuildMessagesBox()
        {
            var box = new Box().WithWidth(60).WithHeight(12).WithOverflow(Overflow.Scroll);
            foreach (var m in _messages.TakeLast(10))
            {
                box.Add(new Text(m).Wrap(TextWrap.Word).MaxWidth(58));
            }
            return box;
        }

        private void OnSend()
        {
            if (!string.IsNullOrWhiteSpace(_input))
            {
                _messages.Add(_input);
                _input = string.Empty;
            }
        }
    }
}
