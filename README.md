# Andy.TUI

A .NET reactive text user interface library for the Andy code assistant.

## Overview

Andy.TUI is a modern terminal user interface library that brings reactive programming patterns to console applications. It provides a declarative, component-based approach to building dynamic terminal interfaces, inspired by frameworks like WPF and SwiftUI.

## Current Status

### Phase 1 - Core Foundation (Completed)

#### Completed Features

- **Observable System** (100% implemented, 89.7% test coverage)
  - `ObservableProperty<T>` - Thread-safe properties with change notifications
  - `ComputedProperty<T>` - Automatically recalculates when dependencies change
  - `ObservableCollection<T>` - Observable collections with batch operations
  - `DependencyTracker` - Automatic dependency tracking for computed properties
  - Comprehensive unit tests (67 tests, all passing)
  - Full API documentation and examples

- **Virtual DOM System** (100% implemented)
  - Virtual node types (Text, Element, Fragment, Component)
  - Efficient diff algorithm with keyed reconciliation
  - Fluent builder API for declarative UI construction
  - Comprehensive patch generation for minimal updates
  - Full test coverage (51 tests, all passing)
  - Examples demonstrating basic, advanced, and reactive scenarios

### Phase 2 - Terminal Abstraction (Completed)

- **Terminal Abstraction Layer** (100% implemented)
  - Cross-platform terminal interface with ANSI escape sequence support
  - Double-buffered rendering for smooth animations
  - Comprehensive color support (16-color, 256-color, and 24-bit RGB)
  - Text styling (bold, italic, underline, strikethrough, dim, inverse, blink)
  - Input handling with keyboard event support
  - Efficient cell-based buffer management
  - Platform-specific handling for Windows and Unix-like systems
  - Full test coverage with unit tests
  - Interactive examples demonstrating all features

- **Rendering System** (100% implemented)
  - High-level rendering API with automatic double buffering
  - Frame rate control and render scheduling
  - ANSI escape sequence generation
  - Cell-based dirty region tracking for optimal performance
  - Comprehensive animation support with smooth transitions
  - Interactive examples including games, animations, and visual effects

### Phase 3.1 - Component Base Infrastructure (Completed)

- **Component System** (100% implemented)
  - `IComponent` interface with full lifecycle management
  - `ComponentBase` abstract class with state management
  - Observable property binding with automatic re-rendering
  - Component Context system for parent/child relationships
  - Service injection via `IServiceProvider`
  - Event handling framework with propagation and bubbling
  - `SharedStateManager` for cross-component state sharing
  - `ThemeProvider` for theming and styling support
  - Comprehensive test coverage (252 tests passing)
  - Working examples demonstrating all component features

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

# Run all observable examples
dotnet run --project examples/Andy.TUI.Examples all

# Run Virtual DOM examples
dotnet run --project examples/VirtualDom basic
dotnet run --project examples/VirtualDom advanced
dotnet run --project examples/VirtualDom reactive

# Run Terminal examples
dotnet run --project examples/Andy.TUI.Examples.Terminal terminal-basic
dotnet run --project examples/Andy.TUI.Examples.Terminal terminal-style
dotnet run --project examples/Andy.TUI.Examples.Terminal terminal-buffer
dotnet run --project examples/Andy.TUI.Examples.Terminal terminal-input

# Run Component examples
dotnet run --project examples/Andy.TUI.Examples.Components
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

### Virtual DOM

```csharp
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

// Build UI declaratively
var ui = VBox()
    .WithClass("app")
    .WithChildren(
        Label().WithText($"Count: {counter.Value}"),
        HBox()
            .WithChildren(
                Button()
                    .WithText("Increment")
                    .OnClick(() => counter.Value++),
                Button()
                    .WithText("Reset")
                    .OnClick(() => counter.Value = 0)
            )
    )
    .Build();

// Efficient updates with diffing
var diffEngine = new DiffEngine();
var patches = diffEngine.Diff(oldTree, newTree);
// Apply only the minimal changes needed
```

### Terminal Abstraction

```csharp
using Andy.TUI.Terminal;

// Create a terminal with double buffering
var terminal = new AnsiTerminal();
using var renderingSystem = new RenderingSystem(terminal);
renderingSystem.Initialize();

// Apply styles and colors
var style = Style.Default
    .WithForegroundColor(Color.Blue)
    .WithBackgroundColor(Color.White)
    .WithBold();

renderingSystem.WriteText(10, 5, "Hello, TUI!", style);
renderingSystem.DrawBox(0, 0, 80, 24, style, BoxStyle.Double);
renderingSystem.Render();

// Handle input
var inputHandler = new ConsoleInputHandler();
inputHandler.KeyPressed += (_, e) => 
{
    if (e.Key == ConsoleKey.Escape) 
        Environment.Exit(0);
};
```

## Architecture

The library is organized into several key components:

- **Andy.TUI.Core** - Core reactive system, virtual DOM, and component infrastructure
  - Observable system with automatic dependency tracking
  - Thread-safe property implementations
  - Memory-efficient weak reference support
  - Component base classes and lifecycle management
  - Event handling framework with propagation
  - Shared state management and theming support
- **Andy.TUI.Terminal** - Terminal abstraction layer
  - Cross-platform ANSI terminal support
  - Double-buffered rendering with frame rate control
  - Rich color and styling capabilities
  - Efficient cell-based rendering with dirty region tracking
  - High-level rendering system with animation support
- **Andy.TUI.Components** - Built-in UI components (planned)
- **Andy.TUI.Framework** - Application framework (planned)

### Documentation

- [Architecture Overview](docs/ARCHITECTURE.md) - Detailed architecture and design decisions
- [Observable API Reference](docs/OBSERVABLE_API.md) - Complete API documentation for the Observable system
- [Virtual DOM API Reference](docs/VIRTUAL_DOM_API.md) - Complete API documentation for the Virtual DOM system
- [Terminal API Reference](docs/TERMINAL_API.md) - Complete API documentation for the Terminal abstraction layer
- [Implementation Plan](docs/IMPLEMENTATION_PLAN.md) - Roadmap and development phases

## Contributing

This project is in early development. For the implementation roadmap, see [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md).

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
