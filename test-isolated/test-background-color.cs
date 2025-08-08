using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Core.Diagnostics;

class TestBackgroundColor
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.Initialize();
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        var toggle = false;
        
        renderer.Run(() => 
            new VStack(spacing: 2) {
                new Text("Background Color Test").Bold(),
                new Text("Press SPACE to toggle background color"),
                new Text(""),
                new Button(
                    toggle ? "Button (Cyan BG)" : "Button (Gray BG)", 
                    () => toggle = !toggle,
                    backgroundColor: toggle ? Color.Cyan : Color.Gray,
                    textColor: Color.White
                ),
                new Text(""),
                new Text("Press Ctrl+C to exit")
            }
        );
    }
}