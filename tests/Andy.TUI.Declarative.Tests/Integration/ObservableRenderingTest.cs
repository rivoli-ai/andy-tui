using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Test to reproduce and investigate chat example rendering issues with observable state.
/// </summary>
public class ObservableRenderingTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public ObservableRenderingTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void ObservableList_ShouldTriggerRerenderOnAdd()
    {
        using (BeginScenario("Observable List Rendering Updates"))
        {
            LogStep("Setting up terminal and rendering system with comprehensive logging");
            
            // Enable all debug logging
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

            // Observable state
            var messages = new ObservableList<string>();
            var status = new ObservableProperty<string>("Ready");
            var renderCount = 0;
            var lastRenderTime = DateTime.Now;

            LogStep("Creating root component with observable bindings");
            ISimpleComponent Root()
            {
                renderCount++;
                var now = DateTime.Now;
                var timeSinceLastRender = (now - lastRenderTime).TotalMilliseconds;
                lastRenderTime = now;
                
                Logger.Debug($"Root() called - Render #{renderCount}, Time since last: {timeSinceLastRender}ms");
                Logger.Debug($"Messages count: {messages.Count}, Status: {status.Value}");
                
                var stack = new VStack(spacing: 1)
                {
                    new Text($"Chat Example (Render #{renderCount})").Bold(),
                    new Text($"Status: {status.Value}").Color(Color.Gray)
                };
                
                // Add messages
                foreach (var msg in messages)
                {
                    stack.Add(new Text($"- {msg}"));
                }
                
                return stack;
            }

            LogStep("Subscribing to observable changes");
            var collectionChangedCount = 0;
            var propertyChangedCount = 0;
            
            messages.CollectionChanged += (s, e) =>
            {
                collectionChangedCount++;
                Logger.Info($"CollectionChanged event #{collectionChangedCount}: Action={e.Action}, NewItems={e.NewItems?.Count}, OldItems={e.OldItems?.Count}");
                Logger.Debug("Requesting render from CollectionChanged handler");
                renderer.RequestRender();
            };
            
            status.PropertyChanged += (s, e) =>
            {
                propertyChangedCount++;
                Logger.Info($"PropertyChanged event #{propertyChangedCount}: Property={e.PropertyName}, NewValue={status.Value}");
                Logger.Debug("Requesting render from PropertyChanged handler");
                renderer.RequestRender();
            };

            LogStep("Initializing rendering system");
            renderingSystem.Initialize();

            LogStep("Starting renderer thread");
            var rendererThread = new Thread(() =>
            {
                try
                {
                    Logger.Debug("Renderer thread started");
                    renderer.Run(Root);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Renderer thread crashed");
                    throw;
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            // Wait for initial render
            Thread.Sleep(200);
            LogAssertion($"Initial render count should be 1, got {renderCount}");
            Assert.Equal(1, renderCount);

            LogStep("Adding first message to observable list");
            messages.Add("Hello, World!");
            
            // Wait for re-render
            Thread.Sleep(100);
            LogData("Render count after first message", renderCount);
            LogData("Collection changed events", collectionChangedCount);
            LogAssertion($"Should have triggered re-render, render count should be 2, got {renderCount}");
            Assert.Equal(2, renderCount);
            Assert.Equal(1, collectionChangedCount);

            LogStep("Changing status property");
            status.Value = "Processing...";
            
            Thread.Sleep(100);
            LogData("Render count after status change", renderCount);
            LogData("Property changed events", propertyChangedCount);
            LogAssertion($"Should have triggered re-render, render count should be 3, got {renderCount}");
            Assert.Equal(3, renderCount);
            Assert.Equal(1, propertyChangedCount);

            LogStep("Adding multiple messages rapidly");
            messages.Add("Message 2");
            messages.Add("Message 3");
            messages.Add("Message 4");
            
            Thread.Sleep(200);
            LogData("Final render count", renderCount);
            LogData("Final collection changed events", collectionChangedCount);
            LogAssertion($"Should have triggered re-renders for each message, got {renderCount} renders");
            // Note: Rapid updates may be batched, which is actually good for performance
            Assert.True(renderCount >= 4, $"Expected at least 4 renders, got {renderCount}");

            LogStep("Checking terminal output for messages");
            _output.WriteLine("=== Terminal Buffer ===");
            for (int y = 0; y < Math.Min(10, terminal.Height); y++)
            {
                var line = terminal.GetLine(y);
                if (!string.IsNullOrWhiteSpace(line))
                    _output.WriteLine($"Line {y}: [{line.TrimEnd()}]");
            }
            
            // Verify messages appear in output
            var bufferText = GetBufferText(terminal);
            LogData("Buffer contains 'Hello, World!'", bufferText.Contains("Hello, World!"));
            LogData("Buffer contains 'Message 4'", bufferText.Contains("Message 4"));
            Assert.Contains("Hello, World!", bufferText);
            Assert.Contains("Message 4", bufferText);

            input.Stop();
            
            // Export comprehensive logs for analysis
            LogStep("Exporting comprehensive logs");
            var summary = GetTestSummary();
            _output.WriteLine("=== Test Summary ===");
            _output.WriteLine(summary);
        }
    }

    [Fact]
    public void ChatExample_SimulatedLLMUpdates_ShouldRender()
    {
        using (BeginScenario("Simulated Chat with LLM Updates"))
        {
            LogStep("Setting up chat simulation");
            
            var terminal = new MockTerminal(100, 30);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

            // Chat state
            var messages = new ObservableList<(string role, string content)>();
            var inputText = new ObservableProperty<string>("");
            var status = new ObservableProperty<string>("Ready");
            var renderCount = 0;

            LogStep("Creating chat UI component");
            ISimpleComponent ChatUI()
            {
                renderCount++;
                Logger.Debug($"ChatUI render #{renderCount}, messages: {messages.Count}");
                
                var ui = new VStack(spacing: 1)
                {
                    new Text("=== Chat Example ===").Bold(),
                    new Text($"Status: {status.Value}").Color(Color.Gray),
                    new Box
                    {
                        BuildMessageList()
                    }.WithHeight(10).WithWidth(80)
                };
                
                return ui;
            }

            VStack BuildMessageList()
            {
                var list = new VStack(spacing: 0);
                foreach (var (role, content) in messages)
                {
                    var color = role == "user" ? Color.Cyan : Color.White;
                    var prefix = role == "user" ? "You: " : "AI: ";
                    list.Add(new Text($"{prefix}{content}").Color(color));
                }
                return list;
            }

            LogStep("Setting up observable subscriptions");
            messages.CollectionChanged += (_, e) =>
            {
                Logger.Info($"Messages changed: {e.Action}, Count={messages.Count}");
                renderer.RequestRender();
            };
            
            status.PropertyChanged += (_, __) =>
            {
                Logger.Info($"Status changed to: {status.Value}");
                renderer.RequestRender();
            };

            renderingSystem.Initialize();

            var rendererThread = new Thread(() =>
            {
                try
                {
                    renderer.Run(ChatUI);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Renderer thread error");
                    throw;
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(200);
            var initialRenderCount = renderCount;
            LogData("Initial render count", initialRenderCount);

            LogStep("Simulating user message");
            messages.Add(("user", "What is the weather today?"));
            status.Value = "Sending...";
            
            Thread.Sleep(100);
            LogData("Render count after user message", renderCount);
            Assert.True(renderCount > initialRenderCount, "Should have re-rendered after user message");

            LogStep("Simulating LLM thinking delay");
            status.Value = "AI is thinking...";
            Thread.Sleep(100);

            LogStep("Simulating LLM response");
            // Simulate LLM response directly (avoid async in test)
            Thread.Sleep(50);
            messages.Add(("assistant", "I don't have access to real-time weather data."));
            status.Value = "Ready";

            Thread.Sleep(200);
            var finalRenderCount = renderCount;
            LogData("Final render count", finalRenderCount);
            
            LogStep("Verifying messages appear in buffer");
            var buffer = GetBufferText(terminal);
            _output.WriteLine("=== Final Buffer Content ===");
            var lines = buffer.Split('\n');
            for (int i = 0; i < Math.Min(15, lines.Length); i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                    _output.WriteLine($"  {lines[i].TrimEnd()}");
            }
            
            Assert.Contains("What is the weather", buffer);
            Assert.Contains("weather data", buffer);
            Assert.True(finalRenderCount >= 4, $"Expected at least 4 renders, got {finalRenderCount}");

            LogStep("Checking for rendering errors in logs");
            var stats = LogManager.GetStatistics();
            var errors = stats.LevelCounts.GetValueOrDefault(LogLevel.Error, 0);
            var warnings = stats.LevelCounts.GetValueOrDefault(LogLevel.Warning, 0);
            
            LogData("Error count", errors);
            LogData("Warning count", warnings);
            
            if (errors > 0)
            {
                _output.WriteLine("=== ERRORS FOUND ===");
                _output.WriteLine(LogManager.GetErrorSummary());
            }

            input.Stop();
        }
    }

    private string GetBufferText(MockTerminal terminal)
    {
        var text = "";
        for (int y = 0; y < terminal.Height; y++)
        {
            text += terminal.GetLine(y) + "\n";
        }
        return text;
    }
}