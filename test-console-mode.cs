using System;
using System.Runtime.InteropServices;
using Andy.TUI.Terminal;

class TestConsoleMode
{
    static void Main()
    {
        Console.WriteLine("Testing console modes and input handling...");
        Console.WriteLine($"Platform: {RuntimeInformation.RuntimeDescription}");
        Console.WriteLine($"Is Windows: {RuntimeInformation.IsOSPlatform(OSPlatform.Windows)}");
        Console.WriteLine($"Is macOS: {RuntimeInformation.IsOSPlatform(OSPlatform.OSX)}");
        Console.WriteLine($"Is Linux: {RuntimeInformation.IsOSPlatform(OSPlatform.Linux)}");
        Console.WriteLine();
        
        // Test 1: Normal mode
        Console.WriteLine("Test 1: Console.KeyAvailable in normal mode");
        Console.WriteLine("Press a key within 3 seconds...");
        
        var start = DateTime.Now;
        while ((DateTime.Now - start).TotalSeconds < 3)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                Console.WriteLine($"Got key: {key.Key}");
                break;
            }
            System.Threading.Thread.Sleep(50);
        }
        
        // Test 2: With alternate screen
        Console.WriteLine("\nTest 2: Console.KeyAvailable in alternate screen");
        Console.WriteLine("Press Enter to continue...");
        Console.ReadLine();
        
        var terminal = new AnsiTerminal();
        terminal.EnterAlternateScreen();
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        terminal.Write("In alternate screen. Press a key within 3 seconds...");
        terminal.Flush();
        
        start = DateTime.Now;
        bool keyFound = false;
        while ((DateTime.Now - start).TotalSeconds < 3)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                terminal.MoveCursor(0, 2);
                terminal.Write($"Got key: {key.Key}");
                terminal.Flush();
                keyFound = true;
                break;
            }
            System.Threading.Thread.Sleep(50);
        }
        
        if (!keyFound)
        {
            terminal.MoveCursor(0, 2);
            terminal.Write("No key detected via Console.KeyAvailable!");
            terminal.Flush();
        }
        
        System.Threading.Thread.Sleep(2000);
        terminal.ExitAlternateScreen();
        
        Console.WriteLine("\nTest complete.");
    }
}