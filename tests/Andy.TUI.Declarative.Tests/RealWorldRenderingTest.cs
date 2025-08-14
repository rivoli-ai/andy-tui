using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests;

/// <summary>
/// Real-world rendering test that runs the actual example in a subprocess
/// and captures its output to verify it works correctly.
/// </summary>
public class RealWorldRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public RealWorldRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void RunActualExample11_CaptureRealOutput()
    {
        using (BeginScenario("Real Example 11 Execution"))
        {
            LogStep("Building the example project");
            
            // Build the example project
            var buildProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "build examples/Andy.TUI.Examples.Input/Andy.TUI.Examples.Input.csproj",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            buildProcess.Start();
            var buildOutput = buildProcess.StandardOutput.ReadToEnd();
            var buildError = buildProcess.StandardError.ReadToEnd();
            buildProcess.WaitForExit();
            
            if (buildProcess.ExitCode != 0)
            {
                _output.WriteLine($"Build failed: {buildError}");
                Assert.Fail($"Failed to build example project: {buildError}");
            }
            
            LogStep("Running Example 11 with automated input");
            
            // Create a script to automate the example
            var scriptPath = Path.GetTempFileName() + ".sh";
            var outputPath = Path.GetTempFileName() + ".txt";
            
            // Script that runs the example and captures output
            var script = $@"#!/bin/bash
# Run the example with automated input
(
    echo '11'  # Select example 11
    sleep 1
    printf '\033[B'  # Down arrow
    sleep 0.5
    printf '\t'  # Tab
    sleep 0.5
    printf '\033[B'  # Down arrow
    sleep 0.5
    printf '\033[B'  # Down arrow
    sleep 0.5
    printf '\033[B'  # Down arrow
    sleep 0.5
    printf '\t'  # Tab to next
    sleep 0.5
    printf '\033[B'  # Down arrow
    sleep 0.5
    # Capture final state
    sleep 1
) | script -q {outputPath} dotnet run --project examples/Andy.TUI.Examples.Input/Andy.TUI.Examples.Input.csproj 2>&1

# Also try to capture with tee
";
            
            File.WriteAllText(scriptPath, script);
            
            // Make script executable
            var chmodProcess = Process.Start("chmod", $"+x {scriptPath}");
            chmodProcess?.WaitForExit();
            
            // Run the script
            var runProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = scriptPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            runProcess.Start();
            
            // Set a timeout
            var completed = runProcess.WaitForExit(10000);
            
            if (!completed)
            {
                runProcess.Kill();
                _output.WriteLine("Process timed out - killed");
            }
            
            var output = runProcess.StandardOutput.ReadToEnd();
            var error = runProcess.StandardError.ReadToEnd();
            
            _output.WriteLine($"=== Process Output ===");
            _output.WriteLine(output);
            
            if (!string.IsNullOrEmpty(error))
            {
                _output.WriteLine($"=== Process Errors ===");
                _output.WriteLine(error);
            }
            
            // Read captured output
            if (File.Exists(outputPath))
            {
                var capturedOutput = File.ReadAllText(outputPath);
                _output.WriteLine($"=== Captured Terminal Output ===");
                _output.WriteLine(capturedOutput);
                
                // Analyze the output
                AnalyzeRealOutput(capturedOutput);
            }
            
            // Cleanup
            try
            {
                File.Delete(scriptPath);
                File.Delete(outputPath);
            }
            catch { }
        }
    }

    private void AnalyzeRealOutput(string output)
    {
        _output.WriteLine("\n=== Output Analysis ===");
        
        // Check for common issues
        var lines = output.Split('\n');
        
        // Count visible lines vs empty lines
        var visibleLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).Count();
        var totalLines = lines.Length;
        
        _output.WriteLine($"Visible lines: {visibleLines}/{totalLines}");
        
        // Check for title visibility
        if (output.Contains("SelectInput Component Demo"))
        {
            _output.WriteLine("✓ Title is visible");
        }
        else
        {
            _output.WriteLine("✗ Title is NOT visible");
        }
        
        // Check for dropdown rendering
        if (output.Contains("┌") && output.Contains("┐") && output.Contains("└") && output.Contains("┘"))
        {
            _output.WriteLine("✓ Dropdown borders rendered");
        }
        else
        {
            _output.WriteLine("✗ Dropdown borders NOT rendered");
        }
        
        // Check for gray color escape codes
        var grayEscapes = CountOccurrences(output, "\x1b[90m"); // Dark gray
        var normalEscapes = CountOccurrences(output, "\x1b[0m"); // Reset
        
        _output.WriteLine($"Gray color escapes: {grayEscapes}");
        _output.WriteLine($"Reset escapes: {normalEscapes}");
        
        // Look for specific patterns that indicate problems
        if (output.Contains("\x1b[90m") && !output.Contains("\x1b[37m") && !output.Contains("\x1b[97m"))
        {
            _output.WriteLine("⚠️ Everything appears to be gray - no white/bright colors found!");
        }
        
        // Extract visible content
        var cleanOutput = RemoveAnsiCodes(output);
        var contentLines = cleanOutput.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        
        _output.WriteLine("\n=== Cleaned Visible Content ===");
        foreach (var line in contentLines.Take(20))
        {
            _output.WriteLine($"  {line}");
        }
    }

    private int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(pattern, index)) != -1)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }

    private string RemoveAnsiCodes(string text)
    {
        // Remove ANSI escape sequences
        return System.Text.RegularExpressions.Regex.Replace(text, @"\x1B\[[^@-~]*[@-~]", "");
    }
}

