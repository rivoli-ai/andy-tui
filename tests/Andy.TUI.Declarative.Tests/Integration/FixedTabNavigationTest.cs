using System;
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

/// <summary>
/// Fixed tab navigation tests that account for automatic initial focus.
/// </summary>
public class FixedTabNavigationTest : TestBase
{
    public FixedTabNavigationTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void TabNavigation_WithInitialFocus_ShouldWorkCorrectly()
    {
        using (BeginScenario("Tab Navigation with Initial Focus"))
        {
            LogStep("Setting up test environment");
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

            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);

            LogStep("Initial state - first field should already have focus");
            LogData("Initial name", name);
            LogData("Initial pass", pass);

            // First field already has focus, so type in it directly
            LogStep("Type 'NAME' in first field (already focused)");
            foreach (char c in "NAME")
            {
                input.EmitKey(c, (ConsoleKey)c);
                Thread.Sleep(30);
            }
            LogData("Name after typing", name);

            // Tab to second field
            LogStep("TAB to second field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);

            // Type in second field
            LogStep("Type 'PASS' in second field");
            foreach (char c in "PASS")
            {
                input.EmitKey(c, (ConsoleKey)c);
                Thread.Sleep(30);
            }
            LogData("Pass after typing", pass);

            LogAssertion("Name should be 'NAME'");
            Assert.Equal("NAME", name);

            LogAssertion("Pass should be 'PASS'");
            Assert.Equal("PASS", pass);

            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }

    [Fact]
    public void TwoTextFields_DirectInput_AccountingForInitialFocus()
    {
        using (BeginScenario("Two TextFields with Direct Input"))
        {
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);

            string username = string.Empty;
            string email = string.Empty;

            ISimpleComponent Root() => new VStack(spacing: 1)
            {
                new TextField("Username", new Andy.TUI.Declarative.State.Binding<string>(() => username, v => username = v)),
                new TextField("Email", new Andy.TUI.Declarative.State.Binding<string>(() => email, v => email = v))
            };

            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);

            // First field is already focused, type username
            LogStep("Type 'user' in username field (already focused)");
            foreach (char c in "user")
            {
                input.EmitKey(c, (ConsoleKey)c);
                Thread.Sleep(30);
            }

            // Tab to email field
            LogStep("TAB to email field");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);

            // Type email (simplified without special chars)
            LogStep("Type 'email' in email field");
            foreach (char c in "email")
            {
                input.EmitKey(c, (ConsoleKey)char.ToUpper(c));
                Thread.Sleep(30);
            }

            LogAssertion("Username should be 'user'");
            Assert.Equal("user", username);

            LogAssertion("Email should be 'email'");
            Assert.Equal("email", email);

            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }

    [Fact]
    public void Backspace_WithInitialFocus_ShouldWork()
    {
        using (BeginScenario("Backspace with Initial Focus"))
        {
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);

            string value = string.Empty;

            ISimpleComponent Root() => new TextField("Type here",
                new Andy.TUI.Declarative.State.Binding<string>(() => value, v => value = v));

            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);

            // Field is already focused, type directly
            LogStep("Type 'ABCD' (field already focused)");
            foreach (char c in "ABCD")
            {
                input.EmitKey(c, (ConsoleKey)c);
                Thread.Sleep(30);
            }
            LogData("After typing", value);

            // Press backspace twice
            LogStep("Press backspace twice");
            input.EmitKey('\b', ConsoleKey.Backspace);
            Thread.Sleep(50);
            input.EmitKey('\b', ConsoleKey.Backspace);
            Thread.Sleep(50);
            LogData("After backspace", value);

            LogAssertion("Should have 'AB' after two backspaces");
            Assert.Equal("AB", value);

            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }

    [Fact]
    public void TabCycling_ShouldWrapAroundProperly()
    {
        using (BeginScenario("Tab Cycling"))
        {
            var terminal = new MockTerminal(80, 24);
            using var renderingSystem = new RenderingSystem(terminal);
            var input = new TestInputHandler();
            var renderer = new DeclarativeRenderer(renderingSystem, input);

            string field1 = string.Empty;
            string field2 = string.Empty;
            string field3 = string.Empty;

            ISimpleComponent Root() => new VStack(spacing: 1)
            {
                new TextField("Field 1", new Andy.TUI.Declarative.State.Binding<string>(() => field1, v => field1 = v)),
                new TextField("Field 2", new Andy.TUI.Declarative.State.Binding<string>(() => field2, v => field2 = v)),
                new TextField("Field 3", new Andy.TUI.Declarative.State.Binding<string>(() => field3, v => field3 = v))
            };

            renderingSystem.Initialize();
            var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
            thread.Start();
            Thread.Sleep(100);

            // Start at field1 (auto-focused), type '1'
            LogStep("Type '1' in field1 (already focused)");
            input.EmitKey('1', ConsoleKey.D1);
            Thread.Sleep(50);

            // Tab to field2
            LogStep("TAB to field2");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            input.EmitKey('2', ConsoleKey.D2);
            Thread.Sleep(50);

            // Tab to field3
            LogStep("TAB to field3");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            input.EmitKey('3', ConsoleKey.D3);
            Thread.Sleep(50);

            // Tab should wrap back to field1
            LogStep("TAB to wrap back to field1");
            input.EmitKey('\t', ConsoleKey.Tab);
            Thread.Sleep(50);
            input.EmitKey('X', ConsoleKey.X);
            Thread.Sleep(50);

            LogAssertion("Field1 should be '1X'");
            Assert.Equal("1X", field1);

            LogAssertion("Field2 should be '2'");
            Assert.Equal("2", field2);

            LogAssertion("Field3 should be '3'");
            Assert.Equal("3", field3);

            renderingSystem.Shutdown();
            thread.Join(100);
        }
    }
}