using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Chat;

/// <summary>
/// Simpler version that shows how to manually trigger UI updates.
/// The key is that the DeclarativeRenderer needs to be accessible
/// so we can call RequestRender() when state changes.
/// </summary>
public class ChatAppDeclarativeSimple
{
    // State - regular fields with Binding for input
    private readonly List<ChatMessage> _messages = new();
    private string _input = string.Empty;
    private string _status = "Ready";
    private int _scroll = 0;
    
    private CerebrasHttpChatClient? _client;
    private DeclarativeRenderer? _renderer;

    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        _renderer = new DeclarativeRenderer(renderingSystem);
        renderingSystem.Initialize();

        var cfg = ChatConfiguration.Load();
        if (string.IsNullOrWhiteSpace(cfg.ApiKey))
        {
            _status = "Missing CEREBRAS_API_KEY environment variable";
        }
        else
        {
            _client = new CerebrasHttpChatClient(cfg);
        }
        
        _renderer.Run(BuildUI);
    }

    private ISimpleComponent BuildUI()
    {
        var width = 100;
        var conversationLines = RenderLines(width - 4);

        return new VStack(spacing: 1)
        {
            new Text($"Andy.TUI Chat — Model: {(_client?.Model ?? "<not configured>")}").Title().Color(Color.Cyan),
            new Text(_status).Color(Color.Gray),

            new Box
            {
                BuildConversation(conversationLines, width - 2, height: 16)
            }.WithWidth(width).WithHeight(16).WithPadding(new Andy.TUI.Layout.Spacing(1,1,1,1)),

            new HStack(spacing: 1)
            {
                new Text("> ").Bold().Color(Color.Green),
                // The Binding here DOES trigger updates when typing
                // because TextField subscribes to PropertyChanged
                new TextField("Type a message…", this.Bind(() => _input))
            },

            new HStack(spacing: 2)
            {
                new Button("Send", async () => await SendAsync()).Primary(),
                new Button("New Chat", () => ClearChat()).Secondary()
            }
        };
    }

    private void ClearChat()
    {
        _messages.Clear();
        _input = string.Empty;
        _status = "New conversation";
        
        // IMPORTANT: Manually request UI update after changing state
        _renderer?.RequestRender();
    }

    private async Task SendAsync()
    {
        var content = (_input ?? string.Empty).Trim();
        if (content.Length == 0) return;
        if (_client == null)
        {
            _status = "Set CEREBRAS_API_KEY and restart";
            _renderer?.RequestRender();
            return;
        }
        
        // Add user message
        _messages.Add(new ChatMessage("user", content));
        _input = string.Empty;
        _status = "Sending…";
        
        // IMPORTANT: Request UI update after adding message
        _renderer?.RequestRender();

        try
        {
            var reply = await _client.CreateCompletionAsync(_messages);
            _messages.Add(new ChatMessage("assistant", reply));
            _status = "Ready";
        }
        catch (Exception ex)
        {
            _messages.Add(new ChatMessage("assistant", $"[error] {ex.Message}"));
            _status = "Error";
        }
        
        // IMPORTANT: Request UI update after receiving response
        _renderer?.RequestRender();
    }

    private VStack BuildConversation(List<(string Text, bool IsUser)> lines, int width, int height)
    {
        var v = new VStack(spacing: 0);
        var start = Math.Max(0, lines.Count - height - _scroll);
        var end = Math.Min(lines.Count, start + height);
        for (int i = start; i < end; i++)
        {
            var (text, isUser) = lines[i];
            var padded = text.Length > width ? text.Substring(0, width) : text.PadRight(width);
            v.Add(new Text(padded).Color(isUser ? Color.Cyan : Color.White));
        }
        // Pad to fixed height
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
}