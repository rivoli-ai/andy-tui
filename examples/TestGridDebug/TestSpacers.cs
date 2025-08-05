using System;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.ViewInstances;

class TestSpacers
{
    public static void Main(string[] args)
    {
        var context = new DeclarativeContext(() => { });
        
        // Test the exact scenario from HStack_WithMultipleSpacers_ShouldDistributeEvenly
        var hstack = new HStack()
        {
            new Box { Width = 50, Height = 100 },
            new Spacer(),
            new Box { Width = 50, Height = 100 },
            new Spacer(),
            new Box { Width = 50, Height = 100 }
        };
        
        var root = context.ViewInstanceManager.GetOrCreateInstance(hstack, "root");
        var constraints = LayoutConstraints.Tight(400, 100);
        
        root.CalculateLayout(constraints);
        
        var stackInstance = root as HStackInstance;
        var children = stackInstance.GetChildInstances();
        
        Console.WriteLine($"HStack size: {root.Layout.Width}x{root.Layout.Height}");
        Console.WriteLine($"Expected: 400x100");
        Console.WriteLine($"\nChild count: {children.Count}");
        Console.WriteLine("Expected: 5 (3 boxes + 2 spacers)");
        
        Console.WriteLine("\nChildren:");
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var childType = child.GetType().Name;
            Console.WriteLine($"\nChild {i} ({childType}):");
            Console.WriteLine($"  Position: X={child.Layout.X}, Y={child.Layout.Y}");
            Console.WriteLine($"  Size: Width={child.Layout.Width}, Height={child.Layout.Height}");
            
            if (i == 1 || i == 3) // Spacers
            {
                Console.WriteLine($"  Expected: Width=125 (spacer should expand)");
                if (child.Layout.Width != 125)
                {
                    Console.WriteLine($"  ❌ Spacer width is wrong!");
                }
            }
        }
        
        Console.WriteLine("\nExpected layout:");
        Console.WriteLine("Box 0: X=0, Width=50");
        Console.WriteLine("Spacer 1: X=50, Width=125");
        Console.WriteLine("Box 2: X=175, Width=50");
        Console.WriteLine("Spacer 3: X=225, Width=125");
        Console.WriteLine("Box 4: X=350, Width=50");
    }
}

// Extension to access child instances
internal static class StackTestExtensions
{
    public static System.Collections.Generic.IReadOnlyList<ViewInstance> GetChildInstances(this HStackInstance stack)
    {
        var childrenField = typeof(HStackInstance).GetField("_childInstances", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return childrenField?.GetValue(stack) as System.Collections.Generic.IReadOnlyList<ViewInstance> ?? new System.Collections.Generic.List<ViewInstance>();
    }
}