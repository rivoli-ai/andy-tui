using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Core.Diagnostics;

class TestEnhancedInput
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.IsDebugEnabled = true;
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        Console.WriteLine("Testing Enhanced Input Handler");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        // This enters alternate screen
        renderingSystem.Initialize();
        
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("Testing Enhanced Input in alternate screen:");
        terminal.MoveCursor(0, 2);
        terminal.Write("Press keys (Ctrl+C to exit):");
        terminal.Flush();
        
        var handler = new EnhancedConsoleInputHandler();
        int keyCount = 0;
        
        handler.KeyPressed += (s, e) => 
        {
            terminal.MoveCursor(0, 4 + keyCount);
            terminal.Write($"Key {++keyCount}: {e.Key} (Char: '{e.KeyChar}')");
            terminal.Flush();
            
            if (e.Key == ConsoleKey.C && e.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                Environment.Exit(0);
            }
        };
        
        handler.Start();
        
        // Keep alive
        while (true)
        {
            Thread.Sleep(100);
        }
    }
}