# Observable System API Reference

## Overview

The Andy.TUI Observable system provides a reactive programming model for managing state in terminal applications. It consists of observable properties, computed properties, observable collections, and a dependency tracking system.

## Core Interfaces

### IObservableProperty

Base interface for all observable properties.

```csharp
public interface IObservableProperty
{
    object? Value { get; }
    bool HasObservers { get; }
    event EventHandler<PropertyChangedEventArgs>? PropertyChanged;
    void AddObserver(IPropertyObserver observer);
    void RemoveObserver(IPropertyObserver observer);
    void Dispose();
}
```

### IObservableProperty<T>

Generic interface for strongly-typed observable properties.

```csharp
public interface IObservableProperty<T> : IObservableProperty
{
    new T Value { get; set; }
    event EventHandler<PropertyChangedEventArgs<T>>? ValueChanged;
    IDisposable Subscribe(Action<T> callback);
    IDisposable Observe(Action<T> callback);
}
```

### IComputedProperty<T>

Interface for computed properties that derive their value from other observables.

```csharp
public interface IComputedProperty<T> : IObservableProperty<T>
{
    IReadOnlySet<IObservableProperty> Dependencies { get; }
    Func<T> Computation { get; }
    bool IsValid { get; }
    void Invalidate();
}
```

## Classes

### ObservableProperty<T>

Thread-safe implementation of an observable property.

#### Constructor

```csharp
public ObservableProperty(T initialValue = default!, 
                         string propertyName = "", 
                         IEqualityComparer<T>? comparer = null)
```

#### Key Methods

- `SetValueSilently(T value)` - Updates value without triggering notifications
- `ForceNotify()` - Forces a change notification even if value hasn't changed
- `static Create(T initialValue, string propertyName)` - Factory method

#### Usage Example

```csharp
var counter = new ObservableProperty<int>(0);

// Subscribe to changes
counter.ValueChanged += (s, e) => 
    Console.WriteLine($"Counter: {e.OldValue} -> {e.NewValue}");

// Update value
counter.Value = 42; // Triggers notification

// Subscribe with callback
using (counter.Subscribe(value => Console.WriteLine($"Current: {value}")))
{
    counter.Value = 100; // Callback invoked
}

// Observe (immediate + future)
using (counter.Observe(value => Console.WriteLine($"Observed: {value}")))
{
    // Callback invoked immediately with current value (100)
    counter.Value = 200; // Callback invoked again
}
```

### ComputedProperty<T>

Automatically updating property that depends on other observables.

#### Constructor

```csharp
public ComputedProperty(Func<T> computation, 
                       string propertyName = "", 
                       IEqualityComparer<T>? comparer = null)
```

#### Key Features

- Lazy evaluation - only computes when accessed
- Automatic dependency tracking
- Circular dependency detection
- Efficient recomputation

#### Usage Example

```csharp
var width = new ObservableProperty<int>(10);
var height = new ObservableProperty<int>(20);

var area = new ComputedProperty<int>(() => width.Value * height.Value);

Console.WriteLine(area.Value); // 200

width.Value = 15;
Console.WriteLine(area.Value); // 300 - automatically recomputed
```

### ObservableCollection<T>

Enhanced observable collection with batch operations and notification control.

#### Key Methods

- `AddRange(IEnumerable<T> items)` - Add multiple items with single notification
- `RemoveRange(IEnumerable<T> items)` - Remove multiple items with single notification
- `RemoveAll(Predicate<T> predicate)` - Remove all matching items
- `Replace(int index, T item)` - Replace item at index
- `Move(int oldIndex, int newIndex)` - Move item to new position
- `SuspendNotifications()` - Returns IDisposable to suspend notifications

#### Usage Example

```csharp
var tasks = new ObservableCollection<string>();

// Subscribe to changes
tasks.CollectionChanged += (s, e) => 
    Console.WriteLine($"Collection changed: {e.Action}");

// Add multiple items with one notification
tasks.AddRange(new[] { "Task 1", "Task 2", "Task 3" });

// Batch operations
using (tasks.SuspendNotifications())
{
    tasks.Add("Task 4");
    tasks.Remove("Task 1");
    tasks[0] = "Updated Task";
} // Single reset notification here

// Remove with predicate
tasks.RemoveAll(t => t.Contains("2"));
```

