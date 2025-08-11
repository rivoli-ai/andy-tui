using System;
using System.IO;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Diagnostics;
using Andy.TUI.Tests.Common;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Diagnostic test runner that captures comprehensive logs for debugging failing tests.
/// </summary>
public class DiagnosticTestRunner : TestBase
{
    public DiagnosticTestRunner(ITestOutputHelper output) : base(output)
    {
    }
    
    [Fact]
    public void Diagnose_TextField_InputHandling()
    {
        // Initialize comprehensive logging at the start
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true, 
            customLogPath: Path.Combine(Directory.GetCurrentDirectory(), "DiagnosticLogs"));
        
        using (BeginScenario("TextField Input Handling Diagnosis"))
        {
            LogStep("Creating test environment");
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);
            
            string fieldValue = string.Empty;
            
            LogStep("Creating simple TextField component");
            ISimpleComponent Root() => new VStack
            {
                new TextField(
                    "Enter text...", 
                    new Andy.TUI.Declarative.State.Binding<string>(
                        () => fieldValue, 
                        v => {
                            Logger.Debug($"TextField binding setter called: '{v}'");
                            fieldValue = v;
                        }))
            };
            
            LogStep("Initializing and starting renderer");
            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);
            
            LogStep("Pressing TAB to focus TextField");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            LogStep("Typing 'TEST' into field");
            foreach (char c in "TEST")
            {
                Logger.Debug($"Emitting key: '{c}'");
                input.EmitKey(c, (ConsoleKey)c);
                Thread.Sleep(30);
                LogData($"Field value after '{c}'", fieldValue);
            }
            
            LogStep("Final verification");
            LogData("Final field value", fieldValue);
            
            // Export diagnostic logs
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), 
                "DiagnosticLogs", $"TextField_Diagnosis_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            LogManager.ExportLogs(logPath);
            Logger.Info($"Diagnostic log exported to: {logPath}");
            
            LogAssertion("Field should contain 'TEST'");
            Assert.Equal("TEST", fieldValue);
            
            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }
    
    [Fact]
    public void Diagnose_FocusManager_TabNavigation()
    {
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true,
            customLogPath: Path.Combine(Directory.GetCurrentDirectory(), "DiagnosticLogs"));
        
        using (BeginScenario("Focus Manager Tab Navigation"))
        {
            LogStep("Setting up multi-field form");
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);
            
            string field1 = string.Empty;
            string field2 = string.Empty;
            
            ISimpleComponent Root() => new VStack(spacing: 1)
            {
                new Text("Two Field Form"),
                new TextField("Field 1", new Andy.TUI.Declarative.State.Binding<string>(
                    () => field1, 
                    v => {
                        Logger.Debug($"Field1 setter: '{v}'");
                        field1 = v;
                    })),
                new TextField("Field 2", new Andy.TUI.Declarative.State.Binding<string>(
                    () => field2,
                    v => {
                        Logger.Debug($"Field2 setter: '{v}'");
                        field2 = v;
                    }))
            };
            
            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);
            
            // TAB to first field
            LogStep("TAB to first field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            // Type in first field
            LogStep("Type 'A' in first field");
            input.EmitKey('A', ConsoleKey.A);
            Thread.Sleep(50);
            LogData("Field1 after 'A'", field1);
            LogData("Field2 after 'A'", field2);
            
            // TAB to second field
            LogStep("TAB to second field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            // Type in second field
            LogStep("Type 'B' in second field");
            input.EmitKey('B', ConsoleKey.B);
            Thread.Sleep(50);
            LogData("Field1 after 'B'", field1);
            LogData("Field2 after 'B'", field2);
            
            var logPath = Path.Combine(Directory.GetCurrentDirectory(),
                "DiagnosticLogs", $"FocusNav_Diagnosis_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            LogManager.ExportLogs(logPath);
            Logger.Info($"Diagnostic log exported to: {logPath}");
            
            LogAssertion("Field1 should contain 'A'");
            Assert.Equal("A", field1);
            LogAssertion("Field2 should contain 'B'");
            Assert.Equal("B", field2);
            
            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }
    
    [Fact]
    public void Diagnose_Backspace_Handling()
    {
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true,
            customLogPath: Path.Combine(Directory.GetCurrentDirectory(), "DiagnosticLogs"));
        
        using (BeginScenario("Backspace Key Handling"))
        {
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);
            
            string value = string.Empty;
            
            ISimpleComponent Root() => new TextField("Type here", 
                new Andy.TUI.Declarative.State.Binding<string>(
                    () => value,
                    v => {
                        Logger.Debug($"Value changed from '{value}' to '{v}'");
                        value = v;
                    }));
            
            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);
            
            // Focus field
            LogStep("Focus field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            // Type "ABC"
            LogStep("Type 'ABC'");
            foreach (char c in "ABC")
            {
                input.EmitKey(c, (ConsoleKey)c);
                Thread.Sleep(30);
                LogData($"After '{c}'", value);
            }
            
            // Press backspace
            LogStep("Press backspace");
            input.EmitKey('\b', ConsoleKey.Backspace);
            Thread.Sleep(50);
            LogData("After backspace", value);
            
            var logPath = Path.Combine(Directory.GetCurrentDirectory(),
                "DiagnosticLogs", $"Backspace_Diagnosis_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            LogManager.ExportLogs(logPath);
            Logger.Info($"Diagnostic log exported to: {logPath}");
            
            LogAssertion("Should have 'AB' after backspace");
            Assert.Equal("AB", value);
            
            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }
}