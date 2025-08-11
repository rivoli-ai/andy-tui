using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Terminal;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Integration;

public class TabNavigationDiagnosticTest
{
    private readonly ITestOutputHelper _output;
    
    public TabNavigationDiagnosticTest(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void DiagnoseTabNavigation()
    {
        var terminal = new MockTerminal(80, 24);
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new TestInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        string name = string.Empty;
        string pass = string.Empty;
        string buttonClicks = string.Empty;

        ISimpleComponent Root() => new VStack(spacing: 1)
        {
            new Text("Form").Bold(),
            new TextField("Name", new Andy.TUI.Declarative.State.Binding<string>(() => name, v => name = v)),
            new TextField("Pass", new Andy.TUI.Declarative.State.Binding<string>(() => pass, v => pass = v)),
            new Button("Submit", () => buttonClicks += "CLICKED;")
        };

        renderingSystem.Initialize();
        var thread = new Thread(() => renderer.Run(Root)) { IsBackground = true };
        thread.Start();

        // Give time for initial render and focus setup
        Thread.Sleep(300);

        // Type without any TAB - where does it go?
        _output.WriteLine("=== Typing '0' without TAB ===");
        input.EmitKey('0', ConsoleKey.D0);
        Thread.Sleep(50);
        _output.WriteLine($"Name: '{name}', Pass: '{pass}', Button: '{buttonClicks}'");
        
        // First TAB then type
        _output.WriteLine("=== TAB once, then type '1' ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('1', ConsoleKey.D1);
        Thread.Sleep(50);
        _output.WriteLine($"Name: '{name}', Pass: '{pass}', Button: '{buttonClicks}'");
        
        // Second TAB then type
        _output.WriteLine("=== TAB again, then type '2' ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('2', ConsoleKey.D2);
        Thread.Sleep(50);
        _output.WriteLine($"Name: '{name}', Pass: '{pass}', Button: '{buttonClicks}'");
        
        // Third TAB then type and Enter
        _output.WriteLine("=== TAB again, then type '3' and Enter ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('3', ConsoleKey.D3);
        Thread.Sleep(50);
        input.EmitKey('\r', ConsoleKey.Enter);
        Thread.Sleep(50);
        _output.WriteLine($"Name: '{name}', Pass: '{pass}', Button: '{buttonClicks}'");
        
        // Fourth TAB (should cycle back) then type
        _output.WriteLine("=== TAB again (should cycle), then type '4' ===");
        input.EmitKey('\t', ConsoleKey.Tab);
        Thread.Sleep(50);
        input.EmitKey('4', ConsoleKey.D4);
        Thread.Sleep(50);
        _output.WriteLine($"Name: '{name}', Pass: '{pass}', Button: '{buttonClicks}'");
        
        _output.WriteLine("=== Final state ===");
        _output.WriteLine($"Name: '{name}', Pass: '{pass}', Button: '{buttonClicks}'");
        
        // Analyze results and fail with diagnostic info
        string diagnosis;
        if (name.Length == 0 && pass.Length > 0)
        {
            diagnosis = "All input going to password field - TAB not working or initial focus wrong";
        }
        else if (name.Length > 0 && pass.Length > 0)
        {
            diagnosis = "TAB appears to be working - input distributed across fields";
        }
        else if (name.Length > 0 && pass.Length == 0)
        {
            diagnosis = "All input going to name field - TAB might not be cycling";
        }
        else
        {
            diagnosis = "No input received by any field";
        }
        
        // Force failure to see output
        Assert.Fail($"DIAGNOSIS: {diagnosis}\nName: '{name}'\nPass: '{pass}'\nButton: '{buttonClicks}'");
        
        // Cleanup
        renderingSystem.Shutdown();
        thread.Join(100);
    }
}