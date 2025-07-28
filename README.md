# Andy.TUI

A .NET reactive text user interface library for the Andy code assistant.

## Overview

Andy.TUI is a modern terminal user interface library that brings reactive programming patterns to console applications. It provides a declarative, component-based approach to building dynamic terminal interfaces, inspired by frameworks like WPF and SwiftUI.

## Current Status (Phase 1 - Core Foundation)

### ✅ Completed Features

- **Observable System**
  - `ObservableProperty<T>` - Thread-safe properties with change notifications
  - `ComputedProperty<T>` - Automatically recalculates when dependencies change
  - `DependencyTracker` - Automatic dependency tracking for computed properties
  - Comprehensive unit tests (36 tests, all passing)
  - Example application demonstrating the reactive system

### 🚧 In Progress

- `ObservableCollection<T>` - Observable collections with change notifications
- Virtual DOM system for efficient terminal rendering
- Diff engine for minimal terminal updates

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
dotnet run --project examples/Andy.TUI.Examples observable
```

## Example Usage

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

## Architecture

The library is organized into several key components:

- **Andy.TUI.Core** - Core reactive system and virtual DOM
- **Andy.TUI.Terminal** - Terminal abstraction layer (planned)
- **Andy.TUI.Components** - Built-in UI components (planned)
- **Andy.TUI.Framework** - Application framework (planned)

For detailed architecture documentation, see [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Contributing

This project is in early development. For the implementation roadmap, see [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md).

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
