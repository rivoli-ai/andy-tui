using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;

namespace Andy.TUI.Declarative.Tests.Integration;

public class TextAreaStabilityTests : TestBase
{
    private readonly ITestOutputHelper _output;

    public TextAreaStabilityTests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void TextArea_Multiline_Typing_ShouldNotFlicker()
    {
        using (BeginScenario("TextArea Multiline Stability"))
        {
            var terminal = new MockTerminal(80, 20);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: true);

            string text = string.Empty;
            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1)
                {
                    new Text("Editor"),
                    new TextArea("Type here...", new Binding<string>(() => text, v => text = v, "TextAreaBinding"), rows: 3, cols: 12)
                };
            }

            renderingSystem.Initialize();

            var renderThread = new Thread(() =>
            {
                try { renderer.Run(BuildUI); }
                catch (Exception ex) { Logger.Error(ex, "Renderer thread error"); }
            }) { IsBackground = true };
            renderThread.Start();

            Thread.Sleep(150);

            // Type enough to wrap to multiple lines
            var toType = "Hello world again"; // wraps at cols=12
            var snapshots = new List<string>();

            foreach (var ch in toType)
            {
                input.EmitKey(ch, ConsoleKey.A);
                Thread.Sleep(20);
                var buf = GetTopBuffer(terminal, 8);
                snapshots.Add(buf);
                // The first character should never disappear once present
                Assert.Contains("H", snapshots[^1]);
            }

            // Final content should include full text (across lines)
            var finalBuf = GetTopBuffer(terminal, 8);
            _output.WriteLine("\n=== Final Buffer ===\n" + finalBuf + "\n====\n");
            Assert.Contains("Hello", finalBuf);
            Assert.Contains("world", finalBuf);

            input.Stop();
        }
    }

    [Fact]
    public void ChatLike_TextArea_Typing_Send_ShouldBeStable()
    {
        using (BeginScenario("Chat-like stability with TextArea"))
        {
            var terminal = new MockTerminal(120, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: true);
            var recorder = new ScreenRecorder("ChatLike_TextArea", 120, 30);

            var messages = new List<(string role, string content)>();
            string draft = string.Empty;

            ISimpleComponent BuildUI()
            {
                // Conversation
                var convo = new VStack(spacing: 0);
                foreach (var (role, content) in messages)
                {
                    if (role == "user")
                        convo.Add(new HStack(spacing: 1) { new Text("You:").Bold().Color(Color.Cyan), new Text(content) });
                    else
                        convo.Add(new HStack(spacing: 1) { new Text("Model:").Bold().Color(Color.Yellow), new Text(content) });
                }

                // Input area and a Send button (button used to simulate end-to-end send)
                return new VStack(spacing: 1)
                {
                    new Text("Andy.TUI Chat — Test").Bold(),
                    convo,
                    new VStack(spacing: 1)
                    {
                        new TextArea("Type a message...", new Binding<string>(() => draft, v => draft = v, "DraftBinding"), rows: 3, cols: 40),
                        new Button("Send", () =>
                        {
                            if (!string.IsNullOrWhiteSpace(draft))
                            {
                                messages.Add(("user", draft));
                                draft = string.Empty;
                                messages.Add(("assistant", "OK"));
                                renderer.RequestRender();
                            }
                        })
                    }
                };
            }

            renderingSystem.Initialize();
            var renderThread = new Thread(() => { try { renderer.Run(BuildUI); } catch (Exception ex) { Logger.Error(ex, "Renderer thread error"); } }) { IsBackground = true };
            renderThread.Start();

            Thread.Sleep(150);
            recorder.RecordFrame(terminal, "Initial");

            // Ensure focus is on the TextArea by probing typed char visibility in its content region
            Assert.True(FocusTextAreaByTypingProbe(terminal, input, maxTabs: 8, scanLines: 30, rows: 3, cols: 40), "Failed to focus TextArea");

            // Type characters and ensure they persist across frames
            var msg = "This is stable";
            var typed = string.Empty;
            foreach (var ch in msg)
            {
                input.EmitKey(ch, ConsoleKey.T);
                typed += ch;
                Thread.Sleep(50);
                var buf = GetTopBuffer(terminal, 30);
                _output.WriteLine($"After '{ch}' buffer:\n" + buf);

                // Extract just the TextArea content region (rows: 3, cols: 40) and assert the typed text persists there
                var area = TryGetTextAreaContent(terminal, scanLines: 30, rows: 3, cols: 40);
                _output.WriteLine("TextArea content:\n" + area);
                Assert.Contains(typed[0].ToString(), area);

                recorder.RecordFrame(terminal, $"Type '{ch}'");
            }

            // Try to focus the Send button by cycling TAB and pressing Enter until messages are added
            int attempts = 0;
            while (attempts++ < 6 && messages.Count < 2)
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(60);
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(120);
                recorder.RecordFrame(terminal, "Attempt Send");
            }

            var finalBuf = GetTopBuffer(terminal, 20);
            _output.WriteLine("\n=== Chat Final Buffer ===\n" + finalBuf + "\n====\n");
            Assert.True(messages.Count >= 2, "Send action did not add messages");
            Assert.Contains("You:", finalBuf);
            Assert.Contains("Model:", finalBuf);

            // Ensure the TextArea region is cleared (draft removed)
            var finalArea = TryGetTextAreaContent(terminal, scanLines: 30, rows: 3, cols: 40);
            Assert.DoesNotContain("This is stable", finalArea);

            // Save recording for debugging
            var path = recorder.SaveToFile();
            _output.WriteLine($"Recording saved: {path}");

            input.Stop();
        }
    }

    private static string GetTopBuffer(MockTerminal terminal, int lines)
    {
        var content = string.Empty;
        for (int y = 0; y < Math.Min(lines, terminal.Height); y++)
        {
            var line = terminal.GetLine(y);
            if (!string.IsNullOrEmpty(line)) content += line + "\n";
        }
        return content;
    }

    private static bool FocusTextAreaByTypingProbe(MockTerminal terminal, TestInputHandler input, int maxTabs, int scanLines, int rows, int cols)
    {
        for (int i = 0; i <= maxTabs; i++)
        {
            // Type a probe character and see if it appears inside the detected TextArea region
            input.EmitKey('X', ConsoleKey.X);
            Thread.Sleep(30);
            var area = TryGetTextAreaContent(terminal, scanLines, rows, cols);
            if (!string.IsNullOrEmpty(area) && area.Contains('X'))
            {
                // Clean up probe
                input.EmitKey('\b', ConsoleKey.Backspace);
                Thread.Sleep(20);
                return true;
            }

            // Clean and move focus forward
            input.EmitKey('\b', ConsoleKey.Backspace);
            Thread.Sleep(10);
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(30);
        }
        return false;
    }

    private static string TryGetTextAreaContent(MockTerminal terminal, int scanLines, int rows, int cols)
    {
        // Heuristic: find first top border like "┌───...┐", then capture next `rows` lines between vertical borders
        int topY = -1;
        for (int y = 0; y < Math.Min(scanLines, terminal.Height); y++)
        {
            var line = terminal.GetLine(y) ?? string.Empty;
            if (line.Contains('┌') && line.Contains('┐'))
            {
                // Ensure there is a horizontal run of at least `cols` dashes between
                var left = line.IndexOf('┌');
                var right = line.IndexOf('┐', left + 1);
                if (right > left)
                {
                    var middle = line.Substring(left + 1, right - left - 1);
                    if (middle.Length >= cols - 1) { topY = y; break; }
                }
            }
        }

        if (topY < 0) return string.Empty;

        var result = string.Empty;
        for (int i = 1; i <= rows; i++)
        {
            var line = terminal.GetLine(topY + i) ?? string.Empty;
            var first = line.IndexOf('│');
            var last = line.LastIndexOf('│');
            if (first >= 0 && last > first)
            {
                result += line.Substring(first + 1, last - first - 1) + "\n";
            }
        }
        return result;
    }
}
