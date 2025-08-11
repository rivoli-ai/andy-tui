using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates basic terminal operations including cursor movement, colors, and text output.
/// </summary>
public class BasicTerminalExample
{
    public static void Run()
    {
        Console.WriteLine("=== Basic Terminal Example ===\n");
        
        // Create an ANSI terminal
        using var terminal = new AnsiTerminal();
        
        // Get terminal dimensions
        Console.WriteLine($"Terminal size: {terminal.Width}x{terminal.Height}");
        
        // Save current position for later
        terminal.SaveCursorPosition();
        
        // Clear screen and move to top
        terminal.Clear();
        terminal.MoveCursor(0, 0);
        
        // Write colored text
        terminal.SetForegroundColor(ConsoleColor.Green);
        terminal.Write("Green text");
        
        terminal.SetForegroundColor(ConsoleColor.Red);
        terminal.SetBackgroundColor(ConsoleColor.White);
        terminal.Write(" Red on white ");
        
        terminal.ResetColors();
        terminal.WriteLine(" Normal text");
        
        // Move cursor around
        terminal.MoveCursor(10, 5);
        terminal.Write("Text at (10, 5)");
        
        terminal.MoveCursor(20, 10);
        terminal.SetForegroundColor(ConsoleColor.Blue);
        terminal.Write("Blue text at (20, 10)");
        
        // Clear a line
        terminal.MoveCursor(0, 15);
        terminal.Write("This line will be partially cleared");
        terminal.MoveCursor(10, 15);
        terminal.ClearLine();
        
        // Hide/show cursor
        terminal.CursorVisible = false;
        terminal.MoveCursor(0, 20);
        terminal.Write("Cursor is hidden");
        terminal.Flush();
        
        Thread.Sleep(2000);
        
        terminal.CursorVisible = true;
        terminal.WriteLine(" - now visible again");
        
        // Restore original position
        terminal.RestoreCursorPosition();
        terminal.ResetColors();
        terminal.Flush();
        
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }
}