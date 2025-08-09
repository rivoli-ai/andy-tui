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
            Console.WriteLine("  autocomplete");
            Console.WriteLine("  cellbuffer");
            Console.WriteLine("  chat");
            Console.WriteLine("  composable-views");
            Console.WriteLine("  credit-card-form");
            Console.WriteLine("  debounce");
            Console.WriteLine("  exec");
            Console.WriteLine("  file-picker");
            Console.WriteLine("  focus-blur");
            Console.WriteLine("  fullscreen");
            Console.WriteLine("  glamour");
            Console.WriteLine("  help");
            Console.WriteLine("  http");
            Console.WriteLine("  list-default");
            Console.WriteLine("  list-fancy");
            Console.WriteLine("  list-simple");
            Console.WriteLine("  mouse");
            return;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "altscreen-toggle":
                AltScreenToggle.Run();
                break;
            case "autocomplete":
                Autocomplete.Run();
                break;
            case "cellbuffer":
                Cellbuffer.Run();
                break;
            case "chat":
                Chat.Run();
                break;
            case "composable-views":
                ComposableViews.Run();
                break;
            case "credit-card-form":
                CreditCardForm.Run();
                break;
            case "debounce":
                Debounce.Run();
                break;
            case "exec":
                Exec.Run();
                break;
            case "file-picker":
                FilePicker.Run();
                break;
            case "focus-blur":
                FocusBlur.Run();
                break;
            case "fullscreen":
                Fullscreen.Run();
                break;
            case "glamour":
                Glamour.Run();
                break;
            case "help":
                Help.Run();
                break;
            case "http":
                HttpExample.Run();
                break;
            case "list-default":
                ListDefault.Run();
                break;
            case "list-fancy":
                ListFancy.Run();
                break;
            case "list-simple":
                ListSimple.Run();
                break;
            case "mouse":
                MouseExample.Run();
                break;
            default:
                Console.WriteLine($"Unknown example: {args[0]}");
                break;
        }
    }
}
