using Andy.TUI;

namespace Andy.TUI.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Andy.TUI Examples");
        Console.WriteLine("=================\n");
        
        if (args.Length == 0 || args[0] == "observable")
        {
            ObservableSystemExample.Run();
        }
        else
        {
            Console.WriteLine("Available examples:");
            Console.WriteLine("  dotnet run observable - Run the observable system example");
        }
    }
}