using System;
using System.Threading;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Core.Diagnostics;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Examples.Input;

class Program
{
    static void Main(string[] args)
    {
        // Initialize debug logging if enabled
        DebugContext.Initialize();

        // Remove Console.Error writes to avoid corrupting terminal output

        Console.Clear();
        Console.WriteLine("Andy.TUI Declarative Examples");
        Console.WriteLine("==============================\n");
        Console.WriteLine("Choose an example to run:\n");
        Console.WriteLine("1. Input Demo (TextField, Button, Dropdown)");
        Console.WriteLine("2. Grid Test (CSS Grid-like layout)");
        Console.WriteLine("3. Spacer Test (Flexible space distribution)");
        Console.WriteLine("4. ZStack Test (Layered layouts)");
        Console.WriteLine("5. Overflow Test (Clipping and scrolling)");
        Console.WriteLine("6. Flex Basis Test (Flex sizing)");
        Console.WriteLine("7. Flex Shrink Test (Content shrinking)");
        Console.WriteLine("8. Declarative Showcase (Comprehensive demo)");
        Console.WriteLine("9. Text Wrap Test (Text wrapping and truncation)");
        Console.WriteLine("10. TextArea Test (Multi-line text input)");
        Console.WriteLine("11. SelectInput Test (Keyboard-navigable lists)");
        Console.WriteLine("12. Table Test (Sortable table with selection)");
        Console.WriteLine("13. Modal Test (Modal/Dialog system)");
        Console.WriteLine("14. Newline Test (Line break component)");
        Console.WriteLine("15. Transform Test (Text transformation)");
        Console.WriteLine("16. MultiSelectInput Test (Multiple selection lists)");
        Console.WriteLine("17. UI Components Showcase (Checkbox, RadioGroup, List, ProgressBar, Spinner)");
        Console.WriteLine("18. Code Assistant (Simulated agentic CLI)");
        Console.WriteLine("\n0. Exit");
        Console.Write("\nEnter your choice (0-17): ");

        var choice = Console.ReadLine();
        Console.Clear();

        switch (choice)
        {
            case "1":
                new InputDemoApp().Run();
                break;
            case "2":
                new GridTestApp().Run();
                break;
            case "3":
                new SpacerTestApp().Run();
                break;
            case "4":
                new ZStackTestApp().Run();
                break;
            case "5":
                new OverflowTestApp().Run();
                break;
            case "6":
                new FlexBasisTestApp().Run();
                break;
            case "7":
                new FlexShrinkTestApp().Run();
                break;
            case "8":
                new DeclarativeShowcaseApp().Run();
                break;
            case "9":
                new TextWrapTestApp().Run();
                break;
            case "10":
                new TextAreaTestApp().Run();
                break;
            case "11":
                new SelectInputTestApp().Run();
                break;
            case "12":
                new TableTestApp().Run();
                break;
            case "13":
                new ModalTestApp().Run();
                break;
            case "14":
                new NewlineTestApp().Run();
                break;
            case "15":
                new TransformTestApp().Run();
                break;
            case "16":
                new MultiSelectInputTestApp().Run();
                break;
            case "17":
                new UIComponentsShowcaseApp().Run();
                break;
            case "18":
                new CodeAssistantExample("").Run();
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid choice. Press any key to exit.");
                Console.ReadKey();
                break;
        }
    }
}

class InputDemoApp
{
    private string name = "";
    private string password = "";
    private string selectedCountry = "";

    private readonly string[] countries = { "United States", "Canada", "United Kingdom", "Germany", "France" };

    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);

        renderingSystem.Initialize();

        renderer.Run(() => CreateDeclarativeUI());
    }

    private ISimpleComponent CreateDeclarativeUI()
    {
        // 🎉 TRUE SWIFTUI-STYLE DECLARATIVE SYNTAX! 🎉
        // This is EXACTLY like SwiftUI with collection initializers and method chaining
        return new VStack(spacing: 1) {
            // Title with method chaining - just like SwiftUI!
            new Text("🚀 Andy.TUI Input Components Demo")
                .Title()
                .Color(Color.Cyan),

            " ", // Empty line
            
            // Form rows using nested layouts - pure declarative composition!
            new HStack(spacing: 2) {
                new Text("  Name:").Bold().Color(Color.White),
                new TextField("Enter your name...", this.Bind(() => name))
            },

            new HStack(spacing: 2) {
                new Text("  Pass:").Bold().Color(Color.White),
                new TextField("Enter password...", this.Bind(() => password)).Secure()
            },

            new HStack(spacing: 2) {
                new Text("Country:").Bold().Color(Color.White),
                new Dropdown<string>("Select a country...", countries, this.Bind(() => selectedCountry))
                    .Color(Color.White)
                    .PlaceholderColor(Color.Gray)
            },

            " ", // Empty line
            
            // Button row with method chaining for styling
            new HStack(spacing: 3) {
                new Button("Submit", HandleSubmit).Primary(),
                new Button("Cancel", HandleCancel).Secondary()
            },

            " ", // Empty line
            
            // Status and information section
            new Text("✅ SUCCESS: SwiftUI-like Collection Initializer Syntax!")
                .Bold()
                .Color(Color.Green),

            new Text("This is EXACTLY like SwiftUI syntax:")
                .Color(Color.Yellow),

            new Text("  • VStack(spacing: 1) { ... } collection initializers")
                .Color(Color.White),

            new Text("  • Method chaining: .Bold().Color(Color.Red)")
                .Color(Color.White),

            new Text("  • Nested layouts: HStack inside VStack")
                .Color(Color.White),

            new Text("  • Two-way binding: this.Bind(() => property)")
                .Color(Color.White),

            new Text("  • Zero AddChild() calls - pure declarative!")
                .Color(Color.White),

            " ", // Empty line
            
            // Live state display
            new Text("Current state values:")
                .Color(Color.Cyan),
            new Text($"  Name: '{name}'")
                .Color(Color.Gray),
            new Text($"  Password: '{new string('•', password.Length)}'")
                .Color(Color.Gray),
            new Text($"  Country: '{(string.IsNullOrEmpty(selectedCountry) ? "Not selected" : selectedCountry)}'")
                .Color(Color.Gray),

            " ", // Empty line
            new Text("Use [Tab] to navigate between fields, [Enter] to submit")
                .Color(Color.Yellow),
            new Text("Press Ctrl+C to exit")
                .Color(Color.DarkGray)

        };
    }

    private void HandleSubmit()
    {
        Console.Clear();
        Console.WriteLine($"Name: {name}");
        Console.WriteLine($"Password: {new string('•', password.Length)}");
        Console.WriteLine($"Country: {(string.IsNullOrEmpty(selectedCountry) ? "Not selected" : selectedCountry)}");
        Environment.Exit(0);
    }

    private void HandleCancel()
    {
        Environment.Exit(0);
    }

}

