using Andy.TUI.VirtualDom;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Examples.VirtualDom;

/// <summary>
/// Demonstrates basic Virtual DOM usage including node creation, 
/// tree building, and diffing.
/// </summary>
public static class BasicVirtualDomExample
{
    public static void Run()
    {
        Console.WriteLine("=== Basic Virtual DOM Example ===\n");
        
        // Example 1: Creating basic nodes
        Console.WriteLine("1. Creating Basic Nodes:");
        
        var textNode = Text("Hello, Virtual DOM!");
        Console.WriteLine($"Text node: {textNode.Content}");
        
        var elementNode = Element("div")
            .WithId("container")
            .WithClass("main-content")
            .WithText("Simple div element")
            .Build();
        Console.WriteLine($"Element node: <{elementNode.TagName} id='{elementNode.Props["id"]}' class='{elementNode.Props["class"]}'>");
        
        // Example 2: Building a tree structure
        Console.WriteLine("\n2. Building a Tree Structure:");
        
        var tree = Div()
            .WithClass("app")
            .WithChildren(
                // Header
                Element("header")
                    .WithClass("header")
                    .WithChildren(
                        Text("My Application"),
                        Button()
                            .WithText("Menu")
                            .OnClick(() => Console.WriteLine("Menu clicked!"))
                    ),
                
                // Main content
                Element("main")
                    .WithClass("content")
                    .WithChildren(
                        Element("h1").WithText("Welcome"),
                        Element("p").WithText("This is a virtual DOM example.")
                    ),
                
                // Footer
                Element("footer")
                    .WithText("© 2024 My App")
            )
            .Build();
        
        PrintTree(tree);
        
        // Example 3: Using fragments
        Console.WriteLine("\n3. Using Fragments:");
        
        var fragment = Fragment(
            Text("First item"),
            Text("Second item"),
            Text("Third item")
        );
        
        var listWithFragment = List()
            .WithChildren(
                fragment.Children.Select(child => 
                    ListItem().WithChild(child).Build()
                ).ToArray()
            )
            .Build();
        
        PrintTree(listWithFragment);
        
        // Example 4: Diffing trees
        Console.WriteLine("\n4. Diffing Trees:");
        
        var oldTree = Div()
            .WithClass("container")
            .WithChildren(
                Span().WithText("Original text"),
                Button().WithText("Click me")
            )
            .Build();
        
        var newTree = Div()
            .WithClass("container updated")
            .WithChildren(
                Span().WithText("Updated text"),
                Button().WithText("Click me"),
                Span().WithText("New element")
            )
            .Build();
        
        var diffEngine = new DiffEngine();
        var patches = diffEngine.Diff(oldTree, newTree);
        
        Console.WriteLine($"Generated {patches.Count} patches:");
        foreach (var patch in patches)
        {
            Console.WriteLine($"  - {patch.Type} at path [{string.Join(", ", patch.Path)}]");
        }
    }
    
    private static void PrintTree(VirtualNode node, int indent = 0)
    {
        var indentStr = new string(' ', indent * 2);
        
        switch (node)
        {
            case TextNode text:
                Console.WriteLine($"{indentStr}Text: \"{text.Content}\"");
                break;
                
            case ElementNode element:
                var props = string.Join(" ", element.Props.Select(p => $"{p.Key}='{p.Value}'"));
                Console.WriteLine($"{indentStr}<{element.TagName} {props}>");
                foreach (var child in element.Children)
                {
                    PrintTree(child, indent + 1);
                }
                Console.WriteLine($"{indentStr}</{element.TagName}>");
                break;
                
            case FragmentNode fragment:
                Console.WriteLine($"{indentStr}<Fragment>");
                foreach (var child in fragment.Children)
                {
                    PrintTree(child, indent + 1);
                }
                Console.WriteLine($"{indentStr}</Fragment>");
                break;
                
            case ComponentNode component:
                Console.WriteLine($"{indentStr}<Component type={component.ComponentType.Name}>");
                break;
        }
    }
}