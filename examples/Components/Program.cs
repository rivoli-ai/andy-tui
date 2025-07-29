using System;
using Andy.TUI.Examples.Components;

namespace Andy.TUI.Examples.Components;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            ComponentExample.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        
        Console.WriteLine("\nExample completed successfully!");
    }
}