namespace Andy.TUI.Examples.BubbleTea;

using Andy.TUI.Terminal;

public static class Cellbuffer
{
    public static void Run()
    {
        var term = new AnsiTerminal();
        term.EnterAlternateScreen();
        term.Clear();
        Console.CursorVisible = false;

        try
        {
            Console.WriteLine("ASCII cell animation (press q to quit)");
            int width = Math.Max(10, term.Width);
            int y = 2;
            int x = 0;
            int dx = 1;

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    if (k == ConsoleKey.Q) break;
                }

                // Erase previous
                term.MoveCursor(x, y);
                term.Write(" ");

                // Update position
                x += dx;
                if (x <= 0) { x = 0; dx = 1; }
                if (x >= width - 1) { x = width - 2; dx = -1; }

                // Draw new
                term.MoveCursor(x, y);
                term.Write("●");
                term.Flush();

                System.Threading.Thread.Sleep(16); // ~60 FPS
            }
        }
        finally
        {
            Console.CursorVisible = true;
            term.ExitAlternateScreen();
        }
    }
}
