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

public class TwoTextFieldsInputTest
{
    private readonly ITestOutputHelper _output;
    
    public TwoTextFieldsInputTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void TwoTextFields_TypeAndTab_ShouldDisplayCorrectText()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        // State for the two text fields
        string firstName = string.Empty;
        string lastName = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Enter Your Information").Bold(),
                new HStack(spacing: 2)
                {
                    new Text("First Name:"),
                    new TextField("Enter first name", 
                        new Binding<string>(() => firstName, v => firstName = v))
                },
                new HStack(spacing: 2)
                {
                    new Text("Last Name:"),
                    new TextField("Enter last name", 
                        new Binding<string>(() => lastName, v => lastName = v))
                }
            };
        }
        
        renderingSystem.Initialize();
        
        // Run renderer in background
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        // Give time for initial render
        Thread.Sleep(200);
        
        // Act - Type in first field
        // Note: The focus might start at a different position, so we'll TAB multiple times
        // to ensure we're at the beginning of the focus cycle
        _output.WriteLine("=== Test Step 1: Focus first field with TAB (multiple times to ensure correct position) ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        // Now we should be back at the first field (after cycling through)
        
        _output.WriteLine("=== Test Step 2: Type 'John' in first field ===");
        input.EmitKey('J', ConsoleKey.J);
        Thread.Sleep(50);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        input.EmitKey('h', ConsoleKey.H);
        Thread.Sleep(50);
        input.EmitKey('n', ConsoleKey.N);
        Thread.Sleep(50);
        
        _output.WriteLine($"First name after typing: '{firstName}'");
        _output.WriteLine($"Last name after typing: '{lastName}'");
        
        _output.WriteLine("=== Test Step 3: TAB to second field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Test Step 4: Type 'Doe' in second field ===");
        input.EmitKey('D', ConsoleKey.D);
        Thread.Sleep(50);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        input.EmitKey('e', ConsoleKey.E);
        Thread.Sleep(50);
        
        _output.WriteLine($"First name after second field: '{firstName}'");
        _output.WriteLine($"Last name after second field: '{lastName}'");
        
        // Assert
        Assert.Equal("John", firstName);
        Assert.Equal("Doe", lastName);
        
        // Verify the text is also displayed in the terminal buffer
        var bufferContent = GetBufferContent(terminal);
        _output.WriteLine("=== Terminal Buffer Content ===");
        _output.WriteLine(bufferContent);
        
        // Note: MockTerminal doesn't expose buffer content directly
        // We're verifying through the state variables instead
        // In a real implementation, we'd check the rendered output
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
    
    [Fact]
    public void TwoTextFields_TypeWithBackspace_ShouldCorrectText()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string username = string.Empty;
        string email = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("User Registration").Bold(),
                new HStack(spacing: 2)
                {
                    new Text("Username:"),
                    new TextField("Enter username", 
                        new Binding<string>(() => username, v => username = v))
                },
                new HStack(spacing: 2)
                {
                    new Text("Email:"),
                    new TextField("Enter email", 
                        new Binding<string>(() => email, v => email = v))
                }
            };
        }
        
        renderingSystem.Initialize();
        
        // Run renderer in background
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        // Give time for initial render
        Thread.Sleep(200);
        
        // Act - Type with mistakes and corrections
        _output.WriteLine("=== Focus first field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Type 'userr' (with typo) ===");
        input.EmitKey('u', ConsoleKey.U);
        input.EmitKey('s', ConsoleKey.S);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('r', ConsoleKey.R);
        input.EmitKey('r', ConsoleKey.R); // Typo
        Thread.Sleep(50);
        
        _output.WriteLine($"Username before backspace: '{username}'");
        
        _output.WriteLine("=== Press backspace to correct ===");
        input.EmitKey('\b', ConsoleKey.Backspace);
        Thread.Sleep(50);
        
        _output.WriteLine($"Username after backspace: '{username}'");
        
        _output.WriteLine("=== TAB to email field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Type email ===");
        input.EmitKey('t', ConsoleKey.T);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('s', ConsoleKey.S);
        input.EmitKey('t', ConsoleKey.T);
        input.EmitKey('@', ConsoleKey.D2, ConsoleModifiers.Shift);
        input.EmitKey('e', ConsoleKey.E);
        input.EmitKey('x', ConsoleKey.X);
        input.EmitKey('.', ConsoleKey.OemPeriod);
        input.EmitKey('c', ConsoleKey.C);
        input.EmitKey('o', ConsoleKey.O);
        Thread.Sleep(50);
        
        _output.WriteLine($"Final username: '{username}'");
        _output.WriteLine($"Final email: '{email}'");
        
        // Assert
        Assert.Equal("user", username);
        Assert.Equal("test@ex.co", email);
        
        // Verify display in buffer
        var bufferContent = GetBufferContent(terminal);
        _output.WriteLine("=== Terminal Buffer Content ===");
        _output.WriteLine(bufferContent);
        
        // Note: MockTerminal doesn't expose buffer content directly
        // We're verifying through the state variables instead
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
    
    [Fact]
    public void TwoTextFields_ShiftTabNavigation_ShouldMoveBackwards()
    {
        // Arrange
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        
        var renderer = new DeclarativeRenderer(renderingSystem, input);
        
        string field1 = string.Empty;
        string field2 = string.Empty;
        
        ISimpleComponent Root()
        {
            return new VStack(spacing: 1)
            {
                new Text("Navigation Test").Bold(),
                new TextField("Field 1", new Binding<string>(() => field1, v => field1 = v)),
                new TextField("Field 2", new Binding<string>(() => field2, v => field2 = v))
            };
        }
        
        renderingSystem.Initialize();
        
        var renderThread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        renderThread.Start();
        
        Thread.Sleep(200);
        
        // Act - Navigate forward then backward
        _output.WriteLine("=== TAB to first field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== TAB to second field ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Type '2' in second field ===");
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Shift+TAB back to first field ===");
        input.EmitKey('\t', ConsoleKey.Tab, ConsoleModifiers.Shift);
        Thread.Sleep(50);
        
        _output.WriteLine("=== Type '1' in first field ===");
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);
        
        _output.WriteLine($"Field 1: '{field1}'");
        _output.WriteLine($"Field 2: '{field2}'");
        
        // Assert
        Assert.Equal("1", field1);
        Assert.Equal("2", field2);
        
        // Cleanup
        renderingSystem.Shutdown();
        renderThread.Join(100);
    }
    
    private string GetBufferContent(MockTerminal terminal)
    {
        var lines = new System.Text.StringBuilder();
        for (int y = 0; y < Math.Min(10, terminal.Height); y++)
        {
            var line = new System.Text.StringBuilder();
            for (int x = 0; x < terminal.Width; x++)
            {
                // MockTerminal doesn't have GetCell, so we'll just use spaces
                // In a real test, we'd need to access the buffer through the rendering system
                line.Append(' ');
            }
            var trimmedLine = line.ToString().TrimEnd();
            if (!string.IsNullOrEmpty(trimmedLine))
            {
                lines.AppendLine(trimmedLine);
            }
        }
        return lines.ToString();
    }
}