### DependencyTracker

Manages automatic dependency tracking for computed properties.

#### Static Methods

- `BeginTracking()` - Start dependency tracking context
- `Current` - Get current tracking context

#### Usage (Internal)

```csharp
using (var tracker = DependencyTracker.BeginTracking())
{
    // Access observable properties here
    var value = someProperty.Value;
    
    // Dependencies are automatically tracked
    var dependencies = tracker.Dependencies;
}
```

## Extension Methods

### ObservableCollectionExtensions

Provides LINQ integration for observable collections.

#### AsTracked<T>

Ensures collection access is tracked as a dependency.

```csharp
var items = new ObservableCollection<int>(new[] { 1, 2, 3 });

var sum = new ComputedProperty<int>(() => 
{
    // Use AsTracked() to ensure dependency tracking with LINQ
    return items.AsTracked().Sum();
});
```

## Event Arguments

### PropertyChangedEventArgs

Provides data for property change events.

```csharp
public class PropertyChangedEventArgs : EventArgs
{
    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
}
```

### PropertyChangedEventArgs<T>

Strongly-typed property change event arguments.

```csharp
public class PropertyChangedEventArgs<T> : EventArgs
{
    public string PropertyName { get; }
    public T OldValue { get; }
    public T NewValue { get; }
}
```

## Thread Safety

All observable types are thread-safe with the following guarantees:

- Property reads and writes are atomic
- Event notifications are thread-safe
- Collection operations are synchronized
- Computed property evaluation is thread-safe

## Memory Management

### Weak References

Observable properties use weak references for callbacks to prevent memory leaks:

```csharp
// This won't cause a memory leak even if not disposed
var subscription = property.Subscribe(value => ProcessValue(value));
```

### Disposal Pattern

All observable types implement IDisposable:

```csharp
var property = new ObservableProperty<int>(42);

// Use in component
try
{
    // Use property
}
finally
{
    property.Dispose(); // Cleans up all subscriptions
}
```

## Best Practices

### 1. Use Computed Properties for Derived Values

```csharp
// Good: Automatically updates
var fullName = new ComputedProperty<string>(() => 
    $"{firstName.Value} {lastName.Value}");

// Avoid: Manual updates required
var fullName = new ObservableProperty<string>();
firstName.ValueChanged += (s, e) => UpdateFullName();
lastName.ValueChanged += (s, e) => UpdateFullName();
```

### Using with Declarative Components

Use `Binding<T>` in declarative components for two-way data binding rather than subscribing manually; subscriptions are managed by the framework.

### 2. Batch Collection Operations

```csharp
// Good: Single notification
collection.AddRange(items);

// Avoid: Multiple notifications
foreach (var item in items)
    collection.Add(item);
```

### 3. Dispose Subscriptions

```csharp
// Good: Proper cleanup
using (property.Subscribe(HandleChange))
{
    // Use subscription
}

// Or store for later disposal
_subscription = property.Subscribe(HandleChange);
// Later: _subscription.Dispose();
```

### 4. Use AsTracked() with LINQ

```csharp
// Good: Ensures dependency tracking
var sum = new ComputedProperty<int>(() => 
    collection.AsTracked().Sum());

// May not track: Direct LINQ usage
var sum = new ComputedProperty<int>(() => 
    collection.Sum());
```

## Performance Considerations

- **Lazy Evaluation**: Computed properties only evaluate when accessed
- **Equality Checking**: Properties only notify if value actually changes
- **Batch Operations**: Use collection batch methods to reduce notifications
- **Weak References**: Automatic cleanup of dead callbacks
- **Thread Safety**: Minimal locking for high concurrency

## Error Handling

### Circular Dependencies

Computed properties detect circular dependencies:

```csharp
ComputedProperty<int> a = null!;
ComputedProperty<int> b = null!;

a = new ComputedProperty<int>(() => b.Value + 1);
b = new ComputedProperty<int>(() => a.Value + 1);

// Throws InvalidOperationException on access
var value = a.Value; // Circular dependency detected
```

### Disposal

Accessing disposed properties throws ObjectDisposedException:

```csharp
var property = new ObservableProperty<int>(42);
property.Dispose();

// Throws ObjectDisposedException
property.Value = 100;
```