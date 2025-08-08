using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Core.Diagnostics;

class TestIssueDemo
{
    static void Main()
    {
        // Enable debug logging
        DebugContext.IsDebugEnabled = true;
        var logger = DebugContext.Logger.ForCategory("TestIssueDemo");
        
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        Console.WriteLine("Testing ConsoleInputHandler issue");
        Console.WriteLine("Press any key to initialize (enters alternate screen)...");
        Console.ReadKey(true);
        
        // This enters alternate screen
        renderingSystem.Initialize();
        
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("In alternate screen. Press keys (Ctrl+C to exit):");
        terminal.MoveCursor(0, 2);
        terminal.Write("Waiting for input...");
        terminal.Flush();
        
        var handler = new ConsoleInputHandler();
        int keyCount = 0;
        
        handler.KeyPressed += (s, e) => 
        {
            logger.Info($"Key event received: {e.Key}");
            terminal.MoveCursor(0, 4 + keyCount);
            terminal.Write($"Key {++keyCount}: {e.Key} (Char: '{e.KeyChar}')");
            terminal.Flush();
            
            if (e.Key == ConsoleKey.C && e.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                Environment.Exit(0);
            }
        };
        
        handler.Start();
        logger.Info("Input handler started");
        
        // Also try direct console reading
        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        logger.Info("Direct Console.KeyAvailable returned true!");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Error checking KeyAvailable: {ex.Message}");
                }
                Thread.Sleep(100);
            }
        });
        
        // Keep alive
        while (true)
        {
            Thread.Sleep(100);
        }
    }
}