using System;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Focus;

namespace Andy.TUI.Declarative.Tests.Integration;

public class FocusRegistrationTest
{
    private readonly ITestOutputHelper _output;

    public FocusRegistrationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Components_ShouldRegisterWithFocusManager()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var context = new DeclarativeContext(() => { });

        string name = string.Empty;
        string pass = string.Empty;

        var vstack = new VStack(spacing: 1)
        {
            new Text("Form").Bold(),
            new TextField("Enter your name...", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => name = v)),
            new TextField("Enter password...", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => pass = v)).Secure(),
            new Button("Submit", () => { /* no-op */ })
        };

        // Create instances from declarations
        var instances = context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");

        // Register focusable components in document order (this is what DeclarativeRenderer does)
        context.ViewInstanceManager.RegisterFocusableComponents(instances);

        // Debug: Check if VStack has children
        if (instances is VStackInstance vstackInstance)
        {
            var children = vstackInstance.GetChildInstances();
            _output.WriteLine($"VStack has {children.Count} children");
            foreach (var child in children)
            {
                _output.WriteLine($"  Child: {child.GetType().Name}, IsFocusable: {child is IFocusable}");
            }
        }

        // Count focusable components registered
        var focusableCount = 0;
        context.FocusManager.MoveFocus(Andy.TUI.Declarative.Focus.FocusDirection.Next);
        var firstFocused = context.FocusManager.FocusedComponent;
        if (firstFocused != null)
        {
            focusableCount++;
            _output.WriteLine($"First focused: {firstFocused.GetType().Name}");
        }

        context.FocusManager.MoveFocus(Andy.TUI.Declarative.Focus.FocusDirection.Next);
        var secondFocused = context.FocusManager.FocusedComponent;
        if (secondFocused != null && secondFocused != firstFocused)
        {
            focusableCount++;
            _output.WriteLine($"Second focused: {secondFocused.GetType().Name}");
        }

        context.FocusManager.MoveFocus(Andy.TUI.Declarative.Focus.FocusDirection.Next);
        var thirdFocused = context.FocusManager.FocusedComponent;
        if (thirdFocused != null && thirdFocused != firstFocused && thirdFocused != secondFocused)
        {
            focusableCount++;
            _output.WriteLine($"Third focused: {thirdFocused.GetType().Name}");
        }

        // We should have 3 focusable components: 2 TextFields and 1 Button
        Assert.True(focusableCount >= 3, $"Expected at least 3 focusable components, but found {focusableCount}");
    }
}