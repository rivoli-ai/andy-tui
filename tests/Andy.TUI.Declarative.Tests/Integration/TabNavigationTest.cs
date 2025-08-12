using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Tests for tab navigation between focusable components.
/// Ensures that the first TAB focuses the first field, not the second.
/// </summary>
public class TabNavigationTest
{
    private readonly ITestOutputHelper _output;

    public TabNavigationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FirstTab_ShouldFocusFirstField_NotSecond()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string field1 = string.Empty;
        string field2 = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Two Fields").Bold(),
            new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
            new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // First TAB should focus field 1
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);

        // Type '1' - should go into field 1
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(100);

        // Verify '1' went into field 1, not field 2
        Assert.Equal("1", field1);
        Assert.Equal("", field2);

        // Second TAB should focus field 2
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);

        // Type '2' - should go into field 2
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(100);

        // Verify '2' went into field 2
        Assert.Equal("1", field1);
        Assert.Equal("2", field2);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void TabNavigation_ShouldCycleThrough_AllFields()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string field1 = string.Empty;
        string field2 = string.Empty;
        string field3 = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Three Fields").Bold(),
            new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
            new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v)),
            new TextField("Field 3", new Binding<string>(() => field3, v => field3 = v))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // TAB to field 1 and type
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('A', ConsoleKey.A);
        Thread.Sleep(50);

        // TAB to field 2 and type
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('B', ConsoleKey.B);
        Thread.Sleep(50);

        // TAB to field 3 and type
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('C', ConsoleKey.C);
        Thread.Sleep(50);

        // TAB should cycle back to field 1
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('D', ConsoleKey.D);
        Thread.Sleep(50);

        // Verify correct distribution
        Assert.Equal("AD", field1);
        Assert.Equal("B", field2);
        Assert.Equal("C", field3);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void ShiftTab_ShouldNavigateBackward()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string field1 = string.Empty;
        string field2 = string.Empty;
        string field3 = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Three Fields").Bold(),
            new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
            new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v)),
            new TextField("Field 3", new Binding<string>(() => field3, v => field3 = v))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // TAB to field 1
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // TAB to field 2
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // TAB to field 3
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('3', ConsoleKey.D3);
        Thread.Sleep(50);

        // Shift+TAB back to field 2
        input.EmitKey('\t', ConsoleKey.Tab, ConsoleModifiers.Shift);
        Thread.Sleep(50);
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);

        // Shift+TAB back to field 1
        input.EmitKey('\t', ConsoleKey.Tab, ConsoleModifiers.Shift);
        Thread.Sleep(50);
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);

        // Verify correct fields got the input
        Assert.Equal("1", field1);
        Assert.Equal("2", field2);
        Assert.Equal("3", field3);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void TabWithMixedComponents_ShouldSkipNonFocusable()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string field1 = string.Empty;
        string field2 = string.Empty;
        bool buttonClicked = false;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Mixed Components").Bold(),
            new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
            new Text("This is not focusable"),
            new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v)),
            new Text("Another non-focusable text"),
            new Button("Click Me", () => buttonClicked = true)
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // TAB to field 1
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);

        // TAB should skip the Text and go to field 2
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);

        // TAB should skip the Text and go to Button
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(50);

        // Verify
        Assert.Equal("1", field1);
        Assert.Equal("2", field2);
        Assert.True(buttonClicked);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}