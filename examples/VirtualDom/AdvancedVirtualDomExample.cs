using Andy.TUI.VirtualDom;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Examples.VirtualDom;

/// <summary>
/// Demonstrates advanced Virtual DOM features including keyed reconciliation,
/// component nodes, and complex diffing scenarios.
/// </summary>
public static class AdvancedVirtualDomExample
{
    public static void Run()
    {
        Console.WriteLine("=== Advanced Virtual DOM Example ===\n");
        
        // Example 1: Keyed reconciliation
        Console.WriteLine("1. Keyed Reconciliation:");
        DemonstrateKeyedReconciliation();
        
        // Example 2: Component nodes
        Console.WriteLine("\n2. Component Nodes:");
        DemonstrateComponentNodes();
        
        // Example 3: Complex updates
        Console.WriteLine("\n3. Complex Updates:");
        DemonstrateComplexUpdates();
        
        // Example 4: Builder patterns
        Console.WriteLine("\n4. Advanced Builder Patterns:");
        DemonstrateBuilderPatterns();
    }
    
    private static void DemonstrateKeyedReconciliation()
    {
        // Create a list with keyed items
        var oldList = List()
            .WithChildren(
                ListItem().WithKey("item-1").WithText("Apple"),
                ListItem().WithKey("item-2").WithText("Banana"),
                ListItem().WithKey("item-3").WithText("Cherry")
            )
            .Build();
        
        // Reorder the list
        var newList = List()
            .WithChildren(
                ListItem().WithKey("item-3").WithText("Cherry"),
                ListItem().WithKey("item-1").WithText("Apple"),
                ListItem().WithKey("item-4").WithText("Date"),  // New item
                ListItem().WithKey("item-2").WithText("Banana")
            )
            .Build();
        
        var diffEngine = new DiffEngine();
        var patches = diffEngine.Diff(oldList, newList);
        
        Console.WriteLine("List reordering patches:");
        foreach (var patch in patches)
        {
            switch (patch)
            {
                case ReorderPatch reorder:
                    Console.WriteLine($"  - Reorder: {reorder.Moves.Count} moves");
                    foreach (var (from, to) in reorder.Moves)
                    {
                        Console.WriteLine($"    Move from index {from} to {to}");
                    }
                    break;
                case InsertPatch insert:
                    var insertedItem = insert.Node as ElementNode;
                    Console.WriteLine($"  - Insert: {insertedItem?.Key} at index {insert.Index}");
                    break;
                case RemovePatch remove:
                    Console.WriteLine($"  - Remove: item at index {remove.Index}");
                    break;
            }
        }
    }
    
    private static void DemonstrateComponentNodes()
    {
        // Define component props
        var userCardProps = new Dictionary<string, object?>
        {
            ["name"] = "John Doe",
            ["email"] = "john@example.com",
            ["role"] = "Developer"
        };
        
        // Create component nodes
        var component1 = Component<UserCard>(userCardProps);
        var component2 = Component<UserCard>(new Dictionary<string, object?>
        {
            ["name"] = "Jane Smith",
            ["email"] = "jane@example.com",
            ["role"] = "Designer"
        });
        
        // Build a layout with components
        var layout = VBox()
            .WithClass("user-list")
            .WithChildren(
                Label().WithText("Team Members"),
                component1,
                component2
            )
            .Build();
        
        Console.WriteLine("Component-based layout created:");
        PrintComponentTree(layout);
    }
    
    private static void DemonstrateComplexUpdates()
    {
        // Create initial state
        var oldState = Box()
            .WithClass("dashboard")
            .WithChildren(
                // Stats section
                HBox()
                    .WithClass("stats")
                    .WithChildren(
                        CreateStatCard("Users", 150),
                        CreateStatCard("Revenue", 5000),
                        CreateStatCard("Orders", 42)
                    ),
                
                // Chart section
                Box()
                    .WithClass("chart")
                    .WithProp("type", "line")
                    .WithText("Chart placeholder")
            )
            .Build();
        
        // Create updated state with changes
        var newState = Box()
            .WithClass("dashboard updated")  // Class changed
            .WithChildren(
                // Stats section with updated values
                HBox()
                    .WithClass("stats")
                    .WithChildren(
                        CreateStatCard("Users", 175),      // Value updated
                        CreateStatCard("Revenue", 6200),   // Value updated
                        CreateStatCard("Orders", 42),      // Unchanged
                        CreateStatCard("Conversion", 15)   // New stat
                    ),
                
                // Chart section with different type
                Box()
                    .WithClass("chart")
                    .WithProp("type", "bar")  // Type changed
                    .WithProp("animated", true)  // New prop
                    .WithText("Updated chart")
            )
            .Build();
        
        var diffEngine = new DiffEngine();
        var patches = diffEngine.Diff(oldState, newState);
        
        Console.WriteLine($"Complex update generated {patches.Count} patches:");
        AnalyzePatches(patches);
    }
    
