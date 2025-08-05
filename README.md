# Andy.TUI

A powerful declarative terminal UI framework for .NET that combines the best of Ink, Yoga, SwiftUI, and WPF.

## Overview

Andy.TUI is a modern terminal user interface library that brings declarative UI patterns to console applications. It combines:
- **Ink's rich component library** and developer experience
- **Yoga's flexbox layout engine** for precise positioning
- **SwiftUI's type safety** and declarative syntax
- **WPF's MVVM patterns** and data binding

See [DECLARATIVE_ARCHITECTURE.md](docs/DECLARATIVE_ARCHITECTURE.md) for the complete architecture overview.

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

### Phase 3 - Declarative UI Framework (Completed)

- **Declarative UI Framework** (100% implemented)
  - SwiftUI-style declarative syntax with collection initializers
  - Flexbox layout engine with Yoga-style properties
  - Type-safe `Length` type supporting pixels, percentages, and auto
  - `Spacing` type for margin/padding with individual side control
  - Layout components: `Box`, `VStack`, `HStack`, `ZStack`, `Grid`, `Spacer`
  - Input components: `TextField`, `Button`, `Dropdown<T>`, `Text`
  - State management with `Binding<T>` for two-way data binding
  - Focus management and keyboard navigation
  - Overflow handling with clipping support
  - CSS Grid-like behavior with fr units and spanning
  - Full test coverage with comprehensive examples
  - Full test coverage (112 tests, all passing)
  - Comprehensive examples demonstrating all input components

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later

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

# Run Declarative examples
dotnet run --project examples/Andy.TUI.Examples.Input

# Run Layout examples
dotnet run --project examples/Andy.TUI.Examples.Layout
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

### Declarative Components

```csharp
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;

// Create a grid layout
var grid = new Grid();
grid.SetColumns(GridLength.Absolute(200), GridLength.Star(1), GridLength.Absolute(150));
grid.SetRows(GridLength.Auto, GridLength.Star(1));

// Add header spanning all columns
var header = new Box
{
    Content = new TextNode("Application Header"),
    BackgroundColor = Color.DarkBlue,
    ForegroundColor = Color.White,
    Padding = new Spacing(1, 3),
    ContentHorizontalAlignment = Alignment.Center
};
grid.AddChild(header, row: 0, column: 0, columnSpan: 3);

// Add main content with scrolling
var scrollView = new ScrollView
{
    Content = new TextNode(longContent),
    ShowVerticalScrollbar = true
};
grid.AddChild(scrollView, row: 1, column: 1);

// Add sidebar with menu items
var sidebar = new Stack
{
    Orientation = Orientation.Vertical,
    Spacing = 2,
    Padding = new Spacing(2)
};
sidebar.AddChild(new TextNode("Menu Item 1"));
sidebar.AddChild(new TextNode("Menu Item 2"));
sidebar.AddChild(new TextNode("Menu Item 3"));
grid.AddChild(sidebar, row: 1, column: 2);
```

### Declarative Input

```csharp
using Andy.TUI.Declarative.Components;

// TextInput for single-line text entry
var nameInput = new TextInput
{
    Placeholder = "Enter your name...",
    MaxLength = 50,
    ValidationFunc = value => !string.IsNullOrWhiteSpace(value)
};

// TextArea for multi-line text
var commentArea = new TextArea
{
    MinHeight = 5,
    MaxHeight = 10,
    WordWrap = true,
    ShowLineNumbers = true
};

// Button with different styles
var saveButton = new Button
{
    Text = "Save Changes",
    Style = ButtonStyle.Primary,
    MinWidth = 15
};
saveButton.Click += (s, e) => SaveData();

// Select dropdown with custom items
var countrySelect = new Select<string>
{
    Placeholder = "Select a country...",
    AllowFiltering = true,
    MaxDisplayItems = 10
};
countrySelect.Items = new[]
{
    new SelectItem<string>("United States"),
    new SelectItem<string>("Canada"),
    new SelectItem<string>("United Kingdom")
};

// Form example combining all input types
var form = new Stack { Orientation = Orientation.Vertical, Spacing = 2 };
form.AddChild(new TextNode("Name:"));
form.AddChild(nameInput);
form.AddChild(new TextNode("Comments:"));
form.AddChild(commentArea);
form.AddChild(new TextNode("Country:"));
form.AddChild(countrySelect);
form.AddChild(saveButton);
```

