using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Chat;

public class ChatAppDeclarative
{
    // State - Using observable types for automatic UI updates
    private readonly ObservableList<ChatMessage> _messages = new();
    private readonly ObservableProperty<string> _input = new("");
    private readonly ObservableProperty<string> _status = new("Ready");
    private int _scroll = 0;
    private DeclarativeRenderer? _renderer;

    private CerebrasHttpChatClient? _client;

    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        _renderer = new DeclarativeRenderer(renderingSystem);
        renderingSystem.Initialize();

        // Subscribe to observable changes to trigger UI updates
        _messages.CollectionChanged += (_, __) => _renderer?.RequestRender();
        _input.PropertyChanged += (_, __) => _renderer?.RequestRender();
        _status.PropertyChanged += (_, __) => _renderer?.RequestRender();

        var cfg = ChatConfiguration.Load();
        if (string.IsNullOrWhiteSpace(cfg.ApiKey))
        {
            _status.Value = "Missing CEREBRAS_API_KEY environment variable";
        }
        else
        {
            _client = new CerebrasHttpChatClient(cfg);
        }
        // Attach key handler for Enter/Alt+Enter behavior while TextArea focused
        var inputHandler = new ConsoleInputHandler();
        inputHandler.KeyPressed += OnKey;
        _renderer = new DeclarativeRenderer(renderingSystem, inputHandler);
        _renderer.Run(BuildUI);
    }

    private void OnKey(object? sender, KeyEventArgs e)
    {
        // Enter to send; Alt+Enter to insert newline
        if (e.Key == ConsoleKey.Enter)
        {
            if (e.Modifiers.HasFlag(System.ConsoleModifiers.Alt))
            {
                _input.Value += "\n";
            }
            else
            {
                // Only send if the TextArea is focused to avoid double-handling
                var focused = _renderer?.Context?.FocusManager.FocusedComponent;
                if (focused is TextAreaInstance)
                {
                    // Fire-and-forget send and prevent TextArea from also inserting a newline
                    _ = SendAsync();
                }
            }
        }

        // Alt+Up/Alt+Down to scroll conversation
        if (e.Modifiers.HasFlag(System.ConsoleModifiers.Alt))
        {
            if (e.Key == ConsoleKey.UpArrow)
            {
                // Increase scroll up to max lines - viewport
                var maxScroll = Math.Max(0, RenderLines(100 - 4).Count - 16);
                _scroll = Math.Min(_scroll + 1, maxScroll);
                _renderer?.RequestRender();
            }
            else if (e.Key == ConsoleKey.DownArrow)
            {
                _scroll = Math.Max(0, _scroll - 1);
                _renderer?.RequestRender();
            }
        }
    }

    private ISimpleComponent BuildUI()
    {
        var width = 100; // fixed logical width; renderer will clip if needed
        var conversationLines = RenderLines(width - 4);

        return new VStack(spacing: 1)
        {
            new Text($"Andy.TUI Chat — Model: {(_client?.Model ?? "<not configured>")}").Title().Color(Color.Cyan),
            new Text(_status.Value).Color(Color.Gray),

            new Box
            {
                // Clear background to prevent flicker/dirty artifacts before text updates
                new Box { new Text(new string(' ', width - 2)) }
                    .WithWidth(width - 2)
                    .WithHeight(16)
                    .WithPadding(new Andy.TUI.Layout.Spacing(0,0,0,0)),
                BuildConversation(conversationLines, width - 2, height: 16)
            }.WithWidth(width).WithHeight(16).WithPadding(new Andy.TUI.Layout.Spacing(1,1,1,1)),

            new VStack(spacing: 1)
            {
                new Text("> ").Bold().Color(Color.Green),
                // Larger TextArea for composing; Enter to send, Alt+Enter for newline
                new TextArea("Type a message…", new Binding<string>(
                    () => _input.Value,
                    v => _input.Value = v,
                    "Input"), rows: 4, cols: width - 4),
                new HStack(spacing: 2)
                {
                    // Remove Send button; keep New Chat to clear
                    new Button("New Chat", () => { _messages.Clear(); _input.Value = ""; _status.Value = "New conversation"; }).Secondary()
                }
            }
        };
    }

    private VStack BuildConversation(List<(string Text, bool IsUser)> lines, int width, int height)
    {
        // Make scrollable and ellipsize consistently
        var v = new VStack(spacing: 0);
        var start = Math.Max(0, lines.Count - height - _scroll);
        var end = Math.Min(lines.Count, start + height);
        for (int i = start; i < end; i++)
        {
            var (text, isUser) = lines[i];
            var content = text.Length > width ? text.Substring(0, width) : text.PadRight(width);
            v.Add(new Text(content).Color(isUser ? Color.Cyan : Color.White));
        }
        int current = CountChildren(v);
        for (int i = current; i < height; i++) v.Add(" ");
        return v;
    }

    private static int CountChildren(VStack stack)
    {
        int n = 0;
        foreach (var _ in stack) n++;
        return n;
    }

    private List<(string Text, bool IsUser)> RenderLines(int maxWidth)
    {
        var list = new List<(string, bool)>();
        foreach (var m in _messages)
        {
            var prefix = m.Role == "user" ? "You: " : "Assistant: ";
            foreach (var line in Wrap(prefix + m.Content, maxWidth))
                list.Add((line, m.Role == "user"));
        }
        return list;
    }

    private IEnumerable<string> Wrap(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text)) yield break;
        var words = text.Split(' ');
        var current = "";
        foreach (var w in words)
        {
            if (current.Length == 0) { current = w; continue; }
            if (current.Length + 1 + w.Length > maxWidth)
            {
                yield return current;
                current = w;
            }
            else
            {
                current += " " + w;
            }
        }
        if (current.Length > 0) yield return current;
    }

    private async Task SendAsync()
    {
        var content = _input.Value.Trim();
        if (content.Length == 0) return;
        if (_client == null)
        {
            _status.Value = "Set CEREBRAS_API_KEY and restart";
            return;
        }
        _messages.Add(new ChatMessage("user", content));
        _input.Value = "";
        _status.Value = "Sending…";

        try
        {
            var reply = await _client.CreateCompletionAsync(_messages.ToList());
            _messages.Add(new ChatMessage("assistant", reply));
            _status.Value = "Ready";
        }
        catch (Exception ex)
        {
            _messages.Add(new ChatMessage("assistant", $"[error] {ex.Message}"));
            _status.Value = "Error";
        }
    }
}
