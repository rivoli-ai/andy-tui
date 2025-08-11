namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Terminal;

public static class Fullscreen
{
    public static void Run()
    {
        var terminal = new AnsiTerminal();
        terminal.EnterAlternateScreen();
        Console.WriteLine("fullscreen example (alternate screen). Press any key to exit.");
        Console.ReadKey(true);
        terminal.ExitAlternateScreen();
    }
}
