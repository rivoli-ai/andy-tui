using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Core.Diagnostics;

class TestButtonCompact
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.Initialize();
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        var clickCount1 = 0;
        var clickCount2 = 0;
        
        renderer.Run(() => 
            new VStack(spacing: 1) {
                new Text("Button Focus Test (TAB to switch, ENTER to click, Ctrl+C to exit)"),
                new HStack(spacing: 2) {
                    new Button($"Button 1 [{clickCount1}]", () => clickCount1++),
                    new Button($"Button 2 [{clickCount2}]", () => clickCount2++)
                }
            }
        );
    }
}