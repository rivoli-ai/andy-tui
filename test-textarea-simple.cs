using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Core.Diagnostics;

class TestTextAreaSimple
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.IsDebugEnabled = true;
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        // Simple TextArea test
        var content = "";
        var binding = new Binding<string>(() => content, v => content = v);
        
        renderer.Run(() => 
            new VStack(spacing: 2) {
                new Text("Simple TextArea Test"),
                new Text("Type text below:"),
                new TextArea("Enter text...", binding),
                new Text($"Content: {content}")
            }
        );
    }
}