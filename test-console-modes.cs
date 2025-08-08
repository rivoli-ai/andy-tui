using System;
using System.Threading;
using System.Threading.Tasks;
using Andy.TUI.Terminal;

class TestConsoleModes
{
    static void Main()
    {
        var terminal = new AnsiTerminal();
        
        Console.WriteLine("Testing console input modes...");
        Console.WriteLine("\n1. Testing BEFORE alternate screen:");
        
        // Test before alternate screen
        TestInputMethods("NORMAL MODE");
        
        Thread.Sleep(1000);
        
        Console.WriteLine("\n\n2. Entering alternate screen...");
        Thread.Sleep(1000);
        
        terminal.EnterAlternateScreen();
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("Testing IN alternate screen:");
        terminal.Flush();
        
        // Test in alternate screen
        TestInputMethodsInAlternate(terminal);
        
        terminal.ExitAlternateScreen();
        
        Console.WriteLine("\nTest complete.");
    }
    
    static void TestInputMethods(string mode)
    {
        Console.WriteLine($"\n{mode}:");
        Console.WriteLine("Press a key within 3 seconds...");
        
        var cts = new CancellationTokenSource(3000);
        bool keyDetected = false;
        
        Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    Console.WriteLine($"KeyAvailable detected: {key.Key}");
                    keyDetected = true;
                    break;
                }
                Thread.Sleep(10);
            }
        }).Wait();
        
        if (!keyDetected)
            Console.WriteLine("No key detected via KeyAvailable");
    }
    
    static void TestInputMethodsInAlternate(AnsiTerminal terminal)
    {
        terminal.MoveCursor(0, 2);
        terminal.Write("Press a key within 3 seconds...");
        terminal.Flush();
        
        var cts = new CancellationTokenSource(3000);
        bool keyDetected = false;
        
        Task.Run(() =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        terminal.MoveCursor(0, 4);
                        terminal.Write($"KeyAvailable detected: {key.Key}");
                        terminal.Flush();
                        keyDetected = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    terminal.MoveCursor(0, 4);
                    terminal.Write($"Error: {ex.Message}");
                    terminal.Flush();
                    break;
                }
                Thread.Sleep(10);
            }
        }).Wait();
        
        if (!keyDetected)
        {
            terminal.MoveCursor(0, 4);
            terminal.Write("No key detected via KeyAvailable");
            terminal.Flush();
        }
        
        Thread.Sleep(2000);
    }
}