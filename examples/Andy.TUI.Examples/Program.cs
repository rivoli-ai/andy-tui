using Andy.TUI.Examples;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Available examples:");
            Console.WriteLine("  observable - Observable properties example");
            Console.WriteLine("  collection - Observable collection example");
            Console.WriteLine("  all - Run all examples");
            return;
        }

        var example = args[0].ToLower();
        
        switch (example)
        {
            case "observable":
                ObservableSystemExample.Run();
                break;
            case "collection":
                ObservableCollectionExample.Run();
                break;
            case "all":
                ObservableSystemExample.Run();
                Console.WriteLine();
                ObservableCollectionExample.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {example}");
                break;
        }
    }
}