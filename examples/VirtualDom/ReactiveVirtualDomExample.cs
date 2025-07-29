using Andy.TUI.Core.Observable;
using Andy.TUI.Core.VirtualDom;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Examples.VirtualDom;

/// <summary>
/// Demonstrates integration of Virtual DOM with the Observable system
/// for reactive UI updates.
/// </summary>
public static class ReactiveVirtualDomExample
{
    public static void Run()
    {
        Console.WriteLine("=== Reactive Virtual DOM Example ===\n");
        
        // Example 1: Basic reactive updates
        Console.WriteLine("1. Basic Reactive Updates:");
        DemonstrateBasicReactivity();
        
        // Example 2: Computed properties with Virtual DOM
        Console.WriteLine("\n2. Computed Properties with Virtual DOM:");
        DemonstrateComputedProperties();
        
        // Example 3: Collection changes
        Console.WriteLine("\n3. Reactive Collections:");
        DemonstrateReactiveCollections();
        
        // Example 4: Complex reactive scenario
        Console.WriteLine("\n4. Complex Reactive Scenario:");
        DemonstrateComplexReactivity();
    }
    
    private static void DemonstrateBasicReactivity()
    {
        // Create reactive state
        var counter = new ObservableProperty<int>(0);
        var isVisible = new ObservableProperty<bool>(true);
        
        VirtualNode? previousTree = null;
        var diffEngine = new DiffEngine();
        
        // Function to build UI based on current state
        VirtualNode BuildUI()
        {
            var builder = VBox()
                .WithClass("counter-app");
            
            if (isVisible.Value)
            {
                builder.WithChildren(
                    Label().WithText($"Counter: {counter.Value}"),
                    HBox()
                        .WithClass("buttons")
                        .WithChildren(
                            Button()
                                .WithText("Increment")
                                .OnClick(() => counter.Value++),
                            Button()
                                .WithText("Decrement")
                                .OnClick(() => counter.Value--),
                            Button()
                                .WithText("Hide")
                                .OnClick(() => isVisible.Value = false)
                        )
                );
            }
            else
            {
                builder.WithChildren(
                    Button()
                        .WithText("Show Counter")
                        .OnClick(() => isVisible.Value = true)
                );
            }
            
            return builder.Build();
        }
        
        // Track changes
        counter.Subscribe(value =>
        {
            Console.WriteLine($"Counter changed to: {value}");
            var newTree = BuildUI();
            if (previousTree != null)
            {
                var patches = diffEngine.Diff(previousTree, newTree);
                Console.WriteLine($"  Generated {patches.Count} patches");
            }
            previousTree = newTree;
        });
        
        isVisible.Subscribe(visible =>
        {
            Console.WriteLine($"Visibility changed to: {visible}");
            var newTree = BuildUI();
            if (previousTree != null)
            {
                var patches = diffEngine.Diff(previousTree, newTree);
                Console.WriteLine($"  Generated {patches.Count} patches");
            }
            previousTree = newTree;
        });
        
        // Initial render
        previousTree = BuildUI();
        
        // Simulate interactions
        counter.Value = 5;
        counter.Value = 10;
        isVisible.Value = false;
        isVisible.Value = true;
    }
    
    private static void DemonstrateComputedProperties()
    {
        // Create reactive state
        var firstName = new ObservableProperty<string>("John");
        var lastName = new ObservableProperty<string>("Doe");
        var age = new ObservableProperty<int>(25);
        
        // Create computed properties
        var fullName = new ComputedProperty<string>(() => $"{firstName.Value} {lastName.Value}");
        var canVote = new ComputedProperty<bool>(() => age.Value >= 18);
        var profile = new ComputedProperty<string>(() => 
            $"{fullName.Value}, Age: {age.Value}, Can Vote: {canVote.Value}");
        
        // Build UI function
        VirtualNode BuildProfileUI()
        {
            return Box()
                .WithClass("profile")
                .WithChildren(
                    Element("h2").WithText(fullName.Value),
                    Label().WithText($"Age: {age.Value}"),
                    canVote.Value
                        ? Label()
                            .WithText("✓ Eligible to vote")
                            .WithForeground("green")
                        : Label()
                            .WithText("✗ Not eligible to vote")
                            .WithForeground("red"),
                    Element("hr"),
                    Label().WithText($"Profile: {profile.Value}")
                )
                .Build();
        }
        
        var diffEngine = new DiffEngine();
        VirtualNode? previousTree = null;
        
        // Subscribe to all changes
        profile.Subscribe(_ =>
        {
            var newTree = BuildProfileUI();
            if (previousTree != null)
            {
                var patches = diffEngine.Diff(previousTree, newTree);
                Console.WriteLine($"Profile update generated {patches.Count} patches");
            }
            previousTree = newTree;
        });
        
        // Initial render
        previousTree = BuildProfileUI();
        Console.WriteLine($"Initial profile: {profile.Value}");
        
        // Make changes
        firstName.Value = "Jane";
        lastName.Value = "Smith";
        age.Value = 17;
        age.Value = 18;
    }
    
