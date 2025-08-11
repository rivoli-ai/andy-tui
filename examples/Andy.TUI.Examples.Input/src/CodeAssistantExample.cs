using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Input;

public class CodeAssistantExample
{
    private readonly string _scenarioPath;
    private int _stepIndex = 0;
    private List<ScenarioStep> _steps = new();

    // UI state
    private string _status = "Ready";
    private string _input = string.Empty;
    private int _scroll = 0;

    private const int UI_WIDTH = 120;
    private const int PANE_WIDTH = 58;
    private const int PANE_HEIGHT = 14;

    public CodeAssistantExample(string scenarioPath)
    {
        _scenarioPath = scenarioPath;
        LoadScenario();
    }

    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        renderingSystem.Initialize();

        // Background simulation thread to advance scenario & simulate typing in TextArea
        var thread = new Thread(() =>
        {
            while (_stepIndex < _steps.Count)
            {
                Thread.Sleep(_steps[_stepIndex].DelayMs);
                _status = _steps[_stepIndex].Status ?? _status;

                // Simulate typing for user prompts if provided
                var user = _steps[_stepIndex].UserInput;
                if (!string.IsNullOrEmpty(user))
                {
                    _input = string.Empty;
                    foreach (var ch in user!)
                    {
                        _input += ch;
                        _status = "Typing…";
                        Thread.Sleep(30);
                    }
                    _status = "Ready";
                }
                _stepIndex++;
            }
        })
        { IsBackground = true };
        thread.Start();

