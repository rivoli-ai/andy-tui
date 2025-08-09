using Andy.TUI.Examples.BubbleTea;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("BubbleTea examples (Andy.TUI)");
            Console.WriteLine("============================\n");
            Console.WriteLine("Usage: dotnet run -- <example-name>\n");
            Console.WriteLine("Available examples:");
            Console.WriteLine("  altscreen-toggle");
            Console.WriteLine("  chat");
            Console.WriteLine("  exec");
            Console.WriteLine("  focus-blur");
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "altscreen-toggle":
                AltScreenToggle.Run();
                break;
            case "chat":
                Chat.Run();
                break;
            case "exec":
                Exec.Run();
                break;
            case "focus-blur":
                FocusBlur.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {args[0]}");
                break;
        }
    }
}
