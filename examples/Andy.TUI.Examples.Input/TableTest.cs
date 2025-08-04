using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Examples.Input;

class TableTestApp
{
    // Sample data
    private readonly List<Product> products = new()
    {
        new Product { Id = 1, Name = "Laptop", Category = "Electronics", Price = 999.99m, Stock = 15 },
        new Product { Id = 2, Name = "Mouse", Category = "Electronics", Price = 29.99m, Stock = 50 },
        new Product { Id = 3, Name = "Keyboard", Category = "Electronics", Price = 79.99m, Stock = 30 },
        new Product { Id = 4, Name = "Monitor", Category = "Electronics", Price = 299.99m, Stock = 12 },
        new Product { Id = 5, Name = "Desk Lamp", Category = "Office", Price = 45.99m, Stock = 25 },
        new Product { Id = 6, Name = "Office Chair", Category = "Office", Price = 199.99m, Stock = 8 },
        new Product { Id = 7, Name = "Notebook", Category = "Office", Price = 4.99m, Stock = 100 },
        new Product { Id = 8, Name = "Pen Set", Category = "Office", Price = 12.99m, Stock = 75 },
        new Product { Id = 9, Name = "Coffee Mug", Category = "Kitchen", Price = 8.99m, Stock = 40 },
        new Product { Id = 10, Name = "Water Bottle", Category = "Kitchen", Price = 15.99m, Stock = 35 },
        new Product { Id = 11, Name = "Headphones", Category = "Electronics", Price = 149.99m, Stock = 20 },
        new Product { Id = 12, Name = "USB Cable", Category = "Electronics", Price = 9.99m, Stock = 100 },
        new Product { Id = 13, Name = "Standing Desk", Category = "Office", Price = 399.99m, Stock = 5 },
        new Product { Id = 14, Name = "Printer Paper", Category = "Office", Price = 19.99m, Stock = 200 },
        new Product { Id = 15, Name = "Webcam", Category = "Electronics", Price = 69.99m, Stock = 18 }
    };
    
    private Optional<Product> selectedProduct = Optional<Product>.None;
    private Optional<Person> selectedPerson = Optional<Person>.None;
    
    private readonly List<Person> people = new()
    {
        new Person { Name = "Alice Johnson", Age = 28, Department = "Engineering" },
        new Person { Name = "Bob Smith", Age = 35, Department = "Sales" },
        new Person { Name = "Carol White", Age = 42, Department = "Marketing" },
        new Person { Name = "David Brown", Age = 29, Department = "Engineering" },
        new Person { Name = "Emma Davis", Age = 31, Department = "HR" }
    };
    
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        renderer.Run(() => CreateUI());
    }
    
    private ISimpleComponent CreateUI()
    {
        // Define product table columns
        var productColumns = new[]
        {
            new TableColumn<Product>("ID", p => p.Id.ToString(), width: 4, sortable: true,
                comparer: (a, b) => a.Id.CompareTo(b.Id)),
            new TableColumn<Product>("Name", p => p.Name, sortable: true,
                comparer: (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)),
            new TableColumn<Product>("Category", p => p.Category, width: 12, sortable: true,
                comparer: (a, b) => string.Compare(a.Category, b.Category, StringComparison.OrdinalIgnoreCase)),
            new TableColumn<Product>("Price", p => $"${p.Price:F2}", width: 10, sortable: true,
                comparer: (a, b) => a.Price.CompareTo(b.Price)),
            new TableColumn<Product>("Stock", p => p.Stock.ToString(), width: 6, sortable: true,
                comparer: (a, b) => a.Stock.CompareTo(b.Stock))
        };
        
        // Define person table columns
        var personColumns = new[]
        {
            new TableColumn<Person>("Name", p => p.Name, sortable: true,
                comparer: (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)),
            new TableColumn<Person>("Age", p => p.Age.ToString(), width: 5, sortable: true,
                comparer: (a, b) => a.Age.CompareTo(b.Age)),
            new TableColumn<Person>("Department", p => p.Department, sortable: true,
                comparer: (a, b) => string.Compare(a.Department, b.Department, StringComparison.OrdinalIgnoreCase))
        };
        
        return new VStack(spacing: 2) {
            new Text("Table Component Demo").Bold().Color(Color.Cyan),
            
            new Text("Product Inventory (Tab to switch tables):").Bold(),
            new Table<Product>(
                products,
                productColumns,
                this.Bind(() => selectedProduct),
                visibleRows: 8
            ),
            
            selectedProduct.TryGetValue(out var prod)
                ? new HStack(spacing: 2) {
                    new Text($"Selected: {prod.Name}").Color(Color.Green),
                    new Text($"Total Value: ${prod.Price * prod.Stock:F2}").Color(Color.Yellow)
                  }
                : new Text("No product selected").Color(Color.DarkGray),
            
            new Text("Employee List (Minimal table):").Bold(),
            new Table<Person>(
                people,
                personColumns,
                this.Bind(() => selectedPerson),
                visibleRows: 5
            ).HideBorder(),
            
            selectedPerson.TryGetValue(out var person)
                ? new Text($"Selected: {person.Name} ({person.Age} years old)").Color(Color.Green)
                : new Text("No person selected").Color(Color.DarkGray),
            
            new Text("Controls:").Bold().Color(Color.Yellow),
            new HStack(spacing: 3) {
                new VStack(spacing: 0) {
                    new Text("• Tab: Switch tables").Color(Color.Gray),
                    new Text("• ↑/↓: Navigate rows").Color(Color.Gray),
                    new Text("• Enter/Space: Select").Color(Color.Gray)
                },
                new VStack(spacing: 0) {
                    new Text("• Home/End: First/Last").Color(Color.Gray),
                    new Text("• PageUp/Down: Scroll").Color(Color.Gray),
                    new Text("• 1-5: Sort by column").Color(Color.Gray)
                }
            },
            
            new HStack(spacing: 2) {
                new Button("Clear Selection", () => {
                    selectedProduct = Optional<Product>.None;
                    selectedPerson = Optional<Person>.None;
                }).Secondary(),
                new Button("Submit", HandleSubmit).Primary()
            },
            
            new Text("• Ctrl+C to exit").Color(Color.DarkGray)
        };
    }
    
    private void HandleSubmit()
    {
        Console.Clear();
        Console.WriteLine("Selected items:");
        Console.WriteLine("===============");
        
        if (selectedProduct.TryGetValue(out var product))
        {
            Console.WriteLine($"Product: {product.Name} (${product.Price})");
        }
        else
        {
            Console.WriteLine("Product: None");
        }
        
        if (selectedPerson.TryGetValue(out var person))
        {
            Console.WriteLine($"Person: {person.Name} ({person.Department})");
        }
        else
        {
            Console.WriteLine("Person: None");
        }
        
        Environment.Exit(0);
    }
    
    // Sample data classes
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
    
    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Department { get; set; } = "";
    }
}