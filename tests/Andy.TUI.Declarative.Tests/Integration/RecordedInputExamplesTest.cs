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
/// Tests that record the actual Input examples to capture all rendering issues.
/// This simulates real usage of the examples and records frame-by-frame.
/// </summary>
public class RecordedInputExamplesTest : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _debugLogs = new();

    public RecordedInputExamplesTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
        Andy.TUI.Diagnostics.Logger.LogMessageWritten += OnLogMessage;
    }

    private void OnLogMessage(object? sender, LogMessageEventArgs e)
    {
        _debugLogs.Add($"[{e.Level}] {e.Message}");
    }

    [Fact]
    public void RecordedTest_MultiSelectInputExample()
    {
        using (BeginScenario("Recorded MultiSelectInput Example"))
        {
            var recorder = new ScreenRecorder("MultiSelectInputExample", 100, 40);
            
            LogStep("Setting up MultiSelectInput example");
            
            var terminal = new MockTerminal(100, 40);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            // Example similar to Input example with MultiSelectInput
            var options = new[] { 
                "Option A", 
                "Option B", 
                "Option C", 
                "Option D", 
                "Option E",
                "Option F",
                "Option G" 
            };
            var selectedOptions = new HashSet<string>();
            var textInput = "";
            var singleSelect = Optional<string>.None;

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 1)
                    {
                        new Text("Multi-Component Form Test").Bold().Color(Color.Cyan),
                        
                        new Text("Text Input:"),
                        new TextField("Enter text...", new Binding<string>(() => textInput, v => textInput = v)),
                        
                        new Text("Single Select:"),
                        new SelectInput<string>(
                            new[] { "Red", "Green", "Blue" },
                            new Binding<Optional<string>>(
                                () => singleSelect,
                                v => singleSelect = v,
                                "SingleSelect"
                            ),
                            visibleItems: 3
                        ).Placeholder("Choose color..."),
                        
                        new Text("Multi Select (Space to toggle):"),
                        new MultiSelectInput<string>(
                            options,
                            new Binding<ISet<string>>(
                                () => selectedOptions,
                                v => selectedOptions = new HashSet<string>(v),
                                "MultiSelect"
                            )
                        ),
                        
                        new Text($"Selected: {string.Join(", ", selectedOptions)}").Color(Color.Yellow),
                        
                        new HStack(spacing: 2)
                        {
                            new Button("Submit", () => { }).Primary(),
                            new Button("Clear", () => { selectedOptions.Clear(); }),
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
            
            // Record initial state
            recorder.RecordFrame(terminal, "Initial");
            RecordLogs(recorder);
            
            // Navigate through components
            var steps = new (string action, ConsoleKey key, char ch)[]
            {
                ("Tab to TextField", ConsoleKey.Tab, '\t'),
                ("Type 'Hello'", ConsoleKey.H, 'H'),
                ("Continue typing", ConsoleKey.E, 'e'),
                ("Continue typing", ConsoleKey.L, 'l'),
                ("Continue typing", ConsoleKey.L, 'l'),
                ("Continue typing", ConsoleKey.O, 'o'),
                ("Tab to SingleSelect", ConsoleKey.Tab, '\t'),
                ("Open dropdown", ConsoleKey.DownArrow, '\0'),
                ("Select Green", ConsoleKey.DownArrow, '\0'),
                ("Tab to MultiSelect", ConsoleKey.Tab, '\t'),
                ("Toggle Option A", ConsoleKey.Spacebar, ' '),
                ("Navigate down", ConsoleKey.DownArrow, '\0'),
                ("Toggle Option B", ConsoleKey.Spacebar, ' '),
                ("Navigate down", ConsoleKey.DownArrow, '\0'),
                ("Navigate down", ConsoleKey.DownArrow, '\0'),
                ("Toggle Option D", ConsoleKey.Spacebar, ' '),
                ("Navigate up", ConsoleKey.UpArrow, '\0'),
                ("Navigate up", ConsoleKey.UpArrow, '\0'),
                ("Tab to Submit button", ConsoleKey.Tab, '\t'),
                ("Tab to Clear button", ConsoleKey.Tab, '\t'),
                ("Tab to Cancel button", ConsoleKey.Tab, '\t'),
                ("Tab back to TextField", ConsoleKey.Tab, '\t'),
            };
            
            foreach (var (action, key, ch) in steps)
            {
                LogStep(action);
                input.EmitKey(ch, key);
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
            ReportIssues(report);
        }
    }

    [Fact]
    public void RecordedTest_RealInputExample11()
    {
        using (BeginScenario("Recorded Real Input Example 11"))
        {
            var recorder = new ScreenRecorder("InputExample11", 120, 50);
            
            LogStep("Setting up exact Input Example 11");
            
            var terminal = new MockTerminal(120, 50);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            // Exact setup from Example 11
            var countries = new[]
            {
                new Country { Code = "US", Name = "United States", Population = 331_000_000 },
                new Country { Code = "CN", Name = "China", Population = 1_412_000_000 },
                new Country { Code = "IN", Name = "India", Population = 1_380_000_000 },
                new Country { Code = "ID", Name = "Indonesia", Population = 273_000_000 },
                new Country { Code = "PK", Name = "Pakistan", Population = 225_000_000 },
                new Country { Code = "BR", Name = "Brazil", Population = 213_000_000 },
                new Country { Code = "NG", Name = "Nigeria", Population = 211_000_000 },
                new Country { Code = "BD", Name = "Bangladesh", Population = 169_000_000 },
                new Country { Code = "RU", Name = "Russia", Population = 146_000_000 },
                new Country { Code = "MX", Name = "Mexico", Population = 128_000_000 }
            };
            
            var selectedCountry = Optional<Country>.None;
            var selectedLanguages = new HashSet<string>();
            var languages = new[] { "English", "Spanish", "French", "German", "Italian", "Portuguese", "Chinese", "Japanese", "Korean", "Arabic" };

            ISimpleComponent BuildUI()
            {
                return new Box
                {
                    new VStack(spacing: 2)
                    {
                        new Text("SelectInput Test - Example 11").Title().Color(Color.Cyan),
                        
                        new VStack(spacing: 1)
                        {
                            new Text("Select your country:").Bold(),
                            new SelectInput<Country>(
                                countries,
                                new Binding<Optional<Country>>(
                                    () => selectedCountry,
                                    v => selectedCountry = v,
                                    "SelectedCountry"
                                ),
                                country => $"{country.Code} - {country.Name} (Pop: {country.Population:N0})",
                                visibleItems: 5,
                                placeholder: "Choose a country..."
                            )
                        },
                        
                        selectedCountry.TryGetValue(out var country)
                            ? new Text($"Selected: {country.Name} with population {country.Population:N0}").Color(Color.Green)
                            : new Text("No country selected").Color(Color.DarkGray),
                        
                        new VStack(spacing: 1)
                        {
                            new Text("Select languages you speak:").Bold(),
                            new MultiSelectInput<string>(
                                languages,
                                new Binding<ISet<string>>(
                                    () => selectedLanguages,
                                    v => selectedLanguages = new HashSet<string>(v),
                                    "SelectedLanguages"
                                )
                            )
                        },
                        
                        selectedLanguages.Any()
                            ? new Text($"Languages: {string.Join(", ", selectedLanguages)}").Color(Color.Green)
                            : new Text("No languages selected").Color(Color.DarkGray),
                        
                        new HStack(spacing: 2)
                        {
                            new Button("Submit", () => { }).Primary(),
                            new Button("Reset", () => 
                            { 
                                selectedCountry = Optional<Country>.None;
                                selectedLanguages.Clear();
                            }),
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
            
            // Simulate real user interaction with Example 11
            var navigationSteps = new (string action, ConsoleKey key, char ch, int delay)[]
            {
                ("Tab to Country SelectInput", ConsoleKey.Tab, '\t', 100),
                ("Navigate down in countries", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate down in countries", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate down in countries", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate down in countries", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate down in countries", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate up in countries", ConsoleKey.UpArrow, '\0', 50),
                ("Select country", ConsoleKey.Enter, '\r', 100),
                ("Tab to Languages MultiSelect", ConsoleKey.Tab, '\t', 100),
                ("Toggle English", ConsoleKey.Spacebar, ' ', 50),
                ("Navigate down", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate down", ConsoleKey.DownArrow, '\0', 50),
                ("Toggle French", ConsoleKey.Spacebar, ' ', 50),
                ("Navigate down", ConsoleKey.DownArrow, '\0', 50),
                ("Navigate down", ConsoleKey.DownArrow, '\0', 50),
                ("Toggle Italian", ConsoleKey.Spacebar, ' ', 50),
                ("Page down", ConsoleKey.PageDown, '\0', 100),
                ("Toggle Korean", ConsoleKey.Spacebar, ' ', 50),
                ("Page up", ConsoleKey.PageUp, '\0', 100),
                ("Tab to Submit", ConsoleKey.Tab, '\t', 100),
                ("Tab to Reset", ConsoleKey.Tab, '\t', 100),
                ("Press Reset", ConsoleKey.Enter, '\r', 100),
                ("Tab back to Country", ConsoleKey.Tab, '\t', 100),
                ("Tab back to Country again", ConsoleKey.Tab, '\t', 100),
            };
            
            foreach (var (action, key, ch, delay) in navigationSteps)
            {
                LogStep(action);
                input.EmitKey(ch, key);
                Thread.Sleep(delay);
                recorder.RecordFrame(terminal, action);
                RecordLogs(recorder);
            }
            
            input.Stop();
            
            // Save and analyze
            var recordingFile = recorder.SaveToFile();
            _output.WriteLine($"\n📹 Recording saved to: {recordingFile}");
            _output.WriteLine($"📄 Summary saved to: {Path.ChangeExtension(recordingFile, ".txt")}");
            
            var report = recorder.Analyze();
            ReportIssues(report);
        }
    }

    private void ReportIssues(AnalysisReport report)
    {
        _output.WriteLine($"\n📊 Analysis Summary:");
        _output.WriteLine($"  Total frames recorded: {report.TotalFrames}");
        _output.WriteLine($"  Total debug logs: {report.TotalLogs}");
        _output.WriteLine($"  Total issues found: {report.Issues.Count}");
        
        // Group issues by type
        var issuesByType = report.Issues.GroupBy(i => i.Type).OrderByDescending(g => g.Count());
        
        _output.WriteLine($"\n⚠️ Issues by Type:");
        foreach (var group in issuesByType)
        {
            _output.WriteLine($"\n  {group.Key}: {group.Count()} occurrences");
            
            // Show severity breakdown
            var bySeverity = group.GroupBy(i => i.Severity);
            foreach (var sev in bySeverity)
            {
                _output.WriteLine($"    [{sev.Key}]: {sev.Count()}");
            }
            
            // Show first few examples
            foreach (var issue in group.Take(2))
            {
                _output.WriteLine($"    Frame {issue.FrameNumber}: {issue.Description}");
                if (issue.Details?.Any() == true)
                {
                    foreach (var detail in issue.Details.Take(2))
                    {
                        _output.WriteLine($"      - {detail}");
                    }
                }
            }
        }
        
        // Report critical color issues specifically
        var colorIssues = report.Issues.Where(i => 
            i.Type == "MultipleHighlightedLines" || 
            i.Type == "PartialHighlight" ||
            i.Type == "MixedBackgroundColors" ||
            i.Type == "InvisibleText" ||
            i.Type == "PoorContrast").ToList();
            
        if (colorIssues.Any())
        {
            _output.WriteLine($"\n🎨 Color Rendering Issues: {colorIssues.Count}");
            foreach (var issue in colorIssues.Take(5))
            {
                _output.WriteLine($"  Frame {issue.FrameNumber} [{issue.Severity}]: {issue.Description}");
            }
        }
        
        // Check for critical failures
        var criticalCount = report.Issues.Count(i => i.Severity == "High");
        if (criticalCount > 0)
        {
            _output.WriteLine($"\n❌ {criticalCount} CRITICAL issues detected!");
            Assert.Fail($"Found {criticalCount} high severity rendering issues. Check recording for details.");
        }
    }

    private void RecordLogs(ScreenRecorder recorder)
    {
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

    // Test data class for Example 11
    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public int Population { get; set; }
    }
}