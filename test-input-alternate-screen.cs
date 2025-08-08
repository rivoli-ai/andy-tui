using System;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;

class TestAlternateScreen
{
    static void Main()
    {
        var terminal = new AnsiTerminal();
        var renderingSystem = new RenderingSystem(terminal);
        var handler = new ConsoleInputHandler();
        
        Console.WriteLine("Starting alternate screen test...");
        System.Threading.Thread.Sleep(1000);
        
        // Enter alternate screen
        terminal.EnterAlternateScreen();
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("In alternate screen. Press keys (Ctrl+C to exit):");
        terminal.Flush();
        
        handler.KeyPressed += (s, e) => 
        {
            terminal.MoveCursor(0, 2);
            terminal.ClearLine();
            terminal.Write($"Key pressed: {e.Key} (Char: '{e.KeyChar}')");
            terminal.Flush();
        };
        
        handler.Start();
        
        try
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
        finally
        {
            handler.Stop();
            terminal.ExitAlternateScreen();
        }
    }
}