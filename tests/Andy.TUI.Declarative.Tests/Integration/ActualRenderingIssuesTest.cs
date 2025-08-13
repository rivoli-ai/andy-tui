using System;
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
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Test specifically for the actual rendering issues users report:
/// - Buttons not appearing initially or disappearing
/// - Text vanishing from fields during typing
/// </summary>
public class ActualRenderingIssuesTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public ActualRenderingIssuesTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void ButtonsNotRenderingInitially()
    {
        using (BeginScenario("Buttons Not Rendering Initially"))
        {
            LogStep("Setting up simple UI with just buttons");
            
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

            ISimpleComponent BuildUI()
            {
                Logger.Debug("BuildUI called - creating button UI");
                return new VStack(spacing: 1)
                {
                    new Text("Test App"),
                    new Button("Button 1", () => Logger.Info("Button 1 clicked")),
                    new Button("Button 2", () => Logger.Info("Button 2 clicked"))
                };
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

            // Wait for initial render
            Thread.Sleep(100);
            
            LogStep("Checking if buttons rendered initially");
            var buffer = GetBufferContent(terminal);
            LogBufferContent("Initial", buffer);
            
            // CRITICAL: Buttons should be visible immediately
            LogAssertion("Buttons should be visible on initial render");
            Assert.Contains("Button 1", buffer);
            Assert.Contains("Button 2", buffer);
            Assert.Contains("[", buffer); // Button brackets
            Assert.Contains("]", buffer);
            
            input.Stop();
        }
    }

    [Fact]
    public void TextDisappearsWhileTyping()
    {
        using (BeginScenario("Text Disappears While Typing"))
        {
            LogStep("Setting up UI with text field");
            
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: true);

            var text = "";
            var previousBuffers = new System.Collections.Generic.List<string>();

            ISimpleComponent BuildUI()
            {
                return new VStack(spacing: 1)
                {
                    new Text("Enter text:"),
                    new TextField("Placeholder", new Binding<string>(
                        () => text,
                        v => text = v,
                        "Text"
                    ))
                };
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

            Thread.Sleep(100);
            
            LogStep("Initial state - should show placeholder");
            var initialBuffer = GetBufferContent(terminal);
            previousBuffers.Add(initialBuffer);
            LogBufferContent("Initial", initialBuffer);
            Assert.Contains("Placeholder", initialBuffer);
            
            LogStep("Typing first character");
            input.EmitKey('H', ConsoleKey.H);
            Thread.Sleep(50);
            
            var afterFirstChar = GetBufferContent(terminal);
            previousBuffers.Add(afterFirstChar);
            LogBufferContent("After 'H'", afterFirstChar);
            
            // Text should NOT disappear
            LogAssertion("First character should be visible");
            Assert.DoesNotContain("Placeholder", afterFirstChar);
            Assert.Contains("H", afterFirstChar);
            
            LogStep("Typing more characters rapidly");
            string testText = "ello";
            foreach (char c in testText)
            {
                input.EmitKey(c, ConsoleKey.E);
                Thread.Sleep(20); // Rapid typing
                
                var currentBuffer = GetBufferContent(terminal);
                previousBuffers.Add(currentBuffer);
                
                // Check if text is accumulating or disappearing
                var expectedLength = previousBuffers.Count - 1; // Minus initial buffer
                Logger.Debug($"After typing '{c}', expected {expectedLength} chars in field");
            }
            
            Thread.Sleep(100);
            var finalBuffer = GetBufferContent(terminal);
            LogBufferContent("Final", finalBuffer);
            
            LogAssertion("Full text 'Hello' should be visible");
            Assert.Contains("Hello", finalBuffer);
            
            LogStep("Analyzing text persistence across renders");
            for (int i = 1; i < previousBuffers.Count; i++)
            {
                var prevBuffer = previousBuffers[i - 1];
                var currBuffer = previousBuffers[i];
                
                // Check if text disappeared between renders
                if (i > 1 && !currBuffer.Contains("H"))
                {
                    Logger.Error($"Text disappeared at buffer {i}!");
                    _output.WriteLine($"ERROR: Text disappeared at buffer {i}");
                    _output.WriteLine($"Previous: {prevBuffer}");
                    _output.WriteLine($"Current: {currBuffer}");
                }
            }
            
            input.Stop();
        }
    }

    [Fact]
    public void ButtonsDisappearAfterStateChange()
    {
        using (BeginScenario("Buttons Disappear After State Change"))
        {
            LogStep("Setting up UI with buttons and state");
            
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

            var counter = new ObservableProperty<int>(0);
            var showButtons = true;

            ISimpleComponent BuildUI()
            {
                Logger.Debug($"BuildUI: counter={counter.Value}, showButtons={showButtons}");
                _output.WriteLine($"BuildUI called: counter={counter.Value}");
                
                var stack = new VStack(spacing: 1)
                {
                    new Text($"Counter: {counter.Value}")
                };
                
                // Conditional rendering - potential issue?
                if (showButtons)
                {
                    stack.Add(new Button("Increment", () => 
                    {
                        _output.WriteLine($"Increment clicked, counter before: {counter.Value}");
                        counter.Value++;
                        _output.WriteLine($"Counter after: {counter.Value}");
                        renderer.RequestRender();
                        _output.WriteLine("RequestRender called");
                    }));
                    stack.Add(new Button("Decrement", () => 
                    {
                        counter.Value--;
                        renderer.RequestRender();
                    }));
                }
                
                return stack;
            }

            // Subscribe to changes
            counter.PropertyChanged += (_, __) => renderer.RequestRender();

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

            Thread.Sleep(100);
            
            LogStep("Initial render - buttons should be visible");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial", initialBuffer);
            Assert.Contains("Increment", initialBuffer);
            Assert.Contains("Decrement", initialBuffer);
            
            LogStep("Clicking increment button");
            input.EmitKey('\t', ConsoleKey.Tab); // Focus first button
            Thread.Sleep(50);
            _output.WriteLine("Sending Enter key to click button");
            input.EmitKey('\r', ConsoleKey.Enter); // Click
            
            // Wait for the BuildUI to be called with new value
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(100);
                var tempBuffer = GetBufferContent(terminal);
                if (tempBuffer.Contains("Counter: 1"))
                {
                    _output.WriteLine($"Counter updated after {(i + 1) * 100}ms");
                    break;
                }
            }
            
            var afterIncrement = GetBufferContent(terminal);
            LogBufferContent("After Increment", afterIncrement);
            
            // Counter should update
            Assert.Contains("Counter: 1", afterIncrement);
            
            // CRITICAL: Buttons should STILL be visible
            LogAssertion("Buttons must remain visible after state change");
            Assert.Contains("Increment", afterIncrement);
            Assert.Contains("Decrement", afterIncrement);
            
            LogStep("Multiple rapid state changes");
            for (int i = 0; i < 3; i++)
            {
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(30);
            }
            
            Thread.Sleep(100);
            var afterRapid = GetBufferContent(terminal);
            LogBufferContent("After Rapid Changes", afterRapid);
            
            LogAssertion("Buttons must still be visible after rapid changes");
            Assert.Contains("Increment", afterRapid);
            Assert.Contains("Decrement", afterRapid);
            
            input.Stop();
        }
    }

    [Fact]
    public void ElementsOverwriteEachOther()
    {
        using (BeginScenario("Elements Overwriting Each Other"))
        {
            LogStep("Setting up UI with overlapping elements");
            
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

            var showModal = false;

            ISimpleComponent BuildUI()
            {
                var ui = new VStack(spacing: 1)
                {
                    new Text("Main Content"),
                    new Button("Show Modal", () => 
                    {
                        showModal = true;
                        renderer.RequestRender();
                    }),
                    new Button("Button 2", () => Logger.Info("Button 2")),
                    new Button("Button 3", () => Logger.Info("Button 3"))
                };
                
                // Modal might overwrite buttons?
                if (showModal)
                {
                    ui.Add(new Text("MODAL ACTIVE"));
                }
                
                return ui;
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

            Thread.Sleep(100);
            
            LogStep("Initial render");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial", initialBuffer);
            
            // All buttons should be visible
            Assert.Contains("Show Modal", initialBuffer);
            Assert.Contains("Button 2", initialBuffer);
            Assert.Contains("Button 3", initialBuffer);
            
            LogStep("Activating modal");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            input.EmitKey('\r', ConsoleKey.Enter);
            Thread.Sleep(100);
            
            var withModal = GetBufferContent(terminal);
            LogBufferContent("With Modal", withModal);
            
            // Modal should be visible
            Assert.Contains("MODAL ACTIVE", withModal);
            
            // But buttons should ALSO still be visible
            LogAssertion("Original buttons should not be overwritten by modal");
            Assert.Contains("Show Modal", withModal);
            Assert.Contains("Button 2", withModal);
            Assert.Contains("Button 3", withModal);
            
            input.Stop();
        }
    }

    private string GetBufferContent(MockTerminal terminal)
    {
        var content = "";
        for (int y = 0; y < Math.Min(15, terminal.Height); y++)
        {
            var line = terminal.GetLine(y);
            if (!string.IsNullOrWhiteSpace(line))
            {
                content += line.TrimEnd() + "\n";
            }
        }
        return content;
    }

    private void LogBufferContent(string label, string content)
    {
        _output.WriteLine($"\n=== {label} Buffer ===");
        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length && i < 15; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                _output.WriteLine($"  {lines[i]}");
            }
        }
        _output.WriteLine("=================\n");
    }
}