using Andy.TUI.Terminal;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Basic demonstration of the enhanced input system - keyboard only for safety.
/// </summary>
public class BasicInputSystemExample
{
    public static void Run()
    {
        Console.WriteLine("Enhanced Input System Demo - Basic Version");
        Console.WriteLine("=========================================");
        Console.WriteLine();
        Console.WriteLine("This demonstrates the new InputEvent system with enhanced keyboard support.");
        Console.WriteLine();
        Console.WriteLine("Instructions:");
        Console.WriteLine("- Press any key to see detailed key information");
        Console.WriteLine("- Try arrow keys, function keys (F1-F12), etc.");
        Console.WriteLine("- Try key combinations with Ctrl, Alt, Shift");
        Console.WriteLine("- Click, drag, and scroll with mouse (if supported)");
        Console.WriteLine("- NOTE: Standalone modifier keys (Shift, Caps, Cmd) are not detected due to Console API limitations");
        Console.WriteLine("- Press 'q' to quit");
        Console.WriteLine();

        var inputManager = new CrossPlatformInputManager();
        var running = true;
        var eventCount = 0;

        // Enable mouse input to test it
        if (inputManager.SupportsMouseInput)
        {
            Console.WriteLine($"Mouse support detected: {inputManager.SupportsMouseInput} - ENABLING for testing");
            inputManager.EnableMouseInput();
        }
        else
        {
            Console.WriteLine($"Mouse support detected: {inputManager.SupportsMouseInput}");
        }
        Console.WriteLine();
        Console.WriteLine("WHAT'S DETECTED:");
        Console.WriteLine("✅ Regular keys, Enter, Tab, Space, Backspace, Delete, Escape");
        Console.WriteLine("✅ Arrow keys, Function keys (F1-F12), Page Up/Down, Home/End");
        Console.WriteLine("✅ Key combinations: Ctrl+A, Alt+B, Shift+Arrow, etc.");
        Console.WriteLine("✅ Mouse clicks, drags, wheel (with modifier combinations)");
        Console.WriteLine("❌ Standalone modifier keys: Shift, Caps Lock, Cmd (Console API limitation)");
        Console.WriteLine();
        Console.WriteLine("Event Log (live scrolling):");
        Console.WriteLine("----------------------------");


        inputManager.InputReceived += (sender, e) =>
        {
            eventCount++;

            string eventText = "";

            // Handle different event types
            if (e.Type == InputEventType.KeyPress && e.Key != null)
            {
                // Check for quit
                if (e.Key.KeyChar == 'q' || e.Key.KeyChar == 'Q')
                {
                    running = false;
                    return;
                }

                // Debug all keys to see what we get for Command key
                var debugInfo = $" [DEBUG: Key={e.Key.Key}, Char='{e.Key.KeyChar}', CharCode={(int)e.Key.KeyChar}, Modifiers={e.Key.Modifiers}";
                if (e.Key.Command)
                {
                    debugInfo += ", Command=true";
                }

                // Try to detect Caps Lock state by character case
                if (char.IsLetter(e.Key.KeyChar) && !e.Key.Shift)
                {
                    var capsLockActive = char.IsUpper(e.Key.KeyChar);
                    debugInfo += $", CapsLock={capsLockActive}";
                }

                if (e.Key.EscapeSequence != null)
                {
                    debugInfo += $", EscapeSeq={e.Key.EscapeSequence}";
                }
                debugInfo += "]";

                eventText = $"#{eventCount}: KEY - {FormatKeyEvent(e.Key)}{debugInfo}";
            }
            else if (e.Type == InputEventType.MousePress && e.Mouse != null)
            {
                eventText = $"#{eventCount}: MOUSE PRESS - {FormatMouseEvent(e.Mouse)}";
            }
            else if (e.Type == InputEventType.MouseRelease && e.Mouse != null)
            {
                eventText = $"#{eventCount}: MOUSE RELEASE - {FormatMouseEvent(e.Mouse)}";
            }
            else if (e.Type == InputEventType.MouseMove && e.Mouse != null)
            {
                // Only log drag operations to reduce noise
                if (e.Mouse.IsDrag)
                {
                    eventText = $"#{eventCount}: MOUSE DRAG - {FormatMouseEvent(e.Mouse)}";
                }
                else
                {
                    return; // Skip regular mouse moves
                }
            }
            else if (e.Type == InputEventType.MouseWheel && e.Mouse != null)
            {
                eventText = $"#{eventCount}: MOUSE WHEEL - {FormatMouseEvent(e.Mouse)}";
            }
            else if (e.Type == InputEventType.Resize && e.Resize != null)
            {
                eventText = $"#{eventCount}: RESIZE - {e.Resize.Width}x{e.Resize.Height}";
            }
            else
            {
                eventText = $"#{eventCount}: UNKNOWN - {e.Type}";
            }

            if (!string.IsNullOrEmpty(eventText))
            {
                // Simply print the new event and let it scroll naturally
                Console.WriteLine(eventText);
            }
        };

        try
        {
            inputManager.Start();

            while (running)
            {
                inputManager.Poll();
                Thread.Sleep(50);
            }
        }
        finally
        {
            inputManager.Stop();
            inputManager.DisableMouseInput();
            inputManager.Dispose();
        }

        Console.WriteLine();
        Console.WriteLine($"Demo completed. Total events processed: {eventCount}");
    }