    private static void DemonstrateReactiveCollections()
    {
        var todos = new ObservableCollection<TodoItem>();
        var filter = new ObservableProperty<string>("all"); // all, active, completed
        
        // Computed property for filtered todos
        var filteredTodos = new ComputedProperty<List<TodoItem>>(() =>
        {
            var items = todos.AsTracked().ToList();
            return filter.Value switch
            {
                "active" => items.Where(t => !t.IsCompleted.Value).ToList(),
                "completed" => items.Where(t => t.IsCompleted.Value).ToList(),
                _ => items
            };
        });
        
        // Build UI function
        VirtualNode BuildTodoUI()
        {
            var items = filteredTodos.Value;
            
            return VBox()
                .WithClass("todo-app")
                .WithChildren(
                    // Header
                    Element("h1").WithText("Todo List"),
                    
                    // Filter buttons
                    HBox()
                        .WithClass("filters")
                        .WithChildren(
                            Button()
                                .WithText("All")
                                .WithClass(filter.Value == "all" ? "active" : "")
                                .OnClick(() => filter.Value = "all"),
                            Button()
                                .WithText("Active")
                                .WithClass(filter.Value == "active" ? "active" : "")
                                .OnClick(() => filter.Value = "active"),
                            Button()
                                .WithText("Completed")
                                .WithClass(filter.Value == "completed" ? "active" : "")
                                .OnClick(() => filter.Value = "completed")
                        ),
                    
                    // Todo list
                    List()
                        .WithChildren(
                            items.Select(todo =>
                                ListItem()
                                    .WithKey(todo.Id)
                                    .WithClass(todo.IsCompleted.Value ? "completed" : "")
                                    .WithChildren(
                                        Input()
                                            .WithProp("type", "checkbox")
                                            .WithProp("checked", todo.IsCompleted.Value),
                                        Span().WithText(todo.Text.Value)
                                    ).Build()
                            ).ToArray()
                        )
                )
                .Build();
        }
        
        var diffEngine = new DiffEngine();
        VirtualNode? previousTree = null;
        
        // Update UI on any change
        Action updateUI = () =>
        {
            var newTree = BuildTodoUI();
            if (previousTree != null)
            {
                var patches = diffEngine.Diff(previousTree, newTree);
                Console.WriteLine($"Todo list update: {patches.Count} patches");
                
                // Analyze patch types
                var patchTypes = patches.GroupBy(p => p.Type);
                foreach (var group in patchTypes)
                {
                    Console.WriteLine($"  - {group.Count()} {group.Key} patches");
                }
                
                // Debug: Check if the trees are actually different
                if (patches.Count == 0 && !previousTree.Equals(newTree))
                {
                    Console.WriteLine("  WARNING: Trees are different but no patches generated!");
                }
            }
            previousTree = newTree;
        };
        
        // Subscribe to changes - use the computed property instead
        filteredTodos.Subscribe(_ => updateUI());
        filter.Subscribe(_ => updateUI());
        
        // Initial render
        previousTree = BuildTodoUI();
        
        // Add some todos
        var todo1 = new TodoItem("Learn Virtual DOM");
        var todo2 = new TodoItem("Build reactive UI");
        var todo3 = new TodoItem("Ship application");
        
        todos.Add(todo1);
        todos.Add(todo2);
        todos.Add(todo3);
        
        // Subscribe to individual todo changes
        todo1.IsCompleted.Subscribe(_ => updateUI());
        todo2.IsCompleted.Subscribe(_ => updateUI());
        todo3.IsCompleted.Subscribe(_ => updateUI());
        
        // Simulate interactions
        Console.WriteLine("\nMarking first todo as completed:");
        todo1.IsCompleted.Value = true;
        
        Console.WriteLine("\nFiltering to show only active:");
        filter.Value = "active";
        
        Console.WriteLine("\nAdding a new todo:");
        Console.WriteLine($"Current filter: {filter.Value}, Todo count before: {todos.Count}");
        Console.WriteLine($"Filtered todos before: {filteredTodos.Value.Count}");
        var todo4 = new TodoItem("Review code");
        todo4.IsCompleted.Subscribe(_ => updateUI());
        todos.Add(todo4);
        // Force re-evaluation of filtered todos
        var filteredAfter = filteredTodos.Value;
        Console.WriteLine($"Todo count after: {todos.Count}, Filtered count: {filteredAfter.Count}");
        
        Console.WriteLine("\nShowing all todos:");
        filter.Value = "all";
    }
    
