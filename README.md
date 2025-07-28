# Andy.TUI

A .NET reactive text user interface library for the Andy code assistant.

## Overview

Andy.TUI is a modern terminal user interface library that brings reactive programming patterns to console applications. It provides a declarative, component-based approach to building dynamic terminal interfaces, inspired by frameworks like WPF and SwiftUI.

## Current Status (Phase 1 - Core Foundation)

### Completed Features

- **Observable System** (100% implemented, 89.7% test coverage)
  - `ObservableProperty<T>` - Thread-safe properties with change notifications
  - `ComputedProperty<T>` - Automatically recalculates when dependencies change
  - `ObservableCollection<T>` - Observable collections with batch operations
  - `DependencyTracker` - Automatic dependency tracking for computed properties
  - Comprehensive unit tests (67 tests, all passing)
  - Full API documentation and examples

### In Progress

- Virtual DOM system for efficient terminal rendering
- Diff engine for minimal terminal updates
- Terminal abstraction layer

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running Examples

```bash
# View available examples
dotnet run --project examples/Andy.TUI.Examples

# Run specific example
dotnet run --project examples/Andy.TUI.Examples observable
dotnet run --project examples/Andy.TUI.Examples collection

# Run all examples
dotnet run --project examples/Andy.TUI.Examples all
```

## Example Usage

### Observable Properties

```csharp
// Create observable properties
var firstName = new ObservableProperty<string>("John");
var lastName = new ObservableProperty<string>("Doe");

// Create a computed property that depends on other observables
var fullName = new ComputedProperty<string>(() => 
    $"{firstName.Value} {lastName.Value}");

// Subscribe to changes
fullName.Subscribe(name => Console.WriteLine($"Name: {name}"));

// Changes automatically propagate
firstName.Value = "Jane"; // Output: "Name: Jane Doe"
```

### Observable Collections

```csharp
// Create an observable collection
var tasks = new ObservableCollection<string>();

// Create computed properties based on the collection
var taskCount = new ComputedProperty<int>(() => tasks.Count);
var summary = new ComputedProperty<string>(() => 
    $"You have {tasks.Count} tasks");

// Batch operations
tasks.AddRange(new[] { "Task 1", "Task 2", "Task 3" });

// Suspend notifications for multiple changes
using (tasks.SuspendNotifications())
{
    tasks.Add("Task 4");
    tasks.RemoveAll(t => t.Contains("2"));
    tasks[0] = "Updated Task";
} // Single notification fired here
```

## Architecture

The library is organized into several key components:

- **Andy.TUI.Core** - Core reactive system and virtual DOM
  - Observable system with automatic dependency tracking
  - Thread-safe property implementations
  - Memory-efficient weak reference support
- **Andy.TUI.Terminal** - Terminal abstraction layer (planned)
- **Andy.TUI.Components** - Built-in UI components (planned)
- **Andy.TUI.Framework** - Application framework (planned)

### Documentation

- [Architecture Overview](docs/ARCHITECTURE.md) - Detailed architecture and design decisions
- [Observable API Reference](docs/OBSERVABLE_API.md) - Complete API documentation for the Observable system
- [Implementation Plan](docs/IMPLEMENTATION_PLAN.md) - Roadmap and development phases

## Contributing

This project is in early development. For the implementation roadmap, see [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md).

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
