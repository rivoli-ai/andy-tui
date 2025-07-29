namespace Andy.TUI.Examples.Terminal;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Andy.TUI Examples");
        Console.WriteLine("=================\n");
        
        if (args.Length == 0)
        {
            Console.WriteLine("Available examples:");
            Console.WriteLine("\nTerminal Abstraction:");
            Console.WriteLine("  dotnet run terminal-basic  - Basic terminal operations");
            Console.WriteLine("  dotnet run terminal-style  - Text styling and colors");
            Console.WriteLine("  dotnet run terminal-buffer - Double buffering animation");
            Console.WriteLine("  dotnet run terminal-input  - Keyboard input handling");
            Console.WriteLine("  dotnet run terminal-pacman - Pac-Man animation demo");
            Console.WriteLine("  dotnet run terminal-top    - System monitor (top-like)");
            Console.WriteLine("  dotnet run terminal-banner - Ubuntu-style system banner");
            Console.WriteLine("  dotnet run terminal-matrix - Matrix digital rain screensaver");
            Console.WriteLine("\nOther:");
            Console.WriteLine("  dotnet run all            - Run all terminal examples");
            return;
        }
        
        switch (args[0].ToLower())
        {
            case "terminal-basic":
                BasicTerminalExample.Run();
                break;
            case "terminal-style":
                StyledTextExample.Run();
                break;
            case "terminal-buffer":
                DoubleBufferExample.Run();
                break;
            case "terminal-input":
                InputHandlingExample.Run();
                break;
            case "terminal-pacman":
                PacManExample.Run();
                break;
            case "terminal-top":
                SystemMonitorExample.Run();
                break;
            case "terminal-banner":
                SystemBannerExample.Run();
                break;
            case "terminal-matrix":
                MatrixScreensaverExample.Run();
                break;
            case "all":
                BasicTerminalExample.Run();
                Console.WriteLine("\n" + new string('=', 50) + "\n");
                StyledTextExample.Run();
                Console.WriteLine("\n" + new string('=', 50) + "\n");
                DoubleBufferExample.Run();
                Console.WriteLine("\n" + new string('=', 50) + "\n");
                InputHandlingExample.Run();
                Console.WriteLine("\n" + new string('=', 50) + "\n");
                PacManExample.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {args[0]}");
                Console.WriteLine("Use 'dotnet run' to see available examples");
                break;
        }
    }
}