    private static void DemonstrateComplexReactivity()
    {
        // Create a reactive data model
        var appState = new AppState();
        
        // Build UI based on state
        VirtualNode BuildAppUI()
        {
            return VBox()
                .WithClass("app")
                .WithChildren(
                    // Header with user info
                    HBox()
                        .WithClass("header")
                        .WithChildren(
                            Label().WithText($"Welcome, {appState.CurrentUser.Value?.Name ?? "Guest"}"),
                            Span().WithFlex(1), // Spacer
                            Label().WithText($"Notifications: {appState.NotificationCount.Value}")
                        ),
                    
                    // Main content area
                    Box()
                        .WithClass("content")
                        .WithFlex(1)
                        .WithChildren(
                            appState.IsLoading.Value
                                ? Spinner()
                                : BuildContentView()
                        ),
                    
                    // Status bar
                    HBox()
                        .WithClass("status-bar")
                        .WithChildren(
                            Label().WithText($"Items: {appState.Items.Count}"),
                            Label().WithText($"Selected: {appState.SelectedCount.Value}"),
                            Label().WithText($"Status: {appState.Status.Value}")
                        )
                )
                .Build();
        }
        
        VirtualNode BuildContentView()
        {
            return Box()
                .WithChildren(
                    // Item list
                    List()
                        .WithClass("item-list")
                        .WithChildren(
                            appState.FilteredItems.Value.Select(item =>
                                ListItem()
                                    .WithKey(item.Id.ToString())
                                    .WithClass(item.IsSelected.Value ? "selected" : "")
                                    .OnClick(() => item.IsSelected.Value = !item.IsSelected.Value)
                                    .WithChildren(
                                        Span().WithText(item.Name.Value),
                                        Span().WithText($"Priority: {item.Priority.Value}")
                                    ).Build()
                            ).ToArray()
                        ),
                    
                    // Action buttons
                    HBox()
                        .WithClass("actions")
                        .WithChildren(
                            Button()
                                .WithText("Select All")
                                .OnClick(() => appState.SelectAll()),
                            Button()
                                .WithText("Clear Selection")
                                .OnClick(() => appState.ClearSelection()),
                            Button()
                                .WithText("Refresh")
                                .OnClick(() => appState.RefreshItems())
                        )
                );
        }
        
        VirtualNode Spinner()
        {
            return Box()
                .WithClass("spinner")
                .WithAlign("center")
                .WithJustify("center")
                .WithText("Loading...");
        }
        
        var diffEngine = new DiffEngine();
        VirtualNode? previousTree = null;
        
        // Subscribe to individual state changes for better tracking
        Action updateUI = () =>
        {
            var newTree = BuildAppUI();
            if (previousTree != null)
            {
                var patches = diffEngine.Diff(previousTree, newTree);
                Console.WriteLine($"State change triggered {patches.Count} patches");
            }
            previousTree = newTree;
        };
        
        appState.CurrentUser.Subscribe(_ => updateUI());
        appState.IsLoading.Subscribe(_ => updateUI());
        appState.NotificationCount.Subscribe(_ => updateUI());
        appState.Status.Subscribe(_ => updateUI());
        appState.FilterPriority.Subscribe(_ => updateUI());
        appState.Items.CollectionChanged += (_, _) => updateUI();
        
        // Initial render
        previousTree = BuildAppUI();
        
        // Simulate application flow
        Console.WriteLine("\nSetting current user:");
        appState.CurrentUser.Value = new User { Name = "Alice" };
        
        Console.WriteLine("\nLoading items:");
        appState.RefreshItems();
        
        // Wait for items to load
        Thread.Sleep(150);
        
        Console.WriteLine("\nSelecting some items:");
        if (appState.Items.Count > 0)
        {
            appState.Items[0].IsSelected.Value = true;
            if (appState.Items.Count > 1)
            {
                appState.Items[1].IsSelected.Value = true;
            }
        }
        
        Console.WriteLine("\nChanging filter:");
        appState.FilterPriority.Value = 2;
        
        Console.WriteLine("\nAdding notification:");
        appState.NotificationCount.Value++;
    }
}

