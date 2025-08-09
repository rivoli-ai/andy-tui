namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Terminal;

public static class AltScreenToggle
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        bool alt = false;
        Console.WriteLine("Press 'a' to toggle alt screen, 'q' to quit");

        while (true)
        {
            var key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Q) break;
            if (key == ConsoleKey.A)
            {
                alt = !alt;
                if (alt) terminal.EnterAlternateScreen();
                else terminal.ExitAlternateScreen();

                terminal.Clear();
                Console.WriteLine(alt ? "Alternate screen ON (press 'a' to leave)" : "Alternate screen OFF (press 'a' to enter)");
            }
        }

        if (alt) terminal.ExitAlternateScreen();
    }
}
