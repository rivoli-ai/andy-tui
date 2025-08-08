using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Core.Diagnostics;

class TestInputFlow
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.IsDebugEnabled = true;
        var logger = DebugContext.Logger.ForCategory("TestInputFlow");
        
        Console.WriteLine("Starting input flow test...");
        Thread.Sleep(1000);
        
        var terminal = new AnsiTerminal();
        var renderingSystem = new RenderingSystem(terminal);
        
        // Don't enter alternate screen initially
        logger.Info("Creating ConsoleInputHandler without alternate screen");
        
        var handler = new ConsoleInputHandler();
        
        handler.KeyPressed += (s, e) => 
        {
            logger.Info($"Key pressed: {e.Key} (Char: '{e.KeyChar}', Modifiers: {e.Modifiers})");
            Console.WriteLine($"\nKey pressed: {e.Key} (Char: '{e.KeyChar}')");
        };
        
        handler.Start();
        logger.Info("Input handler started");
        
        Console.WriteLine("Press keys (test without alternate screen). Press 'a' to enter alternate screen:");
        
        bool inAlternateScreen = false;
        
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                logger.Info($"Direct Console.ReadKey: {key.Key}");
                
                if (key.Key == ConsoleKey.A && !inAlternateScreen)
                {
                    logger.Info("Entering alternate screen");
                    terminal.EnterAlternateScreen();
                    terminal.Clear();
                    terminal.MoveCursor(0, 0);
                    terminal.Write("In alternate screen. Press keys (Ctrl+C to exit):");
                    terminal.Flush();
                    inAlternateScreen = true;
                }
                else if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    break;
                }
            }
            
            Thread.Sleep(50);
        }
        
        handler.Stop();
        
        if (inAlternateScreen)
        {
            terminal.ExitAlternateScreen();
        }
        
        logger.Info("Test complete");
    }
}