// Helper classes for the examples
public class TodoItem
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public ObservableProperty<string> Text { get; }
    public ObservableProperty<bool> IsCompleted { get; }
    
    public TodoItem(string text)
    {
        Text = new ObservableProperty<string>(text);
        IsCompleted = new ObservableProperty<bool>(false);
    }
}

public class User
{
    public string Name { get; set; } = "";
}

public class AppItem
{
    public int Id { get; set; }
    public ObservableProperty<string> Name { get; }
    public ObservableProperty<int> Priority { get; }
    public ObservableProperty<bool> IsSelected { get; }
    
    public AppItem(int id, string name, int priority)
    {
        Id = id;
        Name = new ObservableProperty<string>(name);
        Priority = new ObservableProperty<int>(priority);
        IsSelected = new ObservableProperty<bool>(false);
    }
}

public class AppState
{
    public ObservableProperty<User?> CurrentUser { get; } = new(null);
    public ObservableProperty<bool> IsLoading { get; } = new(false);
    public ObservableProperty<int> NotificationCount { get; } = new(0);
    public ObservableProperty<string> Status { get; } = new("Ready");
    public ObservableProperty<int> FilterPriority { get; } = new(0);
    public ObservableCollection<AppItem> Items { get; } = new();
    
    public ComputedProperty<int> SelectedCount { get; }
    public ComputedProperty<List<AppItem>> FilteredItems { get; }
    public ComputedProperty<object> StateChanged { get; }
    
    public AppState()
    {
        SelectedCount = new ComputedProperty<int>(() =>
            Items.AsTracked().Count(item => item.IsSelected.Value));
        
        FilteredItems = new ComputedProperty<List<AppItem>>(() =>
        {
            var priority = FilterPriority.Value;
            return priority == 0
                ? Items.AsTracked().ToList()
                : Items.AsTracked().Where(item => item.Priority.Value >= priority).ToList();
        });
        
        // Aggregate state change signal
        StateChanged = new ComputedProperty<object>(() => new
        {
            User = CurrentUser.Value,
            Loading = IsLoading.Value,
            Notifications = NotificationCount.Value,
            Status = Status.Value,
            Selected = SelectedCount.Value,
            Filtered = FilteredItems.Value.Count
        });
    }
    
    public void SelectAll()
    {
        foreach (var item in Items)
        {
            item.IsSelected.Value = true;
        }
    }
    
    public void ClearSelection()
    {
        foreach (var item in Items)
        {
            item.IsSelected.Value = false;
        }
    }
    
    public void RefreshItems()
    {
        IsLoading.Value = true;
        Status.Value = "Loading...";
        
        // Simulate async loading
        Task.Delay(100).ContinueWith(_ =>
        {
            Items.Clear();
            Items.AddRange(new[]
            {
                new AppItem(1, "Task 1", 1),
                new AppItem(2, "Task 2", 2),
                new AppItem(3, "Task 3", 3),
                new AppItem(4, "Task 4", 1),
                new AppItem(5, "Task 5", 2)
            });
            
            // Subscribe to selection changes
            foreach (var item in Items)
            {
                item.IsSelected.Subscribe(_ => { }); // Trigger state change
            }
            
            IsLoading.Value = false;
            Status.Value = "Ready";
        });
    }
}