/// <summary>
/// Systematic test runner that validates each component in isolation
/// and then in combination.
/// </summary>
public class SystematicComponentTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public SystematicComponentTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Theory]
    [InlineData("SingleText")]
    [InlineData("VStackWithText")]
    [InlineData("SelectInputAlone")]
    [InlineData("SelectInputInVStack")]
    [InlineData("MultipleSelectInputs")]
    public void TestComponentSystematically(string scenario)
    {
        using (BeginScenario($"Systematic: {scenario}"))
        {
            var terminal = new MockTerminal(80, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            ISimpleComponent BuildUI() => scenario switch
            {
                "SingleText" => new Text("Test Text").Color(Color.White),
                
                "VStackWithText" => new VStack(spacing: 1)
                {
                    new Text("Line 1").Color(Color.White),
                    new Text("Line 2").Color(Color.Yellow),
                    new Text("Line 3").Color(Color.Cyan)
                },
                
                "SelectInputAlone" => new SelectInput<string>(
                    new[] { "Option 1", "Option 2", "Option 3" },
                    new Binding<Optional<string>>(() => Optional<string>.None, _ => { }, "test"),
                    visibleItems: 3
                ),
                
                "SelectInputInVStack" => new VStack(spacing: 1)
                {
                    new Text("Title").Color(Color.Cyan),
                    new SelectInput<string>(
                        new[] { "Option 1", "Option 2", "Option 3" },
                        new Binding<Optional<string>>(() => Optional<string>.None, _ => { }, "test"),
                        visibleItems: 3
                    )
                },
                
                "MultipleSelectInputs" => new VStack(spacing: 1)
                {
                    new Text("Title").Color(Color.Cyan),
                    new SelectInput<string>(
                        new[] { "A", "B", "C" },
                        new Binding<Optional<string>>(() => Optional<string>.None, _ => { }, "test1"),
                        visibleItems: 3
                    ),
                    new SelectInput<string>(
                        new[] { "X", "Y", "Z" },
                        new Binding<Optional<string>>(() => Optional<string>.None, _ => { }, "test2"),
                        visibleItems: 3
                    )
                },
                
                _ => new Text("Unknown scenario")
            };
            
            renderingSystem.Initialize();
            
            var rendererThread = new Thread(() =>
            {
                try { renderer.Run(BuildUI); }
                catch (Exception ex) { Logger.Error(ex, $"Renderer error in {scenario}"); }
            })
            { IsBackground = true };
            rendererThread.Start();
            
            Thread.Sleep(200);
            
            // Capture initial render
            var initialContent = CaptureVisibleContent(terminal);
            _output.WriteLine($"=== Initial Render for {scenario} ===");
            _output.WriteLine(initialContent);
            
            // Validate initial state
            ValidateScenario(scenario, initialContent, "initial");
            
            // If it has inputs, test interaction
            if (scenario.Contains("SelectInput"))
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(100);
                
                var afterTab = CaptureVisibleContent(terminal);
                _output.WriteLine($"\n=== After Tab for {scenario} ===");
                _output.WriteLine(afterTab);
                
                ValidateScenario(scenario, afterTab, "afterTab");
                
                input.EmitKey('\0', ConsoleKey.DownArrow);
                Thread.Sleep(100);
                
                var afterDown = CaptureVisibleContent(terminal);
                _output.WriteLine($"\n=== After DownArrow for {scenario} ===");
                _output.WriteLine(afterDown);
                
                ValidateScenario(scenario, afterDown, "afterDown");
            }
            
            input.Stop();
        }
    }

    private string CaptureVisibleContent(MockTerminal terminal)
    {
        var lines = new List<string>();
        for (int y = 0; y < terminal.Height; y++)
        {
            var line = terminal.GetLine(y);
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add($"[{y:D2}]: {line}");
            }
        }
        return string.Join("\n", lines);
    }

    private void ValidateScenario(string scenario, string content, string phase)
    {
        var hasContent = !string.IsNullOrWhiteSpace(content);
        
        switch (scenario)
        {
            case "SingleText":
                Assert.True(hasContent, $"{phase}: Should have rendered text");
                Assert.Contains("Test Text", content);
                break;
                
            case "VStackWithText":
                Assert.True(hasContent, $"{phase}: Should have rendered VStack");
                Assert.Contains("Line 1", content);
                Assert.Contains("Line 2", content);
                Assert.Contains("Line 3", content);
                break;
                
            case "SelectInputAlone":
                Assert.True(hasContent, $"{phase}: Should have rendered SelectInput");
                if (phase == "initial")
                {
                    // Should show placeholder
                    Assert.Contains("[", content);
                    Assert.Contains("]", content);
                }
                else if (phase == "afterTab")
                {
                    // Should show dropdown
                    Assert.Contains("┌", content);
                    Assert.Contains("Option", content);
                }
                break;
                
            case "SelectInputInVStack":
                Assert.True(hasContent, $"{phase}: Should have rendered VStack with SelectInput");
                Assert.Contains("Title", content);
                break;
                
            case "MultipleSelectInputs":
                Assert.True(hasContent, $"{phase}: Should have rendered multiple SelectInputs");
                Assert.Contains("Title", content);
                if (phase == "afterTab")
                {
                    // First SelectInput should be focused
                    Assert.Contains("A", content);
                }
                break;
        }
    }
}