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
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Test to reproduce and debug rendering stability issues:
/// - Buttons disappearing
/// - Text vanishing from input fields
/// </summary>
public class RenderingStabilityTest : TestBase
{
    private readonly ITestOutputHelper _output;

    public RenderingStabilityTest(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void ButtonsShouldRemainVisibleAfterUpdates()
    {
        using (BeginScenario("Button Rendering Stability"))
        {
            LogStep("Setting up terminal with buttons and state changes");
            
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var buttonClickCount = 0;
            var statusText = "Ready";
            var renderCount = 0;
            var buttonsFoundInRender = new List<int>();

            LogStep("Creating UI with multiple buttons");
            ISimpleComponent BuildUI()
            {
                renderCount++;
                Logger.Debug($"BuildUI called - render #{renderCount}");
                
                return new VStack(spacing: 1)
                {
                    new Text($"Status: {statusText}"),
                    new Text($"Click count: {buttonClickCount}"),
                    new HStack(spacing: 2)
                    {
                        new Button("Click Me", () => 
                        {
                            buttonClickCount++;
                            statusText = $"Clicked {buttonClickCount} times";
                            Logger.Info($"Button clicked, count={buttonClickCount}");
                            renderer.RequestRender();
                        }).Primary(),
                        new Button("Reset", () => 
                        {
                            buttonClickCount = 0;
                            statusText = "Reset";
                            Logger.Info("Reset button clicked");
                            renderer.RequestRender();
                        }).Secondary(),
                        new Button("Exit", () => 
                        {
                            Logger.Info("Exit button clicked");
                        })
                    }
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
                    throw;
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(200);
            
            LogStep("Checking initial button render");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial", initialBuffer);
            
            // Check for button text
            Assert.Contains("Click Me", initialBuffer);
            Assert.Contains("Reset", initialBuffer);
            Assert.Contains("Exit", initialBuffer);
            
            LogStep("Simulating button click via tab navigation");
            input.EmitKey('\t', ConsoleKey.Tab); // Focus first button
            Thread.Sleep(50);
            
            var afterTabBuffer = GetBufferContent(terminal);
            LogBufferContent("After Tab", afterTabBuffer);
            
            // Buttons should still be visible
            Assert.Contains("Click Me", afterTabBuffer);
            Assert.Contains("Reset", afterTabBuffer);
            
            LogStep("Pressing Enter to click button");
            input.EmitKey('\r', ConsoleKey.Enter);
            Thread.Sleep(100);
            
            var afterClickBuffer = GetBufferContent(terminal);
            LogBufferContent("After Click", afterClickBuffer);
            
            // Check status updated
            Assert.Contains("Clicked 1 times", afterClickBuffer);
            
            // CRITICAL: Buttons should STILL be visible after state change
            LogAssertion("Buttons should remain visible after click");
            Assert.Contains("Click Me", afterClickBuffer);
            Assert.Contains("Reset", afterClickBuffer);
            Assert.Contains("Exit", afterClickBuffer);
            
            LogStep("Multiple rapid state changes");
            for (int i = 0; i < 5; i++)
            {
                input.EmitKey('\r', ConsoleKey.Enter);
                Thread.Sleep(20);
            }
            Thread.Sleep(100);
            
            var afterRapidBuffer = GetBufferContent(terminal);
            LogBufferContent("After Rapid Clicks", afterRapidBuffer);
            
            // Buttons MUST still be visible
            LogAssertion("Buttons should remain visible after rapid updates");
            Assert.Contains("Click Me", afterRapidBuffer);
            Assert.Contains("Reset", afterRapidBuffer);
            
            input.Stop();
            
            LogStep("Analyzing render consistency");
            LogData("Total renders", renderCount);
            
            if (renderCount < 3)
            {
                Logger.Warning($"Low render count ({renderCount}) may indicate render suppression issue");
            }
        }
    }

    [Fact]
    public void TextFieldShouldRetainContentDuringTyping()
    {
        using (BeginScenario("TextField Content Stability"))
        {
            LogStep("Setting up terminal with text field");
            
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var textValue = "";
            var renderCount = 0;
            var textFieldContentHistory = new List<string>();

            LogStep("Creating UI with TextField");
            ISimpleComponent BuildUI()
            {
                renderCount++;
                Logger.Debug($"BuildUI called - render #{renderCount}, text='{textValue}'");
                textFieldContentHistory.Add(textValue);
                
                return new VStack(spacing: 1)
                {
                    new Text("Enter your name:"),
                    new TextField("Type here...", new Binding<string>(
                        () => textValue,
                        v => 
                        {
                            Logger.Debug($"TextField binding setter: '{textValue}' -> '{v}'");
                            textValue = v;
                        },
                        "TextValue"
                    )),
                    new Text($"You typed: {textValue}"),
                    new Button("Clear", () => 
                    {
                        textValue = "";
                        renderer.RequestRender();
                    })
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
                    throw;
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(200);
            
            LogStep("Checking initial TextField render");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial", initialBuffer);
            
            Assert.Contains("Type here...", initialBuffer); // Placeholder should be visible
            
            LogStep("Focusing TextField");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            var focusedBuffer = GetBufferContent(terminal);
            LogBufferContent("After Focus", focusedBuffer);
            
            LogStep("Typing characters");
            string testText = "Hello";
            foreach (char c in testText)
            {
                Logger.Debug($"Typing character: '{c}'");
                input.EmitKey(c, ConsoleKey.H); // Key doesn't matter for text
                Thread.Sleep(50);
                
                var bufferAfterChar = GetBufferContent(terminal);
                LogData($"Buffer after '{c}'", bufferAfterChar.Contains(textValue.Length > 0 ? textValue : "Type here"));
                
                // Text should be accumulating
                if (textValue.Length > 0)
                {
                    LogAssertion($"Text '{textValue}' should be visible after typing '{c}'");
                    Assert.Contains(textValue, bufferAfterChar);
                }
            }
            
            Thread.Sleep(100);
            var afterTypingBuffer = GetBufferContent(terminal);
            LogBufferContent("After Typing", afterTypingBuffer);
            
            // CRITICAL: The typed text should be visible
            LogAssertion("Full typed text should be visible");
            Assert.Contains("Hello", afterTypingBuffer);
            Assert.Contains("You typed: Hello", afterTypingBuffer);
            
            LogStep("Testing text persistence during other updates");
            // Tab to button
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            
            var afterTabAwayBuffer = GetBufferContent(terminal);
            LogBufferContent("After Tab Away", afterTabAwayBuffer);
            
            // Text should STILL be there even when focus moves
            LogAssertion("Text should persist when focus changes");
            Assert.Contains("Hello", afterTabAwayBuffer);
            
            LogStep("Analyzing text field history");
            LogData("Render count", renderCount);
            LogData("Text history count", textFieldContentHistory.Count);
            for (int i = 0; i < Math.Min(10, textFieldContentHistory.Count); i++)
            {
                LogData($"Text at render {i}", textFieldContentHistory[i]);
            }
            
            input.Stop();
        }
    }

    [Fact]
    public void ComplexFormShouldMaintainAllElements()
    {
        using (BeginScenario("Complex Form Rendering Stability"))
        {
            LogStep("Setting up complex form with multiple interactive elements");
            
            var terminal = new MockTerminal(100, 40);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);
            
            var formData = new
            {
                Name = "",
                Email = "",
                Message = "",
                Subscribe = false
            };
            var nameValue = "";
            var emailValue = "";
            var messageValue = "";
            var renderCount = 0;
            var elementVisibilityHistory = new List<(int render, bool hasButtons, bool hasFields)>();

            LogStep("Creating complex form UI");
            ISimpleComponent BuildUI()
            {
                renderCount++;
                var hasButtons = false;
                var hasFields = false;
                
                var ui = new VStack(spacing: 1)
                {
                    new Text("=== Contact Form ===").Bold(),
                    new HStack(spacing: 1)
                    {
                        new Text("Name: "),
                        new TextField("Your name", new Binding<string>(
                            () => nameValue,
                            v => { nameValue = v; hasFields = true; },
                            "Name"
                        ))
                    },
                    new HStack(spacing: 1)
                    {
                        new Text("Email: "),
                        new TextField("your@email.com", new Binding<string>(
                            () => emailValue,
                            v => { emailValue = v; hasFields = true; },
                            "Email"
                        ))
                    },
                    new Text("Message:"),
                    new TextArea("Type your message here...", new Binding<string>(
                        () => messageValue,
                        v => messageValue = v,
                        "Message"
                    )).Rows(5).Cols(40),
                    new HStack(spacing: 2)
                    {
                        new Button("Submit", () => 
                        {
                            hasButtons = true;
                            Logger.Info($"Submit: Name={nameValue}, Email={emailValue}");
                            renderer.RequestRender();
                        }).Primary(),
                        new Button("Clear", () => 
                        {
                            hasButtons = true;
                            nameValue = "";
                            emailValue = "";
                            messageValue = "";
                            renderer.RequestRender();
                        }).Secondary(),
                        new Button("Cancel", () => 
                        {
                            hasButtons = true;
                            Logger.Info("Cancel clicked");
                        })
                    }
                };
                
                elementVisibilityHistory.Add((renderCount, hasButtons, hasFields));
                Logger.Debug($"Render #{renderCount}: buttons={hasButtons}, fields={hasFields}");
                
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
                    throw;
                }
            })
            { IsBackground = true };
            rendererThread.Start();

            Thread.Sleep(300);
            
            LogStep("Checking initial form render");
            var initialBuffer = GetBufferContent(terminal);
            LogBufferContent("Initial Form", initialBuffer);
            
            // All elements should be present
            AssertFormElementsPresent(initialBuffer, "initial render");
            
            LogStep("Interacting with form fields");
            // Tab to first field
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            input.EmitKey('J', ConsoleKey.J);
            input.EmitKey('o', ConsoleKey.O);
            input.EmitKey('h', ConsoleKey.H);
            input.EmitKey('n', ConsoleKey.N);
            Thread.Sleep(100);
            
            var afterNameBuffer = GetBufferContent(terminal);
            LogBufferContent("After Name Entry", afterNameBuffer);
            
            // All elements should STILL be present
            AssertFormElementsPresent(afterNameBuffer, "after name entry");
            Assert.Contains("John", afterNameBuffer);
            
            LogStep("Tabbing through all fields");
            for (int i = 0; i < 5; i++)
            {
                input.EmitKey('\t', ConsoleKey.Tab);
                Thread.Sleep(50);
                
                var bufferAfterTab = GetBufferContent(terminal);
                AssertFormElementsPresent(bufferAfterTab, $"after tab {i+1}");
            }
            
            LogStep("Triggering re-renders via button clicks");
            // Should be on a button now
            input.EmitKey('\r', ConsoleKey.Enter);
            Thread.Sleep(100);
            
            var afterButtonBuffer = GetBufferContent(terminal);
            LogBufferContent("After Button Click", afterButtonBuffer);
            
            // CRITICAL: All form elements must still be visible
            AssertFormElementsPresent(afterButtonBuffer, "after button click");
            
            LogStep("Analyzing rendering consistency");
            LogData("Total renders", renderCount);
            LogData("Element visibility history entries", elementVisibilityHistory.Count);
            
            // Check for rendering dropouts
            var missingElementRenders = 0;
            foreach (var (render, hasButtons, hasFields) in elementVisibilityHistory)
            {
                if (!hasButtons || !hasFields)
                {
                    missingElementRenders++;
                    Logger.Warning($"Render {render}: Missing elements - buttons={hasButtons}, fields={hasFields}");
                }
            }
            
            if (missingElementRenders > 0)
            {
                Logger.Error($"Found {missingElementRenders} renders with missing elements!");
            }
            
            input.Stop();
            
            // Export logs if there were issues
            if (missingElementRenders > 0)
            {
                ExportLogsOnFailure(new Exception($"Missing elements in {missingElementRenders} renders"));
            }
        }
    }

    private void AssertFormElementsPresent(string buffer, string context)
    {
        LogAssertion($"Checking all form elements are present in {context}");
        
        // Check for form title
        Assert.Contains("Contact Form", buffer);
        
        // Check for field labels
        Assert.Contains("Name:", buffer);
        Assert.Contains("Email:", buffer);
        Assert.Contains("Message:", buffer);
        
        // Check for buttons - CRITICAL
        LogAssertion($"Buttons must be visible in {context}");
        Assert.Contains("Submit", buffer);
        Assert.Contains("Clear", buffer);
        Assert.Contains("Cancel", buffer);
        
        // Check for field brackets (indicates TextField rendering)
        Assert.Contains("[", buffer);
        Assert.Contains("]", buffer);
    }

    private string GetBufferContent(MockTerminal terminal)
    {
        var content = "";
        for (int y = 0; y < terminal.Height; y++)
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
        _output.WriteLine($"\n=== Buffer: {label} ===");
        var lines = content.Split('\n');
        for (int i = 0; i < Math.Min(20, lines.Length); i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                _output.WriteLine($"  {i:D2}: {lines[i]}");
            }
        }
        _output.WriteLine("=== End Buffer ===\n");
    }
}