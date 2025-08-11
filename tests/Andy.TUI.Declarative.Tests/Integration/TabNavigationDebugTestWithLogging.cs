using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Tests.Common;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Declarative.Tests.Integration;

public class TabNavigationDebugTestWithLogging : TestBase
{
    public TabNavigationDebugTestWithLogging(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void TabNavigation_ShouldMoveFocusBetweenComponents_WithLogging()
    {
        using (BeginScenario("Tab Navigation Between Components"))
        {
            LogStep("Setting up terminal and rendering system");
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);

            string name = string.Empty;
            string pass = string.Empty;

            ISimpleComponent Root() => new VStack(spacing: 1)
            {
                new Text("Form").Bold(),
                new TextField("Enter your name...", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => name = v)),
                new TextField("Enter password...", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => pass = v)).Secure(),
                new Button("Submit", () => { /* no-op */ })
            };

            LogStep("Initializing rendering system");
            renderingSystem.Initialize();

            LogStep("Starting renderer thread");
            var thread = new Thread(() =>
            {
                try
                {
                    renderer.Run(Root);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Renderer thread error");
                    throw;
                }
            })
            { IsBackground = true };
            thread.Start();

            // Give time for initial render
            LogStep("Waiting for initial render");
            Thread.Sleep(100);

            // Capture initial state
            LogData("Initial name value", name);
            LogData("Initial pass value", pass);

            // Check focus manager state
            LogStep("Checking focus manager state before first TAB");
            var focusManager = renderer.GetFocusManager();
            if (focusManager != null)
            {
                LogData("FocusedComponent before TAB", focusManager.FocusedComponent?.GetType().Name);
                LogData("Focusable count", focusManager.GetFocusableComponents().Count);

                foreach (var component in focusManager.GetFocusableComponents())
                {
                    Logger.Debug($"Focusable component: {component.GetType().Name}, CanFocus: {component.CanFocus}");
                }
            }

            // Tab to focus first input
            LogStep("Pressing first TAB to focus name field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);

            // Check focus after first tab
            if (focusManager != null)
            {
                LogData("FocusedComponent after first TAB", focusManager.FocusedComponent?.GetType().Name);
            }

            // Tab to second input
            LogStep("Pressing second TAB to focus password field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);

            // Check focus after second tab
            if (focusManager != null)
            {
                LogData("FocusedComponent after second TAB", focusManager.FocusedComponent?.GetType().Name);
            }

            // Type 'A' into password field
            LogStep("Typing 'A' into password field");
            input.EmitKey('A', ConsoleKey.A);
            Thread.Sleep(50);

            // Check final state
            LogStep("Checking final state");
            LogData("Final name value", name);
            LogData("Final pass value", pass);

            // Export detailed logs before assertions
            var inspector = new LogInspector();
            var report = inspector.GenerateReport();
            Logger.Debug($"Test execution report:\n{report}");

            // Ensure app is still running and state updated
            LogAssertion("Thread should still be alive");
            Assert.True(thread.IsAlive);

            LogAssertion("Name should be empty");
            Assert.Equal(string.Empty, name);

            LogAssertion("Password should be 'A'");
            Assert.Equal("A", pass);

            // Cleanup
            LogStep("Shutting down rendering system");
            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }
}

// Extension methods to access internal state for debugging
public static class DebugExtensions
{
    public static Andy.TUI.Declarative.Focus.FocusManager? GetFocusManager(this DeclarativeRenderer renderer)
    {
        var field = typeof(DeclarativeRenderer).GetField("_focusManager",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(renderer) as Andy.TUI.Declarative.Focus.FocusManager;
    }

    public static List<Andy.TUI.Declarative.IFocusable> GetFocusableComponents(this Andy.TUI.Declarative.Focus.FocusManager focusManager)
    {
        var field = typeof(Andy.TUI.Declarative.Focus.FocusManager).GetField("_focusableComponents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(focusManager) as List<Andy.TUI.Declarative.IFocusable> ?? new List<Andy.TUI.Declarative.IFocusable>();
    }
}