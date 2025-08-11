using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Tests.TestHelpers;
using Andy.TUI.Declarative.Layout;

namespace Andy.TUI.Declarative.Tests.Integration;

/// <summary>
/// Simple test with a single text field to verify keystrokes are displayed
/// </summary>
public class SingleTextFieldTest
{
    private readonly ITestOutputHelper _output;

    public SingleTextFieldTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SingleTextField_TypeText_DisplaysCorrectly()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();

        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string userInput = string.Empty;

        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Simple Text Input Test"),
                new TextField("Type here...", new Binding<string>(() => userInput, v => userInput = v))
            };
        }

        renderingSystem.Initialize();

        // Run renderer in background
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();

        // Give time for initial render
        Thread.Sleep(200);

        _output.WriteLine("=== Single TextField Test ===");

        // Focus the text field
        _output.WriteLine("Step 1: Press TAB to focus the text field");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type "Hello"
        _output.WriteLine("Step 2: Type 'Hello'");
        input.EmitKey('H', ConsoleKey.H);
        Thread.Sleep(50);
        _output.WriteLine($"After 'H': userInput='{userInput}'");

        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(50);
        _output.WriteLine($"After 'e': userInput='{userInput}'");

        input.EmitKey('l', ConsoleKey.L);
        Thread.Sleep(50);
        _output.WriteLine($"After 'l': userInput='{userInput}'");

        input.EmitKey('l', ConsoleKey.L);
        Thread.Sleep(50);
        _output.WriteLine($"After second 'l': userInput='{userInput}'");

        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        _output.WriteLine($"After 'o': userInput='{userInput}'");

        // Assert
        Assert.Equal("Hello", userInput);
        _output.WriteLine($"✓ Final result: userInput='{userInput}'");

        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }

    [Fact]
    public void SingleTextField_TypeAndBackspace_Works()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();

        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string text = string.Empty;

        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Backspace Test"),
                new TextField("", new Binding<string>(() => text, v => text = v))
            };
        }

        renderingSystem.Initialize();

        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();

        Thread.Sleep(200);

        _output.WriteLine("=== Backspace Test ===");

        // Focus field
        _output.WriteLine("Focus field");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type "Test"
        _output.WriteLine("Type 'Test'");
        input.EmitKey('T', ConsoleKey.T);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('s', ConsoleKey.S);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(50);
        _output.WriteLine($"After typing: text='{text}'");

        // Backspace twice
        _output.WriteLine("Press backspace twice");
        input.EmitKey('\b', ConsoleKey.Backspace);
        Thread.Sleep(50);
        _output.WriteLine($"After 1st backspace: text='{text}'");

        input.EmitKey('\b', ConsoleKey.Backspace);
        Thread.Sleep(50);
        _output.WriteLine($"After 2nd backspace: text='{text}'");

        // Type "xt"
        _output.WriteLine("Type 'xt'");
        input.EmitKey('x', ConsoleKey.X);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(50);
        _output.WriteLine($"Final: text='{text}'");

        // Assert
        Assert.Equal("Text", text);

        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }

    [Fact]
    public void SingleTextField_SpecialCharacters_Displayed()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();

        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string email = string.Empty;

        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Email Input"),
                new HStack(spacing: 1)
                {
                    new Text("Email:"),
                    new TextField("user@example.com", new Binding<string>(() => email, v => email = v))
                }
            };
        }

        renderingSystem.Initialize();

        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();

        Thread.Sleep(200);

        _output.WriteLine("=== Special Characters Test ===");

        // Focus field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type an email address
        _output.WriteLine("Type 'user@test.com'");
        input.EmitKey('u', ConsoleKey.U);
        input.EmitKey('s', ConsoleKey.S);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('r', ConsoleKey.R);
        Thread.Sleep(50);
        _output.WriteLine($"After 'user': email='{email}'");

        // @ symbol (Shift+2 on most keyboards)
        input.EmitKey('@', ConsoleKey.D2, ConsoleModifiers.Shift);
        Thread.Sleep(50);
        _output.WriteLine($"After '@': email='{email}'");

        input.EmitKey('t', ConsoleKey.T);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('s', ConsoleKey.S);
        input.EmitKey('t', ConsoleKey.T);
        Thread.Sleep(50);

        // Period
        input.EmitKey('.', ConsoleKey.OemPeriod);
        Thread.Sleep(50);
        _output.WriteLine($"After '.': email='{email}'");

        input.EmitKey('c', ConsoleKey.C);
        input.EmitKey('o', ConsoleKey.O);
        input.EmitKey('m', ConsoleKey.M);
        Thread.Sleep(50);

        _output.WriteLine($"Final email: '{email}'");

        // Assert
        Assert.Equal("user@test.com", email);
        Assert.Contains("@", email);
        Assert.Contains(".", email);

        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }

    [Fact]
    public void SingleTextField_Numbers_Displayed()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();

        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string phoneNumber = string.Empty;

        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Phone Number Input"),
                new TextField("Enter phone number", new Binding<string>(() => phoneNumber, v => phoneNumber = v))
            };
        }

        renderingSystem.Initialize();

        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();

        Thread.Sleep(200);

        _output.WriteLine("=== Numbers Test ===");

        // Focus field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type phone number
        _output.WriteLine("Type '555-1234'");
        input.EmitKey('5', ConsoleKey.D5);
        input.EmitKey('5', ConsoleKey.D5);
        input.EmitKey('5', ConsoleKey.D5);
        Thread.Sleep(50);
        _output.WriteLine($"After '555': phoneNumber='{phoneNumber}'");

        input.EmitKey('-', ConsoleKey.OemMinus);
        Thread.Sleep(50);
        _output.WriteLine($"After '-': phoneNumber='{phoneNumber}'");

        input.EmitKey('1', ConsoleKey.D1);
        input.EmitKey('2', ConsoleKey.D2);
        input.EmitKey('3', ConsoleKey.D3);
        input.EmitKey('4', ConsoleKey.D4);
        Thread.Sleep(50);

        _output.WriteLine($"Final phone: '{phoneNumber}'");

        // Assert
        Assert.Equal("555-1234", phoneNumber);

        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }

    [Fact]
    public void SingleTextField_MixedCase_PreservesCase()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();

        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string name = string.Empty;

        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Name Input (Mixed Case)"),
                new TextField("", new Binding<string>(() => name, v => name = v))
            };
        }

        renderingSystem.Initialize();

        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();

        Thread.Sleep(200);

        _output.WriteLine("=== Mixed Case Test ===");

        // Focus field
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);

        // Type "John Smith" with proper capitalization
        _output.WriteLine("Type 'John Smith'");

        // Capital J (Shift+J)
        input.EmitKey('J', ConsoleKey.J, ConsoleModifiers.Shift);
        input.EmitKey('o', ConsoleKey.O);
        input.EmitKey('h', ConsoleKey.H);
        input.EmitKey('n', ConsoleKey.N);
        Thread.Sleep(50);
        _output.WriteLine($"After 'John': name='{name}'");

        // Space
        input.EmitKey(' ', ConsoleKey.Spacebar);
        Thread.Sleep(50);

        // Capital S (Shift+S)
        input.EmitKey('S', ConsoleKey.S, ConsoleModifiers.Shift);
        input.EmitKey('m', ConsoleKey.M);
        input.EmitKey('i', ConsoleKey.I);
        input.EmitKey('t', ConsoleKey.T);
        input.EmitKey('h', ConsoleKey.H);
        Thread.Sleep(50);

        _output.WriteLine($"Final name: '{name}'");

        // Assert
        Assert.Equal("John Smith", name);
        Assert.Contains(" ", name); // Has space
        Assert.True(char.IsUpper(name[0])); // First letter is capital
        Assert.True(char.IsUpper(name[5])); // S in Smith is capital

        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
}