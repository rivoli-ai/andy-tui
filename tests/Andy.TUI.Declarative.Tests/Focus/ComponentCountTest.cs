using System;
using System.Reflection;
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

namespace Andy.TUI.Declarative.Tests.Focus;

public class ComponentCountTest
{
    private readonly ITestOutputHelper _output;

    public ComponentCountTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifyCorrectNumberOfFocusableComponents()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();

        // We need to access the context to check the FocusManager
        DeclarativeRenderer renderer = null!;
        DeclarativeContext? context = null;

        string name = string.Empty;
        string pass = string.Empty;

        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Form").Bold(),
                new TextField("Name", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => name = v)),
                new TextField("Pass", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => pass = v)),
                new Button("Submit", () => { })
            };
        }

        renderer = new DeclarativeRenderer(renderingSystem, input);

        // Use reflection to access the private _context field
        var contextField = typeof(DeclarativeRenderer).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
        context = contextField?.GetValue(renderer) as DeclarativeContext;

        Assert.NotNull(context);

        renderingSystem.Initialize();

        // Run renderer in background
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render and component registration
        Thread.Sleep(300);

        // Access the FocusManager through reflection
        var focusManager = context.FocusManager;
        var componentsField = typeof(FocusManager).GetField("_focusableComponents", BindingFlags.NonPublic | BindingFlags.Instance);
        var components = componentsField?.GetValue(focusManager) as System.Collections.Generic.List<IFocusable>;

        Assert.NotNull(components);

        _output.WriteLine($"Number of registered focusable components: {components.Count}");
        for (int i = 0; i < components.Count; i++)
        {
            _output.WriteLine($"  [{i}] {components[i].GetType().Name}");
        }

        // We expect 3 focusable components: 2 TextFields and 1 Button
        Assert.Equal(3, components.Count);

        // Verify the types
        Assert.Contains(components, c => c.GetType().Name == "TextFieldInstance");
        Assert.Contains(components, c => c.GetType().Name == "ButtonInstance");

        // Count each type
        int textFieldCount = 0;
        int buttonCount = 0;
        foreach (var comp in components)
        {
            if (comp.GetType().Name == "TextFieldInstance") textFieldCount++;
            if (comp.GetType().Name == "ButtonInstance") buttonCount++;
        }

        _output.WriteLine($"TextFields: {textFieldCount}, Buttons: {buttonCount}");

        Assert.Equal(2, textFieldCount);
        Assert.Equal(1, buttonCount);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}