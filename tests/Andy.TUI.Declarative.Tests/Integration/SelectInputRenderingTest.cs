using System;
using System.Collections.Generic;
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
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Tests SelectInput rendering to detect the reported issues with multiple highlights
/// and partial backgrounds when navigating with arrow keys.
/// </summary>
public class SelectInputRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public SelectInputRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void SelectInput_Navigation_ShouldNotHaveMultipleHighlights()
    {
        using (BeginScenario("SelectInput Navigation Rendering"))
        {
            var recorder = new ScreenRecorder("SelectInput_Navigation", 80, 30);
            var terminal = new MockTerminal(80, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { "Red", "Green", "Blue", "Yellow", "Purple" };
            var selectedItem = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 2)
                    {
                        new Text("SelectInput Test").Title().Color(Color.Cyan),
                        
                        new VStack(spacing: 1)
                        {
                            new Text("Select a color:").Bold(),
                            new SelectInput<string>(
                                items,
                                new Binding<Optional<string>>(
                                    () => selectedItem,
                                    v => selectedItem = v,
                                    "SelectedItem"
                                ),
                                item => item,
                                visibleItems: 5,
                                placeholder: "Choose a color..."
                            )
                        },
                        
                        selectedItem.TryGetValue(out var item)
                            ? new Text($"Selected: {item}").Color(Color.Green)
                            : new Text("No color selected").Color(Color.DarkGray)
                    }
                }.WithPadding(new Andy.TUI.Layout.Spacing(2, 2, 2, 2));
            }

            renderingSystem.Initialize();
            
            var rendererThread = new Thread(() =>
            {
                try
                {
                    renderer.Run(BuildUI);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Renderer thread error");
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(200);
            
            // Record initial state
            recorder.RecordFrame(terminal, "Initial");
            
            // Focus the SelectInput
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After Tab - Focus SelectInput");
            
            // Navigate down through items and check for issues
            var highlightIssues = new List<string>();
            
            for (int i = 1; i <= 5; i++)
            {
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(50);
                recorder.RecordFrame(terminal, $"After DownArrow {i}");
                
                // Check for multiple highlights or partial backgrounds
                var issue = CheckForHighlightIssues(terminal, i);
                if (issue != null)
                {
                    highlightIssues.Add(issue);
                    _output.WriteLine($"Frame {i}: {issue}");
                }
            }
            
            // Navigate up and check
            for (int i = 1; i <= 3; i++)
            {
                input.EmitKey('\0', ConsoleKey.UpArrow);
                Thread.Sleep(50);
                recorder.RecordFrame(terminal, $"After UpArrow {i}");
                
                var issue = CheckForHighlightIssues(terminal, 5 + i);
                if (issue != null)
                {
                    highlightIssues.Add(issue);
                    _output.WriteLine($"Frame {5 + i}: {issue}");
                }
            }
            
            // Save recording
            var recordingPath = recorder.SaveToFile();
            _output.WriteLine($"\n📹 Recording saved to: {recordingPath}");
            
            // Analyze recording for issues
            var report = recorder.Analyze();
            _output.WriteLine($"\n📊 Analysis Report:");
            _output.WriteLine($"  Total frames: {report.TotalFrames}");
            _output.WriteLine($"  Issues found: {report.Issues.Count}");
            
            foreach (var issue in report.Issues)
            {
                _output.WriteLine($"  - {issue.Type}: {issue.Description}");
            }
            
            // Check specific issues
            if (highlightIssues.Any())
            {
                _output.WriteLine($"\n❌ Found {highlightIssues.Count} highlight rendering issues:");
                foreach (var issue in highlightIssues.Take(5))
                {
                    _output.WriteLine($"  - {issue}");
                }
                
                // This assertion will fail to show the issues
                Assert.Fail($"SelectInput has rendering issues: {string.Join("; ", highlightIssues.Take(3))}");
            }
            
            input.Stop();
        }
    }

    private string? CheckForHighlightIssues(MockTerminal terminal, int frameNumber)
    {
        // Count lines with highlight-like backgrounds
        var highlightedLines = new List<int>();
        var partialHighlights = new List<(int line, int startX, int endX)>();
        
        for (int y = 0; y < terminal.Height; y++)
        {
            var lineHighlights = new List<(int start, int end, ConsoleColor bg)>();
            ConsoleColor? currentBg = null;
            int highlightStart = -1;
            
            for (int x = 0; x < terminal.Width; x++)
            {
                var (ch, fg, bg) = terminal.GetCharAt(x, y);
                
                // Track background color changes
                if (bg != ConsoleColor.Black && bg != ConsoleColor.DarkGray)
                {
                    if (currentBg == null)
                    {
                        currentBg = bg;
                        highlightStart = x;
                    }
                    else if (bg != currentBg)
                    {
                        // Background color changed mid-line
                        lineHighlights.Add((highlightStart, x - 1, currentBg.Value));
                        currentBg = bg;
                        highlightStart = x;
                    }
                }
                else if (currentBg != null)
                {
                    // End of highlight
                    lineHighlights.Add((highlightStart, x - 1, currentBg.Value));
                    currentBg = null;
                    highlightStart = -1;
                }
            }
            
            // Check if line ended with highlight
            if (currentBg != null)
            {
                lineHighlights.Add((highlightStart, terminal.Width - 1, currentBg.Value));
            }
            
            // Analyze line highlights
            if (lineHighlights.Any())
            {
                highlightedLines.Add(y);
                
                // Check for partial highlights (not spanning expected item width)
                foreach (var (start, end, bg) in lineHighlights)
                {
                    var width = end - start + 1;
                    
                    // If highlight doesn't span at least 20 characters (typical item width),
                    // it might be a partial highlight issue
                    if (width < 20 && width > 1)
                    {
                        partialHighlights.Add((y, start, end));
                    }
                }
            }
        }
        
        // Detect issues
        var issues = new List<string>();
        
        if (highlightedLines.Count > 1)
        {
            issues.Add($"Multiple highlights detected on lines: {string.Join(", ", highlightedLines)}");
        }
        
        if (partialHighlights.Any())
        {
            var partial = partialHighlights.First();
            issues.Add($"Partial background on line {partial.line} from x={partial.startX} to x={partial.endX}");
        }
        
        // Check for scattered highlights (non-consecutive lines)
        if (highlightedLines.Count > 1)
        {
            for (int i = 1; i < highlightedLines.Count; i++)
            {
                if (highlightedLines[i] - highlightedLines[i - 1] > 1)
                {
                    issues.Add($"Non-consecutive highlights on lines {highlightedLines[i - 1]} and {highlightedLines[i]}");
                }
            }
        }
        
        return issues.Any() ? string.Join("; ", issues) : null;
    }

    [Fact]
    public void SelectInput_ColorConsistency_Test()
    {
        using (BeginScenario("SelectInput Color Consistency"))
        {
            var terminal = new MockTerminal(80, 20);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var colors = new[] { "Red", "Green", "Blue" };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new SelectInput<string>(
                    colors,
                    new Binding<Optional<string>>(() => selected, v => selected = v, "Selected"),
                    c => c,
                    visibleItems: 3,
                    placeholder: "Pick one"
                );
            }

            renderingSystem.Initialize();
            
            var rendererThread = new Thread(() =>
            {
                try { renderer.Run(BuildUI); }
                catch (Exception ex) { Logger.Error(ex, "Renderer error"); }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(100);
            
            // Focus and open dropdown
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            // Capture colors for each navigation step
            var colorSnapshots = new List<string>();
            
            for (int i = 0; i < 3; i++)
            {
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(50);
                
                var snapshot = CaptureColorDistribution(terminal);
                colorSnapshots.Add($"Step {i + 1}: {snapshot}");
                _output.WriteLine(snapshot);
            }
            
            // Check for color consistency issues
            var uniquePatterns = colorSnapshots.Distinct().Count();
            if (uniquePatterns < colorSnapshots.Count)
            {
                _output.WriteLine($"⚠️ Color inconsistency detected: {uniquePatterns} unique patterns in {colorSnapshots.Count} steps");
            }
            
            input.Stop();
        }
    }

    private string CaptureColorDistribution(MockTerminal terminal)
    {
        var colorCounts = new Dictionary<string, int>();
        
        for (int y = 0; y < terminal.Height; y++)
        {
            for (int x = 0; x < terminal.Width; x++)
            {
                var (ch, fg, bg) = terminal.GetCharAt(x, y);
                if (bg != ConsoleColor.Black)
                {
                    var key = $"{bg}";
                    colorCounts[key] = colorCounts.GetValueOrDefault(key) + 1;
                }
            }
        }
        
        return string.Join(", ", colorCounts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}"));
    }
}