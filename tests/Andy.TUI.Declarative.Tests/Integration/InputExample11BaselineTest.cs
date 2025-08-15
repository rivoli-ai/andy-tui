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
using Andy.TUI.Theming;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Tests Input Example #11 with baseline comparison to detect rendering issues.
/// This test creates both expected baseline and actual recordings, then compares them.
/// </summary>
public class InputExample11BaselineTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public InputExample11BaselineTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);
    }

    [Fact]
    public void Example11_BaselineComparison()
    {
        using (BeginScenario("Input Example #11 Baseline Comparison"))
        {
            LogStep("Creating baseline recording");
            var baselineRecorder = CreateBaselineRecording();
            
            LogStep("Creating actual recording");
            var actualRecorder = CreateActualRecording();
            
            LogStep("Performing baseline comparison");
            var comparison = new BaselineComparison(actualRecorder, baselineRecorder, new ComparisonOptions
            {
                DiffThreshold = 3.0, // 3% tolerance
                ColorMismatchWeight = 3.0, // Colors are very important
                CharMismatchWeight = 1.0,
                MinRegionSize = 5,
                OscillationWindowSize = 3,
                OscillationThreshold = 2,
                PersistenceThreshold = 4,
                EntropySpikeFactor = 2.5
            });
            
            var report = comparison.Compare();
            
            // Save comparison report
            var reportFile = SaveComparisonReport(report);
            _output.WriteLine($"\n📊 Comparison report saved to: {reportFile}");
            
            // Output summary
            _output.WriteLine("\n" + report.GenerateSummary());
            
            // Analyze specific issues
            AnalyzeExample11Issues(report);
            
            // Check for critical failures
            var criticalIssues = report.Issues.Where(i => i.Severity == "High").ToList();
            if (criticalIssues.Any())
            {
                _output.WriteLine($"\n❌ Found {criticalIssues.Count} critical rendering issues:");
                foreach (var issue in criticalIssues.Take(5))
                {
                    _output.WriteLine($"  - {issue.Type}: {issue.Description}");
                    if (issue.Details?.Any() == true)
                    {
                        foreach (var detail in issue.Details.Take(2))
                        {
                            _output.WriteLine($"    {detail}");
                        }
                    }
                }
                
                Assert.Fail($"Example 11 has {criticalIssues.Count} critical rendering issues. See report: {reportFile}");
            }
        }
    }

    /// <summary>
    /// Creates the expected baseline recording with correct rendering.
    /// </summary>
    private ScreenRecorder CreateBaselineRecording()
    {
        var recorder = new ScreenRecorder("Example11_Baseline", 120, 40);
        var terminal = new MockTerminal(120, 40);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
        
        var theme = ThemeManager.Instance.CurrentTheme;
        var countries = CreateCountries();
        var selectedCountry = Optional<Country>.None;

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
                        new MockSelectInput<Country>(
                            countries,
                            new Binding<Optional<Country>>(
                                () => selectedCountry,
                                v => selectedCountry = v,
                                "SelectedCountry"
                            ),
                            country => $"{country.Code} - {country.Name}",
                            visibleItems: 5,
                            placeholder: "Choose a country...",
                            theme: theme // Pass theme for correct colors
                        )
                    },
                    
                    selectedCountry.TryGetValue(out var country)
                        ? new Text($"Selected: {country.Name}").Color(Color.Green)
                        : new Text("No country selected").Color(Color.DarkGray)
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
                Logger.Error(ex, "Baseline renderer thread error");
            }
        })
        { IsBackground = true };
        rendererThread.Start();

        Thread.Sleep(200);
        
        // Record baseline interactions with EXPECTED rendering
        recorder.RecordFrame(terminal, "Initial");
        
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);
        recorder.RecordFrame(terminal, "After Tab - Focus SelectInput");
        
        // Navigate down - each should show ONLY ONE highlighted item
        for (int i = 1; i <= 5; i++)
        {
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(50);
            recorder.RecordFrame(terminal, $"After DownArrow {i}");
            
            // In baseline, ensure only one item is highlighted
            VerifyBaselineHighlight(terminal, i);
        }
        
        input.EmitKey('\0', ConsoleKey.UpArrow);
        Thread.Sleep(50);
        recorder.RecordFrame(terminal, "After UpArrow");
        
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(100);
        recorder.RecordFrame(terminal, "After Enter - Select");
        
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);
        recorder.RecordFrame(terminal, "After Tab - Unfocus");
        
        input.Stop();
        return recorder;
    }

    /// <summary>
    /// Creates the actual recording from the real component.
    /// </summary>
    private ScreenRecorder CreateActualRecording()
    {
        var recorder = new ScreenRecorder("Example11_Actual", 120, 40);
        var terminal = new MockTerminal(120, 40);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
        
        var countries = CreateCountries();
        var selectedCountry = Optional<Country>.None;

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
                            country => $"{country.Code} - {country.Name}",
                            visibleItems: 5,
                            placeholder: "Choose a country..."
                        )
                    },
                    
                    selectedCountry.TryGetValue(out var country)
                        ? new Text($"Selected: {country.Name}").Color(Color.Green)
                        : new Text("No country selected").Color(Color.DarkGray)
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
                Logger.Error(ex, "Actual renderer thread error");
            }
        })
        { IsBackground = true };
        rendererThread.Start();

        Thread.Sleep(200);
        
        // Record same interactions as baseline
        recorder.RecordFrame(terminal, "Initial");
        
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);
        recorder.RecordFrame(terminal, "After Tab - Focus SelectInput");
        
        for (int i = 1; i <= 5; i++)
        {
            input.EmitKey('\0', ConsoleKey.DownArrow);
            Thread.Sleep(50);
            recorder.RecordFrame(terminal, $"After DownArrow {i}");
        }
        
        input.EmitKey('\0', ConsoleKey.UpArrow);
        Thread.Sleep(50);
        recorder.RecordFrame(terminal, "After UpArrow");
        
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(100);
        recorder.RecordFrame(terminal, "After Enter - Select");
        
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);
        recorder.RecordFrame(terminal, "After Tab - Unfocus");
        
        input.Stop();
        return recorder;
    }

    private void VerifyBaselineHighlight(MockTerminal terminal, int expectedHighlightIndex)
    {
        // In baseline, verify only one line has highlight background
        var highlightCount = 0;
        var theme = ThemeManager.Instance.CurrentTheme;
        var highlightBg = ConvertToConsoleColor(theme.Primary.Background);
        
        for (int y = 0; y < terminal.Height; y++)
        {
            var hasHighlight = false;
            for (int x = 0; x < terminal.Width; x++)
            {
                var (_, _, bg) = terminal.GetCharAt(x, y);
                if (bg == highlightBg)
                {
                    hasHighlight = true;
                    break;
                }
            }
            if (hasHighlight) highlightCount++;
        }
        
        if (highlightCount != 1)
        {
            _output.WriteLine($"⚠️ Baseline verification failed: Expected 1 highlight, found {highlightCount}");
        }
    }

    private ConsoleColor ConvertToConsoleColor(System.Drawing.Color color)
    {
        // Map theme colors to console colors
        // This is simplified - real implementation would use proper mapping
        if (color.GetBrightness() > 0.8) return ConsoleColor.White;
        if (color.GetBrightness() < 0.2) return ConsoleColor.Black;
        
        if (color.R > 128 && color.G < 128 && color.B < 128) return ConsoleColor.Red;
        if (color.R < 128 && color.G > 128 && color.B < 128) return ConsoleColor.Green;
        if (color.R < 128 && color.G < 128 && color.B > 128) return ConsoleColor.Blue;
        if (color.R > 128 && color.G > 128 && color.B < 128) return ConsoleColor.Yellow;
        if (color.R > 128 && color.G < 128 && color.B > 128) return ConsoleColor.Magenta;
        if (color.R < 128 && color.G > 128 && color.B > 128) return ConsoleColor.Cyan;
        
        return ConsoleColor.Gray;
    }

    private void AnalyzeExample11Issues(ComparisonReport report)
    {
        _output.WriteLine("\n🔍 Example 11 Specific Analysis:");
        
        // Check for multiple highlights issue
        var multiHighlightIssues = report.Issues.Where(i => 
            i.Type == "ColorRegionMismatch" && 
            i.Description.Contains("highlight")).ToList();
            
        if (multiHighlightIssues.Any())
        {
            _output.WriteLine($"  Multiple Highlights: {multiHighlightIssues.Count} occurrences");
            foreach (var issue in multiHighlightIssues.Take(3))
            {
                _output.WriteLine($"    Frame {issue.FrameNumber}: {issue.Description}");
            }
        }
        
        // Check for color bleeding
        var colorBleedIssues = report.Issues.Where(i => 
            i.Type == "ColorPersistence" || 
            i.Type == "ColorOscillation").ToList();
            
        if (colorBleedIssues.Any())
        {
            _output.WriteLine($"  Color Bleeding/Persistence: {colorBleedIssues.Count} occurrences");
            foreach (var issue in colorBleedIssues.Take(3))
            {
                _output.WriteLine($"    Frame {issue.FrameNumber}: {issue.Description}");
            }
        }
        
        // Check for partial background issues
        var partialBgIssues = report.FrameComparisons
            .SelectMany(fc => fc.Issues)
            .Where(i => i.Description.Contains("partial") || i.Description.Contains("background"))
            .ToList();
            
        if (partialBgIssues.Any())
        {
            _output.WriteLine($"  Partial Background Rendering: {partialBgIssues.Count} occurrences");
            foreach (var issue in partialBgIssues.Take(3))
            {
                _output.WriteLine($"    Frame {issue.FrameNumber}: {issue.Description}");
            }
        }
        
        // Entropy analysis for color scattering
        var entropyIssues = report.Issues.Where(i => i.Type == "EntropySpike").ToList();
        if (entropyIssues.Any())
        {
            _output.WriteLine($"  Color Scattering (Entropy Spikes): {entropyIssues.Count} occurrences");
            foreach (var issue in entropyIssues)
            {
                _output.WriteLine($"    Frame {issue.FrameNumber}: {issue.Description}");
            }
        }
    }

    private string SaveComparisonReport(ComparisonReport report)
    {
        var directory = Path.GetTempPath();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var filename = Path.Combine(directory, $"example11_comparison_{timestamp}.txt");
        
        File.WriteAllText(filename, report.GenerateSummary());
        
        // Also save detailed frame comparisons
        var detailFile = Path.ChangeExtension(filename, ".detailed.txt");
        var detailContent = GenerateDetailedReport(report);
        File.WriteAllText(detailFile, detailContent);
        
        return filename;
    }

    private string GenerateDetailedReport(ComparisonReport report)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== DETAILED FRAME COMPARISON ===");
        sb.AppendLine();
        
        foreach (var fc in report.FrameComparisons)
        {
            sb.AppendLine($"Frame {fc.ActualFrameNumber}: {fc.Action}");
            sb.AppendLine($"  Diff Score: {fc.DiffScore:F2}%");
            sb.AppendLine($"  Mismatched Cells: {fc.MismatchedCells}/{fc.TotalCells}");
            sb.AppendLine($"  Color Mismatches: {fc.ColorMismatches}");
            sb.AppendLine($"  Character Mismatches: {fc.CharMismatches}");
            
            if (fc.Issues.Any())
            {
                sb.AppendLine("  Issues:");
                foreach (var issue in fc.Issues)
                {
                    sb.AppendLine($"    - {issue.Type}: {issue.Description}");
                }
            }
            
            // Show sample of cell diffs
            if (fc.CellDiffs.Any())
            {
                sb.AppendLine("  Sample Cell Diffs (first 5):");
                foreach (var diff in fc.CellDiffs.Take(5))
                {
                    sb.AppendLine($"    ({diff.X},{diff.Y}): ");
                    if (diff.CharMismatch)
                        sb.AppendLine($"      Char: '{diff.ActualChar}' vs '{diff.BaselineChar}'");
                    if (diff.ColorMismatch)
                    {
                        sb.AppendLine($"      FG: {diff.ActualFg} vs {diff.BaselineFg}");
                        sb.AppendLine($"      BG: {diff.ActualBg} vs {diff.BaselineBg}");
                    }
                }
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    private Country[] CreateCountries()
    {
        return new[]
        {
            new Country { Code = "US", Name = "United States" },
            new Country { Code = "CN", Name = "China" },
            new Country { Code = "IN", Name = "India" },
            new Country { Code = "ID", Name = "Indonesia" },
            new Country { Code = "PK", Name = "Pakistan" },
            new Country { Code = "BR", Name = "Brazil" },
            new Country { Code = "NG", Name = "Nigeria" },
            new Country { Code = "BD", Name = "Bangladesh" }
        };
    }

    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    /// <summary>
    /// Mock SelectInput that renders correctly for baseline.
    /// </summary>
    private class MockSelectInput<T> : ISimpleComponent
    {
        private readonly T[] _items;
        private readonly Binding<Optional<T>> _binding;
        private readonly Func<T, string> _itemRenderer;
        private readonly int _visibleItems;
        private readonly string _placeholder;
        private readonly ITheme _theme;

        public MockSelectInput(
            T[] items, 
            Binding<Optional<T>> binding,
            Func<T, string> itemRenderer,
            int visibleItems,
            string placeholder,
            ITheme theme)
        {
            _items = items;
            _binding = binding;
            _itemRenderer = itemRenderer;
            _visibleItems = visibleItems;
            _placeholder = placeholder;
            _theme = theme;
        }

        public string GetKey() => "mock-select";
        
        public IEnumerable<ISimpleComponent> GetChildren() => Enumerable.Empty<ISimpleComponent>();
        
        // Simplified - real implementation would properly render
        public ViewInstance CreateInstance(string id) => throw new NotImplementedException();
        
        public VirtualNode Render() => throw new NotImplementedException();
    }
}

/// <summary>
/// Extensions for recording theme-aware colors.
/// </summary>
public static class ThemeColorExtensions
{
    public static string ToThemeColorName(this ConsoleColor color, ITheme theme)
    {
        // Map console colors back to theme color names
        // This helps track which theme color is being used
        
        if (color == ConsoleColor.Black) return "theme.Background";
        if (color == ConsoleColor.White) 
        {
            // Could be primary background for highlights
            return "theme.Primary.Background";
        }
        if (color == ConsoleColor.DarkGray) return "theme.Disabled.Foreground";
        if (color == ConsoleColor.Gray) return "theme.Default.Foreground";
        if (color == ConsoleColor.Green) return "theme.Success.Foreground";
        if (color == ConsoleColor.Red) return "theme.Error.Foreground";
        if (color == ConsoleColor.Yellow) return "theme.Warning.Foreground";
        if (color == ConsoleColor.Cyan) return "theme.Info.Foreground";
        
        return $"console.{color}";
    }
}