        renderer.Run(BuildUI);
    }

    private ISimpleComponent BuildUI()
    {
        var bannerLines = new[]
        {
            "╔══════════════════════════════════════════════════════════════╗",
            "║  Andy.TUI Code Assistant  —  agentic CLI with tools & diffs  ║",
            "╚══════════════════════════════════════════════════════════════╝",
        };

        var helpLines = new[]
        {
            "help  Show commands · tools  List tools · run <cmd>  Execute tool",
            "send  Send prompt · diff  Show latest diff · quit  Exit",
        };

        var step = _steps.ElementAtOrDefault(Math.Clamp(_stepIndex - 1, 0, Math.Max(0, _steps.Count - 1)));
        var convo = step?.Conversation ?? new List<string>();
        var diffs = step?.DiffLines ?? new List<string>();

        // Scrolling: show last N lines based on height (approx 12 without full layout calc)
        int maxConvo = 12;
        var visible = convo.Skip(Math.Max(0, convo.Count - maxConvo - _scroll)).Take(maxConvo).ToList();

        return new VStack(spacing: 1)
        {
            // Header area (fixed height to avoid overlap)
            new Box
            {
                new VStack(spacing: 0)
                {
                    new Text(Pad(bannerLines[0], UI_WIDTH)).Color(Color.Cyan),
                    new Text(Pad(bannerLines[1], UI_WIDTH)).Color(Color.Cyan),
                    new Text(Pad(bannerLines[2], UI_WIDTH)).Color(Color.Cyan),
                    new Text(Pad("Minimal agentic loop simulation with tools, LLM calls, and code diffs", UI_WIDTH)).Color(Color.DarkGray),
                    new Text(Pad(helpLines[0], UI_WIDTH)).Color(Color.Gray),
                    new Text(Pad(helpLines[1], UI_WIDTH)).Color(Color.Gray)
                }
            }
            .WithWidth(UI_WIDTH)
            .WithHeight(8),

            " ",

            // Middle content: conversation + diff side by side
            new HStack(spacing: 4)
            {
                new VStack(spacing: 0)
                {
                    new Text("Conversation").Bold().Color(Color.Yellow),
                    new Box { BuildPaddedLines(visible, PANE_WIDTH - 2, PANE_HEIGHT, fixedColor: Color.White) }
                        .WithPadding(new Spacing(1,1,1,1))
                        .WithWidth(PANE_WIDTH)
                        .WithHeight(PANE_HEIGHT)
                },
                new VStack(spacing: 0)
                {
                    new Text("Latest diff").Bold().Color(Color.Yellow),
                    new Box { BuildPaddedLines(diffs, PANE_WIDTH - 2, PANE_HEIGHT, colorizeDiff: true) }
                        .WithPadding(new Spacing(1,1,1,1))
                        .WithWidth(PANE_WIDTH)
                        .WithHeight(PANE_HEIGHT)
                }
            },

            " ",

            // Printing TextArea with simulated typing
            new Text("Message (auto-typed):").Color(Color.Gray),
            new TextArea("", new Binding<string>(() => _input, v => _input = v)).Rows(3).Cols(UI_WIDTH),

            // Status bar
            new Box { new Text(Pad($"Status: {_status}", UI_WIDTH)).Color(Color.DarkGray) }
                .WithWidth(UI_WIDTH)
                .WithHeight(1)
        };
    }

    private Color LineColor(string line)
    {
        if (line.StartsWith("+")) return Color.Green;
        if (line.StartsWith("-")) return Color.Red;
        if (line.StartsWith("@@")) return Color.Cyan;
        return Color.Gray;
    }

    private VStack BuildPaddedLines(IEnumerable<string> lines, int width, int height, Color? fixedColor = null, bool colorizeDiff = false)
    {
        var v = new VStack(spacing: 0);
        // Take last 'height' lines; pad to fixed width and exact height with blanks
        var lns = lines.ToList();
        var start = Math.Max(0, lns.Count - height);
        var slice = lns.Skip(start).Take(height).ToList();
        while (slice.Count < height) slice.Insert(0, "");
        foreach (var raw in slice)
        {
            var truncated = raw.Length > width ? raw.Substring(0, width) : raw;
            var padded = truncated.PadRight(width, ' ');
            var text = new Text(padded);
            if (colorizeDiff)
                v.Add(text.Color(LineColor(raw)));
            else if (fixedColor.HasValue)
                v.Add(text.Color(fixedColor.Value));
            else
                v.Add(text);
        }
        return v;
    }

    private string Pad(string text, int width)
    {
        if (text.Length >= width) return text.Substring(0, width);
        return text.PadRight(width, ' ');
    }

    private void LoadScenario()
    {
        if (!File.Exists(_scenarioPath))
        {
            // Default inline scenario
            _steps = new List<ScenarioStep>
            {
                new ScenarioStep
                {
                    DelayMs = 800,
                    Status = "Initializing tools…",
                    Conversation =
                    {
                        "assistant: Hello! I can run tests, apply diffs, and commit.",
                        "assistant: What would you like to do?"
                    }
                },
                new ScenarioStep
                {
                    DelayMs = 1000,
                    Status = "Running tests…",
                    Conversation =
                    {
                        "user: add tests for dropdown",
                        "assistant: running: dotnet test --filter Dropdown",
                        "assistant: 3 failed, 12 passed"
                    },
                    DiffLines =
                    {
                        "@@ tests/DropdownTests.cs @@",
                        "+ [Fact] Should_Show_All_Items_When_Navigating()",
                        "+ [Fact] Should_Close_On_Enter()"
                    }
                },
                new ScenarioStep
                {
                    DelayMs = 1200,
                    Status = "Applying edits…",
                    Conversation =
                    {
                        "assistant: generated diff and applied patch",
                        "assistant: re-running tests…",
                        "assistant: all tests passed"
                    },
                    DiffLines =
                    {
                        "@@ src/DropdownInstance.cs @@",
                        "+ if (_isOpen) DrawOverlayMenu();",
                        "- layout.Height = 1 + items.Count",
                        "+ layout.Height = 1"
                    }
                }
            };
            return;
        }

        var json = File.ReadAllText(_scenarioPath);
        var scenario = JsonSerializer.Deserialize<Scenario>(json, _jsonOptions) ?? new Scenario();
        _steps = scenario.Steps ?? new List<ScenarioStep>();
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private class Scenario
    {
        public List<ScenarioStep>? Steps { get; set; }
    }

    private class ScenarioStep
    {
        public int DelayMs { get; set; } = 1000;
        public string? Status { get; set; }
        public List<string> Conversation { get; set; } = new();
        public List<string> DiffLines { get; set; } = new();
        public string? UserInput { get; set; }
    }
}


