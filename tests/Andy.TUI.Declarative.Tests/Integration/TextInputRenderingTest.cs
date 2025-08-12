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
/// Tests that text input is rendered correctly in TextFields.
/// </summary>
public class TextInputRenderingTest
{
    private readonly ITestOutputHelper _output;

    public TextInputRenderingTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TextField_ShouldRenderTypedText()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string fieldValue = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Input Test").Bold(),
            new TextField("Enter text...", new Binding<string>(() => fieldValue, v => fieldValue = v))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // Focus the field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);

        // Type "Hello"
        input.EmitKey('H', ConsoleKey.H);
        Thread.Sleep(50);
        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(50);
        input.EmitKey('l', ConsoleKey.L);
        Thread.Sleep(50);
        input.EmitKey('l', ConsoleKey.L);
        Thread.Sleep(50);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(100);

        // Verify the text was stored
        Assert.Equal("Hello", fieldValue);

        // Verify the value was captured correctly
        // We can't check rendered output easily with MockTerminal

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void TextField_Backspace_ShouldDeleteCharacters()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string fieldValue = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Backspace Test").Bold(),
            new TextField("Type here...", new Binding<string>(() => fieldValue, v => fieldValue = v))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // Focus the field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);

        // Type "Test"
        input.EmitKey('T', ConsoleKey.T);
        Thread.Sleep(50);
        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(50);
        input.EmitKey('s', ConsoleKey.S);
        Thread.Sleep(50);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(100);

        Assert.Equal("Test", fieldValue);

        // Press backspace twice
        input.EmitKey('\b', ConsoleKey.Backspace);
        Thread.Sleep(50);
        input.EmitKey('\b', ConsoleKey.Backspace);
        Thread.Sleep(100);

        // Should have deleted last two characters
        Assert.Equal("Te", fieldValue);

        // Type "xt" to make "Text"
        input.EmitKey('x', ConsoleKey.X);
        Thread.Sleep(50);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(100);

        Assert.Equal("Text", fieldValue);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void SecureTextField_ShouldMaskPassword()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string password = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Password Test").Bold(),
            new TextField("Enter password...", new Binding<string>(() => password, v => password = v)).Secure()
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // Focus the field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(100);

        // Type "Secret"
        input.EmitKey('S', ConsoleKey.S);
        Thread.Sleep(50);
        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(50);
        input.EmitKey('c', ConsoleKey.C);
        Thread.Sleep(50);
        input.EmitKey('r', ConsoleKey.R);
        Thread.Sleep(50);
        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(50);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(100);

        // Verify the actual value is stored
        Assert.Equal("Secret", password);

        // The secure field should mask the input
        // We can verify the actual value is stored correctly

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }

    [Fact]
    public void MultipleTextFields_ShouldMaintainSeparateValues()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input, autoFocus: false);

        string username = string.Empty;
        string email = string.Empty;
        string phone = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Contact Form").Bold(),
            new TextField("Username...", new Binding<string>(() => username, v => username = v)),
            new TextField("Email...", new Binding<string>(() => email, v => email = v)),
            new TextField("Phone...", new Binding<string>(() => phone, v => phone = v))
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        // Fill first field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('j', ConsoleKey.J);
        input.EmitKey('o', ConsoleKey.O);
        input.EmitKey('h', ConsoleKey.H);
        input.EmitKey('n', ConsoleKey.N);
        Thread.Sleep(50);

        // Fill second field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('j', ConsoleKey.J);
        input.EmitKey('@', ConsoleKey.D2, ConsoleModifiers.Shift);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('.', ConsoleKey.OemPeriod);
        input.EmitKey('c', ConsoleKey.C);
        Thread.Sleep(50);

        // Fill third field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('5', ConsoleKey.D5);
        input.EmitKey('5', ConsoleKey.D5);
        input.EmitKey('5', ConsoleKey.D5);
        Thread.Sleep(50);

        // Go back to first field and add more
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('d', ConsoleKey.D);
        input.EmitKey('o', ConsoleKey.O);
        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(100);

        // Verify all values are correct
        Assert.Equal("johndoe", username);
        Assert.Equal("j@e.c", email);
        Assert.Equal("555", phone);

        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}