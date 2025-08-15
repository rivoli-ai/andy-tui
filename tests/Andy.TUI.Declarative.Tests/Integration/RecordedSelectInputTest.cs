using System;
using System.Collections.Generic;
using System.IO;
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
/// Test that records every frame and analyzes for rendering issues.
/// This test captures exactly what happens at each step.
/// </summary>
public class RecordedSelectInputTest : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _debugLogs = new();

    public RecordedSelectInputTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
        
        // Hook into the logger to capture debug messages
        Andy.TUI.Diagnostics.Logger.LogMessageWritten += OnLogMessage;
    }

    private void OnLogMessage(object? sender, LogMessageEventArgs e)
    {
        _debugLogs.Add($"[{e.Level}] {e.Message}");
    }

    [Fact]
    public void RecordedTest_SelectInputNavigation()
    {
        using (BeginScenario("Recorded SelectInput Navigation"))
        {
            var recorder = new ScreenRecorder("SelectInputNavigation", 80, 30);
            
            LogStep("Setting up SelectInput with recording");
            
            var terminal = new MockTerminal(80, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var items = new[] { 
                "Apple", 
                "Banana", 
                "Cherry", 
                "Date", 
                "Elderberry",
                "Fig",
                "Grape"
            };
            var selected = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("Example 11 Simulation").Bold(),
                        new SelectInput<string>(
                            items,
                            new Binding<Optional<string>>(
                                () => selected,
                                v => selected = v,
                                "Selected"
                            ),
                            visibleItems: 5
                        ).Placeholder("Choose a fruit..."),
                        new Button("OK", () => { }),
                        new Button("Cancel", () => { })
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
            recorder.RecordFrame(terminal, "Initial render");
            RecordLogs(recorder);
            
            LogStep("Focus the SelectInput");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After Tab (focus SelectInput)");
            RecordLogs(recorder);
            
            // Navigate down multiple times
            for (int i = 0; i < 5; i++)
            {
                LogStep($"Navigate down {i + 1}");
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(100);
                recorder.RecordFrame(terminal, $"After DownArrow {i + 1}");
                RecordLogs(recorder);
            }
            
            // Navigate up
            LogStep("Navigate up");
            input.EmitKey('\0', ConsoleKey.UpArrow);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After UpArrow");
            RecordLogs(recorder);
            
            // Tab to button
            LogStep("Tab to button");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After Tab (to button)");
            RecordLogs(recorder);
            
            // Tab back to SelectInput
            LogStep("Tab back to SelectInput");
            input.EmitKey('\t', ConsoleKey.Tab);
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(100);
            recorder.RecordFrame(terminal, "After Tab back to SelectInput");
            RecordLogs(recorder);
            
            input.Stop();
            
            // Save recording and analyze
            var recordingFile = recorder.SaveToFile();
            _output.WriteLine($"\n📹 Recording saved to: {recordingFile}");
            _output.WriteLine($"📄 Summary saved to: {Path.ChangeExtension(recordingFile, ".txt")}");
            
            // Analyze the recording
            var report = recorder.Analyze();
            
            // Output issues found
            if (report.Issues.Any())
            {
                _output.WriteLine($"\n⚠️ Found {report.Issues.Count} rendering issues:");
                foreach (var issue in report.Issues)
                {
                    _output.WriteLine($"  - Frame {issue.FrameNumber}: [{issue.Severity}] {issue.Type}");
                    _output.WriteLine($"    {issue.Description}");
                    if (issue.Details?.Any() == true)
                    {
                        foreach (var detail in issue.Details)
                        {
                            _output.WriteLine($"      {detail}");
                        }
                    }
                }
            }
            else
            {
                _output.WriteLine("\n✅ No rendering issues detected");
            }
            
            // Assert no high-severity issues
            var highSeverityIssues = report.Issues.Where(i => i.Severity == "High").ToList();
            if (highSeverityIssues.Any())
            {
                var issueDescriptions = string.Join("\n", 
                    highSeverityIssues.Select(i => $"Frame {i.FrameNumber}: {i.Description}"));
                Assert.Fail($"High severity rendering issues found:\n{issueDescriptions}");
            }
        }
    }

    [Fact]
    public void RecordedTest_ComplexInteraction()
    {
        using (BeginScenario("Recorded Complex Interaction"))
        {
            var recorder = new ScreenRecorder("ComplexInteraction", 100, 40);
            
            LogStep("Setting up complex UI with multiple components");
            
            var terminal = new MockTerminal(100, 40);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var colors = new[] { "Red", "Green", "Blue", "Yellow" };
            var sizes = new[] { "Small", "Medium", "Large", "Extra Large" };
            var selectedColor = Optional<string>.None;
            var selectedSize = Optional<string>.None;
            var text = "";

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("Complex Form").Bold(),
                        
                        new Text("Name:"),
                        new TextField("", new Binding<string>(() => text, v => text = v)),
                        
                        new Text("Color:"),
                        new SelectInput<string>(
                            colors,
                            new Binding<Optional<string>>(
                                () => selectedColor,
                                v => selectedColor = v,
                                "Color"
                            ),
                            visibleItems: 3
                        ).Placeholder("Pick color..."),
                        
                        new Text("Size:"),
                        new SelectInput<string>(
                            sizes,
                            new Binding<Optional<string>>(
                                () => selectedSize,
                                v => selectedSize = v,
                                "Size"
                            ),
                            visibleItems: 3
                        ).Placeholder("Pick size..."),
                        
                        new HStack(spacing: 2)
                        {
                            new Button("Submit", () => { }),
                            new Button("Reset", () => { }),
                            new Button("Cancel", () => { })
                        }
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
            
            // Record initial
            recorder.RecordFrame(terminal, "Initial");
            RecordLogs(recorder);
            
            // Tab through all components
            var actions = new[]
            {
                ("Tab to TextField", ConsoleKey.Tab),
                ("Type 'Test'", ConsoleKey.T),
                ("Type 'e'", ConsoleKey.E),
                ("Type 's'", ConsoleKey.S),
                ("Type 't'", ConsoleKey.T),
                ("Tab to Color SelectInput", ConsoleKey.Tab),
                ("Down in Color", ConsoleKey.DownArrow),
                ("Down in Color", ConsoleKey.DownArrow),
                ("Tab to Size SelectInput", ConsoleKey.Tab),
                ("Down in Size", ConsoleKey.DownArrow),
                ("Tab to Submit button", ConsoleKey.Tab),
                ("Tab to Reset button", ConsoleKey.Tab),
                ("Tab to Cancel button", ConsoleKey.Tab),
                ("Tab back to TextField", ConsoleKey.Tab),
                ("Backspace", ConsoleKey.Backspace),
                ("Backspace", ConsoleKey.Backspace)
            };
            
            foreach (var (action, key) in actions)
            {
                LogStep(action);
                
                if (key == ConsoleKey.T)
                    input.EmitKey('T', key);
                else if (key == ConsoleKey.E)
                    input.EmitKey('e', key);
                else if (key == ConsoleKey.S)
                    input.EmitKey('s', key);
                else if (key == ConsoleKey.Backspace)
                    input.EmitKey('\b', key);
                else if (key == ConsoleKey.Tab)
                    input.EmitKey('\t', key);
                else
                    input.EmitKey('\0', key);
                    
                Thread.Sleep(100);
                recorder.RecordFrame(terminal, action);
                RecordLogs(recorder);
            }
            
            input.Stop();
            
            // Save and analyze
            var recordingFile = recorder.SaveToFile();
            _output.WriteLine($"\n📹 Recording saved to: {recordingFile}");
            _output.WriteLine($"📄 Summary saved to: {Path.ChangeExtension(recordingFile, ".txt")}");
            
            var report = recorder.Analyze();
            
            // Report findings
            _output.WriteLine($"\nAnalysis Summary:");
            _output.WriteLine($"  Total frames: {report.TotalFrames}");
            _output.WriteLine($"  Total logs captured: {report.TotalLogs}");
            _output.WriteLine($"  Issues found: {report.Issues.Count}");
            
            foreach (var issue in report.Issues.GroupBy(i => i.Type))
            {
                _output.WriteLine($"\n  {issue.Key}: {issue.Count()} occurrences");
                foreach (var instance in issue.Take(3))
                {
                    _output.WriteLine($"    Frame {instance.FrameNumber}: {instance.Description}");
                }
                if (issue.Count() > 3)
                {
                    _output.WriteLine($"    ... and {issue.Count() - 3} more");
                }
            }
            
            // Check for critical issues
            var criticalIssues = report.Issues.Where(i => 
                i.Type == "TextDisappeared" || 
                i.Type == "MultipleHighlights" ||
                i.Type == "NullCharacter").ToList();
                
            if (criticalIssues.Any())
            {
                _output.WriteLine("\n❌ Critical issues detected!");
                Assert.Fail($"Found {criticalIssues.Count} critical rendering issues. Check recording at: {recordingFile}");
            }
        }
    }

    private void RecordLogs(ScreenRecorder recorder)
    {
        // Capture any debug logs since last recording
        foreach (var log in _debugLogs)
        {
            recorder.RecordLog("DEBUG", log);
        }
        _debugLogs.Clear();
    }

    public override void Dispose()
    {
        Andy.TUI.Diagnostics.Logger.LogMessageWritten -= OnLogMessage;
        base.Dispose();
    }
}