using Andy.TUI;

namespace Andy.TUI.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Andy.TUI Examples");
        Console.WriteLine("=================\n");
        
        if (args.Length == 0)
        {
            Console.WriteLine("Available examples:");
            Console.WriteLine("  dotnet run observable - Run the observable system example");
            Console.WriteLine("  dotnet run collection - Run the observable collection example");
            Console.WriteLine("  dotnet run all       - Run all examples");
            return;
        }
        
        switch (args[0].ToLower())
        {
            case "observable":
                ObservableSystemExample.Run();
                break;
            case "collection":
                ObservableCollectionExample.Run();
                break;
            case "all":
                ObservableSystemExample.Run();
                Console.WriteLine("\n" + new string('=', 50) + "\n");
                ObservableCollectionExample.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {args[0]}");
                Console.WriteLine("Use 'dotnet run' to see available examples");
                break;
        }
    }
}