using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class RecordedChatAppTest
{
    private readonly ITestOutputHelper _output;

    public RecordedChatAppTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Chat_TextArea_Typing_writ_ShouldRemainVisible()
    {
            const int width = 100;
            const int height = 28;
            var recorder = new ScreenRecorder("ChatApp_TextArea_writ", width, height);

            var terminal = new MockTerminal(width, height);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: true);

            var messages = new ObservableList<(string role, string content)>();
            var inputText = new ObservableProperty<string>("");
            var status = new ObservableProperty<string>("Ready");

            ISimpleComponent BuildUI()
            {
                var conversationLines = new System.Collections.Generic.List<(string Text, bool IsUser)>();
                foreach (var m in messages)
                {
                    var prefix = m.role == "user" ? "You: " : "Model: ";
                    var content = prefix + m.content;
                    if (content.Length > width - 4) content = content.Substring(0, width - 4);
                    conversationLines.Add((content.PadRight(width - 4), m.role == "user"));
                }

                var convo = new VStack(spacing: 0);
                foreach (var (text, isUser) in conversationLines)
                {
                    var label = isUser ? new Text("You:").Bold().Color(Color.Cyan) : new Text("Model:").Bold().Color(Color.Yellow);
                    var body = new Text(text.StartsWith("You: ") ? text.Substring(5) : (text.StartsWith("Model: ") ? text.Substring(7) : text)).Color(Color.White);
                    convo.Add(new HStack(spacing: 1) { label, body });
                }
                while (CountChildren(convo) < 16) convo.Add(" ");

                return new VStack(spacing: 1)
                {
                    new Text($"Andy.TUI Chat — Model: test").Title().Color(Color.Cyan),
                    new Text(status.Value).Color(Color.Gray),
                    new Box { convo }.WithWidth(width).WithHeight(16).WithPadding(new Andy.TUI.Layout.Spacing(1,1,1,1)),
                    new VStack(spacing: 1)
                    {
                        new Text("> ").Bold().Color(Color.Green),
                        new TextArea("Type a message…", new Binding<string>(() => inputText.Value, v => inputText.Value = v, "Input"), rows: 4, cols: width - 4)
                    }
                };
            }

            int CountChildren(VStack stack)
            {
                int n = 0; foreach (var _ in stack) n++; return n;
            }

            renderingSystem.Initialize();
            var t = new Thread(() => { try { renderer.Run(BuildUI); } catch (Exception ex) { Andy.TUI.Diagnostics.Logger.Error(ex, "Renderer thread error"); }}) { IsBackground = true };
            t.Start();

            Thread.Sleep(300);
            recorder.RecordFrame(terminal, "Initial");

            // Ensure focus lands on TextArea using a typing probe (best-effort)
            var focused = FocusTextAreaByTypingProbe(terminal, input, maxTabs: 12, scanLines: height, rows: 4, cols: width - 4);
            if (!focused)
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(120);
            }
            recorder.RecordFrame(terminal, focused ? "Focused TextArea (probe)" : "Focus failed; proceeding after Tab");

            var toType = "write a poem";
            var seen = "";
            var seenFirstCharInAnyFrame = false;
            foreach (var ch in toType)
            {
                input.EmitKey(ch, ConsoleKey.A);
                seen += ch;
                Thread.Sleep(80);
                recorder.RecordFrame(terminal, $"Type '{ch}'");

                // Extract the TextArea inner region and verify we can see at least the first char
                var area = ExtractTextAreaInnerBottomMost(terminal, rows: 4, cols: width - 4, scanLines: height);
                if (!string.IsNullOrEmpty(area) && area.Contains(seen[0].ToString()))
                {
                    seenFirstCharInAnyFrame = true;
                }
            }

            // Save and analyze (always save before assertions)
            var file = recorder.SaveToFile();
            _output.WriteLine($"Recording saved: {file}");
            var report = recorder.Analyze();

            // No high severity issues like disappearing text
            Assert.True(report.Issues.All(i => i.Type != "TextDisappeared"), "Detected disappearing text in recording");
            // Ensure first typed character was visible in at least one frame
            Assert.True(seenFirstCharInAnyFrame, "First typed character not visible within TextArea in any recorded frame");

            input.Stop();
    }

    private static string ExtractTextAreaInnerBottomMost(MockTerminal terminal, int rows, int cols, int scanLines)
    {
        // Find all top borders, pick the bottom-most matching a box of expected height
        int bestTopY = -1;
        for (int y = 0; y < Math.Min(scanLines - (rows + 1), terminal.Height - (rows + 1)); y++)
        {
            var line = terminal.GetLine(y) ?? string.Empty;
            var left = line.IndexOf('┌');
            var right = line.LastIndexOf('┐');
            if (left >= 0 && right > left)
            {
                var bottom = terminal.GetLine(y + rows + 1) ?? string.Empty;
                if (bottom.IndexOf('└') >= 0 && bottom.LastIndexOf('┘') > bottom.IndexOf('└'))
                {
                    bestTopY = Math.Max(bestTopY, y);
                }
            }
        }
        if (bestTopY < 0) return string.Empty;
        var result = string.Empty;
        for (int i = 1; i <= rows; i++)
        {
            var li = terminal.GetLine(bestTopY + i) ?? string.Empty;
            var l2 = li.IndexOf('│');
            var r2 = li.LastIndexOf('│');
            if (l2 >= 0 && r2 > l2) result += li.Substring(l2 + 1, r2 - l2 - 1) + "\n";
        }
        return result;
    }

    private static bool FocusTextAreaByTypingProbe(MockTerminal terminal, TestInputHandler input, int maxTabs, int scanLines, int rows, int cols)
    {
        for (int i = 0; i <= maxTabs; i++)
        {
            input.EmitKey('X', ConsoleKey.X);
            Thread.Sleep(50);
            var area = ExtractTextAreaInnerBottomMost(terminal, rows, cols, scanLines);
            if (!string.IsNullOrEmpty(area) && area.Contains('X'))
            {
                input.EmitKey('\b', ConsoleKey.Backspace);
                Thread.Sleep(30);
                return true;
            }
            input.EmitKey('\b', ConsoleKey.Backspace);
            Thread.Sleep(20);
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
        }
        return false;
    }
}
