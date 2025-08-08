using System;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Core.Diagnostics;

class TestInputOrder
{
    static void Main()
    {
        DebugContext.IsDebugEnabled = true;
        
        Console.WriteLine("Testing input handler initialization order...");
        Console.WriteLine("Test 1: Start handler AFTER alternate screen (current approach)");
        Console.WriteLine("Press Enter to start test 1...");
        Console.ReadLine();
        
        Test1_HandlerAfterAlternateScreen();
        
        Console.WriteLine("\nTest 2: Start handler BEFORE alternate screen");
        Console.WriteLine("Press Enter to start test 2...");
        Console.ReadLine();
        
        Test2_HandlerBeforeAlternateScreen();
    }
    
    static void Test1_HandlerAfterAlternateScreen()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        // Enter alternate screen FIRST
        renderingSystem.Initialize();
        
        // Then start input handler
        var handler = new ConsoleInputHandler();
        int keyCount = 0;
        handler.KeyPressed += (s, e) => 
        {
            terminal.MoveCursor(0, 2 + keyCount);
            terminal.Write($"Test1 - Key {++keyCount}: {e.Key}");
            terminal.Flush();
        };
        handler.Start();
        
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("Test 1: Handler started AFTER alternate screen. Press keys (wait 5 sec):");
        terminal.Flush();
        
        Thread.Sleep(5000);
        
        handler.Stop();
        renderingSystem.Shutdown();
    }
    
    static void Test2_HandlerBeforeAlternateScreen()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        
        // Start input handler FIRST
        var handler = new ConsoleInputHandler();
        int keyCount = 0;
        handler.KeyPressed += (s, e) => 
        {
            terminal.MoveCursor(0, 2 + keyCount);
            terminal.Write($"Test2 - Key {++keyCount}: {e.Key}");
            terminal.Flush();
        };
        handler.Start();
        
        // Then enter alternate screen
        renderingSystem.Initialize();
        
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("Test 2: Handler started BEFORE alternate screen. Press keys (wait 5 sec):");
        terminal.Flush();
        
        Thread.Sleep(5000);
        
        handler.Stop();
        renderingSystem.Shutdown();
    }
}