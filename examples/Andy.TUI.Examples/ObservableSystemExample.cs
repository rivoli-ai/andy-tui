using Andy.TUI.Observable;

namespace Andy.TUI.Examples;

/// <summary>
/// Demonstrates the reactive observable system in Andy.TUI.
/// </summary>
public class ObservableSystemExample
{
    public static void Run()
    {
        Console.WriteLine("=== Andy.TUI Observable System Example ===\n");

        // Example 1: Basic Observable Property
        BasicObservableExample();

        // Example 2: Computed Properties
        ComputedPropertyExample();

        // Example 3: Complex Reactive System
        ComplexReactiveExample();

        // Example 4: Shopping Cart Example
        ShoppingCartExample();
    }

    private static void BasicObservableExample()
    {
        Console.WriteLine("1. Basic Observable Property:");
        Console.WriteLine("-----------------------------");

        var name = new ObservableProperty<string>("John");
        var age = new ObservableProperty<int>(30);

        // Subscribe to changes
        name.ValueChanged += (sender, args) =>
        {
            Console.WriteLine($"  Name changed from '{args.OldValue}' to '{args.NewValue}'");
        };

        age.Subscribe(value =>
        {
            Console.WriteLine($"  Age is now: {value}");
        });

        // Make changes
        name.Value = "Jane";
        age.Value = 31;
        age.Value = 32;

        Console.WriteLine();
    }

    private static void ComputedPropertyExample()
    {
        Console.WriteLine("2. Computed Properties:");
        Console.WriteLine("----------------------");

        var firstName = new ObservableProperty<string>("John");
        var lastName = new ObservableProperty<string>("Doe");

        // Create a computed property that depends on firstName and lastName
        var fullName = new ComputedProperty<string>(() => $"{firstName.Value} {lastName.Value}");

        // Observe changes to the computed property
        fullName.Observe(value =>
        {
            Console.WriteLine($"  Full name: {value}");
        });

        // Changes to dependencies trigger recomputation
        firstName.Value = "Jane";
        lastName.Value = "Smith";

        Console.WriteLine();
    }

    private static void ComplexReactiveExample()
    {
        Console.WriteLine("3. Complex Reactive System:");
        Console.WriteLine("--------------------------");

        // Temperature in Celsius
        var celsius = new ObservableProperty<double>(25.0);

        // Computed Fahrenheit
        var fahrenheit = new ComputedProperty<double>(() => celsius.Value * 9.0 / 5.0 + 32.0);

        // Computed temperature status
        var status = new ComputedProperty<string>(() =>
        {
            var temp = celsius.Value;
            return temp switch
            {
                < 0 => "Freezing",
                < 10 => "Cold",
                < 20 => "Cool",
                < 30 => "Warm",
                _ => "Hot"
            };
        });

        // Combined display
        var display = new ComputedProperty<string>(() =>
            $"{celsius.Value:F1}°C / {fahrenheit.Value:F1}°F - {status.Value}");

        display.Subscribe(value => Console.WriteLine($"  {value}"));

        // Make temperature changes
        celsius.Value = -5;
        celsius.Value = 15;
        celsius.Value = 35;

        Console.WriteLine();
    }

    private static void ShoppingCartExample()
    {
        Console.WriteLine("4. Shopping Cart Example:");
        Console.WriteLine("------------------------");

        // Create cart items with observable quantities
        var items = new[]
        {
            new CartItem("Apple", 0.50m, new ObservableProperty<int>(3)),
            new CartItem("Banana", 0.30m, new ObservableProperty<int>(5)),
            new CartItem("Orange", 0.60m, new ObservableProperty<int>(2))
        };

        // Tax rate
        var taxRate = new ObservableProperty<decimal>(0.08m);

        // Computed subtotal
        var subtotal = new ComputedProperty<decimal>(() =>
            items.Sum(item => item.Price * item.Quantity.Value));

        // Computed tax
        var tax = new ComputedProperty<decimal>(() =>
            Math.Round(subtotal.Value * taxRate.Value, 2));

        // Computed total
        var total = new ComputedProperty<decimal>(() =>
            subtotal.Value + tax.Value);

        // Display function
        void DisplayCart()
        {
            Console.WriteLine("\n  Cart Contents:");
            foreach (var item in items)
            {
                Console.WriteLine($"    {item.Name}: {item.Quantity.Value} x ${item.Price:F2} = ${item.Quantity.Value * item.Price:F2}");
            }
            Console.WriteLine($"  Subtotal: ${subtotal.Value:F2}");
            Console.WriteLine($"  Tax ({taxRate.Value:P0}): ${tax.Value:F2}");
            Console.WriteLine($"  Total: ${total.Value:F2}");
        }

        // Initial display
        DisplayCart();

        // Update quantities
        Console.WriteLine("\n  Updating apple quantity to 5...");
        items[0].Quantity.Value = 5;
        DisplayCart();

        // Update tax rate
        Console.WriteLine("\n  Changing tax rate to 10%...");
        taxRate.Value = 0.10m;
        DisplayCart();

        Console.WriteLine();
    }

    private class CartItem
    {
        public string Name { get; }
        public decimal Price { get; }
        public ObservableProperty<int> Quantity { get; }

        public CartItem(string name, decimal price, ObservableProperty<int> quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;
        }
    }
}