    private static void DemonstrateBuilderPatterns()
    {
        // Create a reusable card builder
        ElementNode CreateCard(string title, string content, Action? onClick = null)
        {
            var builder = Box()
                .WithClass("card")
                .WithPadding(10)
                .WithMargin(5)
                .WithBorder("solid")
                .WithChildren(
                    Element("h3").WithText(title),
                    Element("p").WithText(content)
                );
            
            if (onClick != null)
            {
                builder.OnClick(onClick);
            }
            
            return builder.Build();
        }
        
        // Create a form using builder patterns
        var form = VBox()
            .WithClass("form")
            .WithChildren(
                // Form title
                Label()
                    .WithText("User Registration")
                    .WithClass("form-title"),
                
                // Input fields
                CreateFormField("Name", "text"),
                CreateFormField("Email", "email"),
                CreateFormField("Password", "password"),
                
                // Buttons
                HBox()
                    .WithClass("form-actions")
                    .WithJustify("space-between")
                    .WithChildren(
                        Button()
                            .WithText("Cancel")
                            .WithClass("btn-secondary")
                            .OnClick(() => Console.WriteLine("Cancel clicked")),
                        
                        Button()
                            .WithText("Submit")
                            .WithClass("btn-primary")
                            .OnClick(() => Console.WriteLine("Submit clicked"))
                    )
            )
            .Build();
        
        Console.WriteLine("Form structure created:");
        PrintSimpleTree(form);
        
        // Demonstrate fluent API chaining
        var complexElement = Div()
            .WithKey("complex")
            .WithId("main-container")
            .WithClass("container flex-column")
            .WithStyle("padding: 20px; margin: auto;")
            .WithWidth(800)
            .WithHeight(600)
            .WithBackground("#f0f0f0")
            .WithForeground("#333333")
            .WithAlign("center")
            .WithJustify("flex-start")
            .OnClick(() => Console.WriteLine("Container clicked"))
            .OnFocus(() => Console.WriteLine("Container focused"))
            .OnBlur(() => Console.WriteLine("Container blurred"))
            .WithChildren(
                CreateCard("Welcome", "This demonstrates fluent API chaining"),
                CreateCard("Features", "All builder methods return the builder instance", 
                    () => Console.WriteLine("Feature card clicked"))
            )
            .Build();
        
        Console.WriteLine("\nComplex element with chained properties created");
        Console.WriteLine($"Element has {complexElement.Props.Count} properties set");
    }
    
    private static ElementNode CreateStatCard(string label, int value)
    {
        return Box()
            .WithClass("stat-card")
            .WithChildren(
                Label().WithText(label).WithClass("stat-label"),
                Span().WithText(value.ToString()).WithClass("stat-value")
            )
            .Build();
    }
    
    private static ElementNode CreateFormField(string label, string type)
    {
        return VBox()
            .WithClass("form-field")
            .WithChildren(
                Label().WithText(label),
                Input()
                    .WithProp("type", type)
                    .WithProp("placeholder", $"Enter {label.ToLower()}")
            )
            .Build();
    }
    
    private static void PrintComponentTree(VirtualNode node, int indent = 0)
    {
        var indentStr = new string(' ', indent * 2);
        
        if (node is ComponentNode component)
        {
            var propsStr = string.Join(", ", component.Props.Select(p => $"{p.Key}={p.Value}"));
            Console.WriteLine($"{indentStr}<{component.ComponentType.Name} {propsStr} />");
        }
        else if (node is ElementNode element)
        {
            Console.WriteLine($"{indentStr}<{element.TagName}>");
            foreach (var child in element.Children)
            {
                PrintComponentTree(child, indent + 1);
            }
            Console.WriteLine($"{indentStr}</{element.TagName}>");
        }
    }
    
    private static void PrintSimpleTree(VirtualNode node, int indent = 0)
    {
        var indentStr = new string(' ', indent * 2);
        
        if (node is ElementNode element)
        {
            var className = element.Props.ContainsKey("class") ? $".{element.Props["class"]}" : "";
            Console.WriteLine($"{indentStr}{element.TagName}{className}");
            foreach (var child in element.Children)
            {
                PrintSimpleTree(child, indent + 1);
            }
        }
        else if (node is TextNode text)
        {
            Console.WriteLine($"{indentStr}\"{text.Content}\"");
        }
    }
    
    private static void AnalyzePatches(IReadOnlyList<Patch> patches)
    {
        var patchTypes = patches.GroupBy(p => p.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() });
        
        foreach (var patchType in patchTypes)
        {
            Console.WriteLine($"  - {patchType.Count} {patchType.Type} patches");
        }
    }
}

// Mock component class for demonstration
public class UserCard
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}