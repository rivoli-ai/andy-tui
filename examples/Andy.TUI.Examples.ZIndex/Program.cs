using Andy.TUI.Examples.ZIndex;
using System;

// Z-Index Examples Launcher
Console.WriteLine("Z-Index Examples:");
Console.WriteLine("1. TabView Example");
Console.WriteLine("2. Modal Example");
Console.WriteLine("3. Complex Layered Example");
Console.WriteLine();
Console.Write("Select example (1-3): ");

var choice = Console.ReadLine();

switch (choice)
{
    case "1":
        RunExample(TabViewExample.Create(), "TabView Z-Index Example");
        break;
    case "2":
        RunExample(ModalExample.Create(), "Modal Z-Index Example");
        break;
    case "3":
        RunExample(ComplexLayeredExample.Create(), "Complex Layered Example");
        break;
    default:
        Console.WriteLine("Invalid choice");
        break;
}

static void RunExample(Andy.TUI.Declarative.ISimpleComponent component, string title)
{
    using var app = new Andy.TUI.Terminal.TerminalApp();
    app.RunDeclarative(component);
}