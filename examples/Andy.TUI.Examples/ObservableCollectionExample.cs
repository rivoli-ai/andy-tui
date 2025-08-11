using Andy.TUI.Observable;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Andy.TUI.Examples;

/// <summary>
/// Demonstrates the features of ObservableCollection including batch operations,
/// notification suspension, and integration with computed properties.
/// </summary>
public static class ObservableCollectionExample
{
    public static void Run()
    {
        Console.WriteLine("=== ObservableCollection Example ===\n");

        BasicOperationsExample();
        BatchOperationsExample();
        NotificationSuspensionExample();
        ComputedPropertyIntegrationExample();
        TodoListExample();
    }

    private static void BasicOperationsExample()
    {
        Console.WriteLine("1. Basic Operations:");

        var collection = new ObservableCollection<string>();

        // Subscribe to changes
        collection.CollectionChanged += (s, e) =>
        {
            Console.WriteLine($"   Collection changed: {e.Action}");
            if (e.NewItems != null)
                Console.WriteLine($"   - New items: {string.Join(", ", e.NewItems.Cast<string>())}");
            if (e.OldItems != null)
                Console.WriteLine($"   - Old items: {string.Join(", ", e.OldItems.Cast<string>())}");
        };

        collection.PropertyChanged += (s, e) =>
        {
            Console.WriteLine($"   Property changed: {e.PropertyName}");
        };

        // Add items
        Console.WriteLine("   Adding items...");
        collection.Add("Apple");
        collection.Add("Banana");
        collection.Add("Cherry");

        // Remove item
        Console.WriteLine("   Removing 'Banana'...");
        collection.Remove("Banana");

        // Replace item
        Console.WriteLine("   Replacing 'Apple' with 'Apricot'...");
        collection.Replace(0, "Apricot");

        Console.WriteLine($"   Final collection: [{string.Join(", ", collection)}]\n");
    }

    private static void BatchOperationsExample()
    {
        Console.WriteLine("2. Batch Operations:");

        var collection = new ObservableCollection<int>();
        var changeCount = 0;

        collection.CollectionChanged += (s, e) => changeCount++;

        // AddRange
        Console.WriteLine("   Adding range [1, 2, 3, 4, 5]...");
        collection.AddRange(new[] { 1, 2, 3, 4, 5 });
        Console.WriteLine($"   Change notifications: {changeCount} (batch operation fires once)");

        // RemoveAll
        Console.WriteLine("   Removing all even numbers...");
        changeCount = 0;
        var removed = collection.RemoveAll(x => x % 2 == 0);
        Console.WriteLine($"   Removed {removed} items, notifications: {changeCount}");
        Console.WriteLine($"   Remaining: [{string.Join(", ", collection)}]\n");
    }

    private static void NotificationSuspensionExample()
    {
        Console.WriteLine("3. Notification Suspension:");

        var collection = new ObservableCollection<string>();
        var notifications = new List<string>();

        collection.CollectionChanged += (s, e) =>
        {
            notifications.Add($"{e.Action} at {DateTime.Now:HH:mm:ss.fff}");
        };

        Console.WriteLine("   Performing multiple operations with suspended notifications...");
        using (collection.SuspendNotifications())
        {
            collection.Add("First");
            collection.Add("Second");
            collection.Add("Third");
            collection.Remove("Second");
            collection.Add("Fourth");
            Console.WriteLine($"   During suspension: {notifications.Count} notifications");
        }

        Console.WriteLine($"   After suspension: {notifications.Count} notification(s)");
        foreach (var notification in notifications)
        {
            Console.WriteLine($"   - {notification}");
        }
        Console.WriteLine($"   Final collection: [{string.Join(", ", collection)}]\n");
    }

    private static void ComputedPropertyIntegrationExample()
    {
        Console.WriteLine("4. Computed Property Integration:");

        var scores = new ObservableCollection<int>(new[] { 85, 92, 78, 95, 88 });

        // Create computed properties that depend on the collection
        var average = new ComputedProperty<double>(() =>
        {
            if (scores.Count == 0) return 0;
            double sum = 0;
            int count = 0;
            foreach (var score in scores)
            {
                sum += score;
                count++;
            }
            return sum / count;
        }, "Average");

        var highest = new ComputedProperty<int>(() =>
        {
            if (scores.Count == 0) return 0;
            int max = int.MinValue;
            foreach (var score in scores)
            {
                if (score > max) max = score;
            }
            return max;
        }, "Highest");

        var passing = new ComputedProperty<int>(() =>
        {
            int count = 0;
            foreach (var score in scores)
            {
                if (score >= 80) count++;
            }
            return count;
        }, "PassingCount");

        Console.WriteLine($"   Initial scores: [{string.Join(", ", scores)}]");
        Console.WriteLine($"   Average: {average.Value:F2}, Highest: {highest.Value}, Passing: {passing.Value}");

        // Add a new score
        Console.WriteLine("   Adding score: 100");
        scores.Add(100);
        Console.WriteLine($"   Average: {average.Value:F2}, Highest: {highest.Value}, Passing: {passing.Value}");

        // Remove lowest score
        Console.WriteLine("   Removing lowest score: 78");
        scores.Remove(78);
        Console.WriteLine($"   Average: {average.Value:F2}, Highest: {highest.Value}, Passing: {passing.Value}\n");
    }

    private static void TodoListExample()
    {
        Console.WriteLine("5. Real-World Example - Todo List:");

        var todos = new ObservableCollection<TodoItem>();

        // Create computed properties for statistics
        var totalCount = new ComputedProperty<int>(() =>
        {
            int count = 0;
            foreach (var _ in todos)
            {
                count++;
            }
            return count;
        });

        var completedCount = new ComputedProperty<int>(() =>
        {
            int count = 0;
            foreach (var todo in todos)
            {
                if (todo.IsCompleted) count++;
            }
            return count;
        });

        var percentComplete = new ComputedProperty<double>(() =>
            totalCount.Value > 0 ? (completedCount.Value * 100.0 / totalCount.Value) : 0);

        // Subscribe to show statistics updates
        percentComplete.ValueChanged += (s, e) =>
        {
            Console.WriteLine($"   Progress: {e.NewValue:F1}% complete ({completedCount.Value}/{totalCount.Value} tasks)");
        };

        // Add todos
        Console.WriteLine("   Adding todos...");
        todos.AddRange(new[]
        {
            new TodoItem { Title = "Write documentation", IsCompleted = false },
            new TodoItem { Title = "Review pull requests", IsCompleted = false },
            new TodoItem { Title = "Fix bug #123", IsCompleted = false },
            new TodoItem { Title = "Update dependencies", IsCompleted = false }
        });

        // Complete some tasks
        Console.WriteLine("   Completing tasks...");
        todos[0].IsCompleted = true;
        todos[2].IsCompleted = true;

        // Add more tasks in batch
        Console.WriteLine("   Adding more tasks...");
        using (todos.SuspendNotifications())
        {
            todos.Add(new TodoItem { Title = "Write tests", IsCompleted = false });
            todos.Add(new TodoItem { Title = "Deploy to staging", IsCompleted = false });
        }

        // Show final state
        Console.WriteLine("\n   Final todo list:");
        foreach (var todo in todos)
        {
            Console.WriteLine($"   - [{(todo.IsCompleted ? "x" : " ")}] {todo.Title}");
        }
    }

    private class TodoItem : INotifyPropertyChanged
    {
        private bool _isCompleted;

        public string Title { get; set; } = "";

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsCompleted)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}