using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Core.Diagnostics;

class TestDirectInput
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.IsDebugEnabled = true;
        
        var terminal = new AnsiTerminal();
        terminal.EnterAlternateScreen();
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("Direct input test in alternate screen. Press keys (Ctrl+C to exit):");
        terminal.Flush();
        
        var handler = new ConsoleInputHandler();
        
        handler.KeyPressed += (s, e) => 
        {
            terminal.MoveCursor(0, 2);
            terminal.ClearLine();
            terminal.Write($"Key pressed: {e.Key} (Char: '{e.KeyChar}')");
            terminal.Flush();
            
            if (e.Key == ConsoleKey.C && e.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                Environment.Exit(0);
            }
        };
        
        handler.Start();
        
        // Keep the main thread alive
        while (true)
        {
            Thread.Sleep(100);
        }
    }
}