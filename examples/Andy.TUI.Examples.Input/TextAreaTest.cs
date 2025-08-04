using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Examples.Input;

class TextAreaTestApp
{
    private string content = "";
    private string notes = "This is some initial text.\nYou can edit multiple lines.\nPress Tab to switch fields.";
    
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        renderer.Run(() => CreateUI());
    }
    
    private ISimpleComponent CreateUI()
    {
        return new VStack(spacing: 2) {
            new Text("TextArea Component Demo").Bold().Color(Color.Cyan),
            
            new Text("1. Basic TextArea (5x40):").Bold(),
            new TextArea("Enter your text here...", this.Bind(() => content)),
            
            new Text($"Character count: {content.Length}").Color(Color.Gray),
            
            new Text("2. Larger TextArea with initial content (10x60):").Bold(),
            new TextArea("Notes...", this.Bind(() => notes))
                .Rows(10)
                .Cols(60),
            
            new HStack(spacing: 2) {
                new Button("Clear", () => { content = ""; notes = ""; }).Secondary(),
                new Button("Submit", HandleSubmit).Primary()
            },
            
            new Text("Navigation:").Bold().Color(Color.Yellow),
            new Text("• Arrow keys to move cursor").Color(Color.Gray),
            new Text("• Enter for new line").Color(Color.Gray),
            new Text("• Home/End for line start/end").Color(Color.Gray),
            new Text("• Ctrl+Home/End for text start/end").Color(Color.Gray),
            new Text("• Tab to switch between fields").Color(Color.Gray),
            new Text("• Ctrl+C to exit").Color(Color.Gray)
        };
    }
    
    private void HandleSubmit()
    {
        Console.Clear();
        Console.WriteLine("Submitted content:");
        Console.WriteLine("=================");
        Console.WriteLine(content);
        Console.WriteLine();
        Console.WriteLine("Notes:");
        Console.WriteLine("======");
        Console.WriteLine(notes);
        Environment.Exit(0);
    }
}