## Debugging

Andy.TUI includes a comprehensive debug logging system that helps diagnose issues with rendering, event routing, and state management.

### Enabling Debug Logging

Set the `ANDY_TUI_DEBUG` environment variable to enable debug logging:

```bash
# Enable debug logging with default level (Debug)
export ANDY_TUI_DEBUG=1

# Set specific log level (Debug, Info, Warning, Error)
export ANDY_TUI_DEBUG=Info

# Run your application
dotnet run --project examples/Andy.TUI.Examples.Input
```

### Debug Output Location

By default, debug logs are written to timestamped directories in your system's temp folder:
- **macOS/Linux**: `/tmp/andy-tui-debug/YYYYMMDD_HHMMSS/`
- **Windows**: `%TEMP%\andy-tui-debug\YYYYMMDD_HHMMSS\`

You can customize the log directory:
```bash
export ANDY_TUI_DEBUG_DIR=/path/to/custom/logs
```

### Log Categories

The debug system organizes logs by category:
- **DeclarativeRenderer**: Rendering cycles, virtual DOM updates, and patch application
- **FocusManager**: Focus changes and keyboard navigation
- **EventRouter**: Event routing and handling
- **ViewInstanceManager**: Component lifecycle and instance management
- **SelectInputInstance**: SelectInput-specific behavior and state changes

### Reading Debug Logs

Each category gets its own log file:
```
/tmp/andy-tui-debug/20250804_175311/
├── DeclarativeRenderer.log
├── EventRouter.log
├── FocusManager.log
└── ViewInstanceManager.log
```

Example log output:
```
[2025-08-04 17:53:11.123] [DEBUG] [DeclarativeRenderer] Render requested, executing render cycle
[2025-08-04 17:53:11.124] [DEBUG] [DeclarativeRenderer] Virtual DOM rendered
[2025-08-04 17:53:11.125] [DEBUG] [DeclarativeRenderer] Performing full render
[2025-08-04 17:53:11.126] [DEBUG] [DeclarativeRenderer] Buffer dirty state before flush: True
[2025-08-04 17:53:11.127] [DEBUG] [DeclarativeRenderer] Forced render flush
```

### Common Debugging Scenarios

1. **UI Not Updating**: Check `DeclarativeRenderer.log` for render cycles and patch application
2. **Keyboard Input Issues**: Check `EventRouter.log` and `FocusManager.log`
3. **Component State Problems**: Check the specific component's log file
4. **Performance Issues**: Look for excessive render cycles or large patch counts

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
- **Andy.TUI.Declarative** - SwiftUI-style declarative UI framework
  - Layout components (Box, VStack, HStack, ZStack, Grid, Spacer)
  - Input components (TextField, Button, Dropdown, Text)
  - State management with Binding<T> for two-way data binding
  - Flexbox layout engine with Yoga-style properties
  - Display components (planned)
- **Andy.TUI.Framework** - Application framework (planned)

### Documentation

- [Architecture Overview](docs/ARCHITECTURE.md) - Detailed architecture and design decisions
- [Observable API Reference](docs/OBSERVABLE_API.md) - Complete API documentation for the Observable system
- [Virtual DOM API Reference](docs/VIRTUAL_DOM_API.md) - Complete API documentation for the Virtual DOM system
- [Terminal API Reference](docs/TERMINAL_API.md) - Complete API documentation for the Terminal abstraction layer
- [Debug Logging Guide](docs/DEBUG_LOGGING.md) - How to use the debug logging system
- [Implementation Plan](docs/IMPLEMENTATION_PLAN.md) - Roadmap and development phases

## Contributing

This project is in early development. For the implementation roadmap, see [docs/IMPLEMENTATION_PLAN.md](docs/IMPLEMENTATION_PLAN.md).

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.
