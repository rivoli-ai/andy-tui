using System;
using Andy.TUI.Terminal;

class TestInput
{
    static void Main()
    {
        var handler = new ConsoleInputHandler();
        handler.KeyPressed += (s, e) => Console.WriteLine($"Key: {e.Key}");
        
        handler.Start();
        Console.WriteLine("Press keys (Ctrl+C to exit):");
        
        while (true)
        {
            System.Threading.Thread.Sleep(100);
        }
    }
}