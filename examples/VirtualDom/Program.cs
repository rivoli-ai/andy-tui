namespace Andy.TUI.Examples.VirtualDom;

/// <summary>
/// Entry point for Virtual DOM examples.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Andy.TUI Virtual DOM Examples");
        Console.WriteLine("=============================\n");
        
        if (args.Length == 0 || args[0] == "basic")
        {
            BasicVirtualDomExample.Run();
        }
        else if (args[0] == "advanced")
        {
            AdvancedVirtualDomExample.Run();
        }
        else if (args[0] == "reactive")
        {
            ReactiveVirtualDomExample.Run();
        }
        else if (args[0] == "all")
        {
            BasicVirtualDomExample.Run();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            AdvancedVirtualDomExample.Run();
            Console.WriteLine("\n" + new string('=', 50) + "\n");
            
            ReactiveVirtualDomExample.Run();
        }
        else
        {
            Console.WriteLine("Usage: dotnet run [example]");
            Console.WriteLine("Examples:");
            Console.WriteLine("  basic    - Basic Virtual DOM usage");
            Console.WriteLine("  advanced - Advanced features (keyed reconciliation, components)");
            Console.WriteLine("  reactive - Integration with Observable system");
            Console.WriteLine("  all      - Run all examples");
        }
    }
}