    private static string FormatKeyEvent(KeyInfo key)
    {
        var parts = new List<string>();

        // Add modifiers
        if (key.Control) parts.Add("Ctrl");
        if (key.Alt) parts.Add("Alt");
        if (key.Shift) parts.Add("Shift");
        if (key.Command) parts.Add("Cmd");

        // Add key information with special handling
        if (key.IsSpecialKey)
        {
            parts.Add($"{key.Key} [Special: {key.EscapeSequence}]");
        }
        else if (key.Key == ConsoleKey.Enter)
        {
            parts.Add($"{key.Key} [Enter/Return]");
        }
        else if (key.Key == ConsoleKey.Tab)
        {
            parts.Add($"{key.Key} [Tab]");
        }
        else if (key.Key == ConsoleKey.Spacebar)
        {
            parts.Add($"{key.Key} [Space]");
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            parts.Add($"{key.Key} [Backspace]");
        }
        else if (key.Key == ConsoleKey.Delete)
        {
            parts.Add($"{key.Key} [Delete]");
        }
        else if (key.Key == ConsoleKey.Escape)
        {
            parts.Add($"{key.Key} [Escape]");
        }
        else if (char.IsControl(key.KeyChar))
        {
            parts.Add($"{key.Key} [Control Char: \\x{(int)key.KeyChar:X2}]");
        }
        else if (key.KeyChar != '\0')
        {
            parts.Add($"{key.Key} ['{key.KeyChar}']");
        }
        else
        {
            // Handle modifier keys pressed alone
            parts.Add($"{key.Key} [Modifier Key]");
        }

        return string.Join(" + ", parts);
    }

    private static string FormatMouseEvent(MouseInfo mouse)
    {
        var parts = new List<string>();

        // Add modifiers
        if (mouse.Control) parts.Add("Ctrl");
        if (mouse.Alt) parts.Add("Alt");
        if (mouse.Shift) parts.Add("Shift");
        if (mouse.Command) parts.Add("Cmd");

        // Add button information
        if (mouse.Button != MouseButton.None)
            parts.Add($"{mouse.Button}Button");

        // Add position
        parts.Add($"({mouse.X},{mouse.Y})");

        // Add wheel information
        if (mouse.WheelDelta != 0)
            parts.Add($"Wheel:{mouse.WheelDelta}");

        // Add drag information
        if (mouse.IsDrag && mouse.DragStart.HasValue)
            parts.Add($"Drag from ({mouse.DragStart.Value.X},{mouse.DragStart.Value.Y})");

        return string.Join(" ", parts);
    }
}