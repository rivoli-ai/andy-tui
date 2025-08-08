using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Core.Diagnostics;

class TestButtonFocus
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
            new VStack(spacing: 2) {
                new Text("Simple Button Focus Test").Bold(),
                new Text("Press TAB to switch focus between buttons"),
                new Text("Focused button should show in yellow"),
                new Text("Press ENTER to click the focused button"),
                new Spacer(1),
                new HStack(spacing: 4) {
                    new Button($"Button 1 (Clicks: {clickCount1})", () => clickCount1++),
                    new Button($"Button 2 (Clicks: {clickCount2})", () => clickCount2++)
                },
                new Spacer(1),
                new Text("Press Ctrl+C to exit")
            }
        );
    }
}