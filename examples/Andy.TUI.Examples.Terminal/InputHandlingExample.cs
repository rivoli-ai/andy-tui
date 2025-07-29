using Andy.TUI.Terminal;
using System.Text;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates keyboard input handling and interactive terminal applications.
/// </summary>
public class InputHandlingExample
{
    public static void Run()
    {
        Console.WriteLine("=== Input Handling Example ===");
        Console.WriteLine("This example demonstrates keyboard input handling.");
        Console.WriteLine("Press any key to start...");
        Console.ReadKey(true);
        
        var terminal = new AnsiTerminal();
        using var renderer = new TerminalRenderer(terminal);
        using var inputHandler = new ConsoleInputHandler();
        
        // State
        var keyLog = new List<string>();
        var cursorX = renderer.Width / 2;
        var cursorY = renderer.Height / 2;
        var inputBuffer = new StringBuilder();
        var exit = false;
        
        // Setup input handlers
        inputHandler.KeyPressed += (_, e) =>
        {
            // Log the key press
            var modifiers = new List<string>();
            if (e.Control) modifiers.Add("Ctrl");
            if (e.Alt) modifiers.Add("Alt");
            if (e.Shift) modifiers.Add("Shift");
            
            var modifierStr = modifiers.Count > 0 ? $"[{string.Join("+", modifiers)}]" : "";
            keyLog.Add($"{DateTime.Now:HH:mm:ss.fff} - {e.Key} '{e.KeyChar}' {modifierStr}");
            if (keyLog.Count > 10) keyLog.RemoveAt(0);
            
            // Handle special keys
            switch (e.Key)
            {
                case ConsoleKey.Escape:
                    exit = true;
                    break;
                    
                case ConsoleKey.UpArrow:
                    cursorY = Math.Max(3, cursorY - 1);
                    break;
                    
                case ConsoleKey.DownArrow:
                    cursorY = Math.Min(renderer.Height - 4, cursorY + 1);
                    break;
                    
                case ConsoleKey.LeftArrow:
                    cursorX = Math.Max(1, cursorX - 1);
                    break;
                    
                case ConsoleKey.RightArrow:
                    cursorX = Math.Min(renderer.Width - 2, cursorX + 1);
                    break;
                    
                case ConsoleKey.Backspace:
                    if (inputBuffer.Length > 0)
                        inputBuffer.Length--;
                    break;
                    
                case ConsoleKey.Enter:
                    inputBuffer.Clear();
                    break;
                    
                default:
                    // Add printable characters to input buffer
                    if (!char.IsControl(e.KeyChar))
                    {
                        inputBuffer.Append(e.KeyChar);
                        if (inputBuffer.Length > 50)
                            inputBuffer.Remove(0, 1);
                    }
                    break;
            }
        };
        
        inputHandler.Start();
        
        // Main loop
        while (!exit)
        {
            renderer.BeginFrame();
            renderer.Clear();
            
            // Draw UI
            renderer.DrawBox(0, 0, renderer.Width, renderer.Height, BorderStyle.Single,
                Style.WithForeground(Color.DarkCyan));
            
            // Title
            var title = " Keyboard Input Demo ";
            renderer.DrawText((renderer.Width - title.Length) / 2, 0, title,
                Style.WithForeground(Color.White).WithBold());
            
            // Instructions
            renderer.DrawText(2, 2, "Use arrow keys to move cursor", Style.WithForeground(Color.Gray));
            renderer.DrawText(2, 3, "Type to add text to buffer", Style.WithForeground(Color.Gray));
            renderer.DrawText(2, 4, "Press ENTER to clear buffer", Style.WithForeground(Color.Gray));
            renderer.DrawText(2, 5, "Press ESC to exit", Style.WithForeground(Color.Gray));
            
            // Draw movable cursor
            renderer.DrawChar(cursorX, cursorY, '⊕', 
                Style.WithForeground(Color.Yellow).WithBold());
            
            // Draw cursor position
            var posText = $"Position: ({cursorX}, {cursorY})";
            renderer.DrawText(2, renderer.Height - 3, posText, 
                Style.WithForeground(Color.Green));
            
            // Draw input buffer
            var bufferLabel = "Input Buffer: ";
            renderer.DrawText(2, renderer.Height - 5, bufferLabel,
                Style.WithForeground(Color.White));
            renderer.DrawText(2 + bufferLabel.Length, renderer.Height - 5, inputBuffer.ToString(),
                Style.WithForeground(Color.Cyan));
            
            // Draw key log
            renderer.DrawBox(renderer.Width - 52, 2, 50, 13, BorderStyle.Rounded,
                Style.WithForeground(Color.DarkGray));
            renderer.DrawText(renderer.Width - 51 + 18, 2, " Key Log ",
                Style.WithForeground(Color.White));
            
            for (int i = 0; i < keyLog.Count; i++)
            {
                var logEntry = keyLog[i];
                if (logEntry.Length > 46)
                    logEntry = logEntry.Substring(0, 46) + "...";
                    
                renderer.DrawText(renderer.Width - 50, 4 + i, logEntry,
                    Style.WithForeground(Color.DarkGray));
            }
            
            renderer.EndFrame();
            
            // Small delay to reduce CPU usage
            Thread.Sleep(16); // ~60 FPS
            
            // Poll for input
            inputHandler.Poll();
        }
        
        Console.Clear();
        Console.WriteLine("\nInput handling example complete!");
    }
}