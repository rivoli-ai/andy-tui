# Andy.TUI.Declarative Project Documentation

## Overview

Andy.TUI.Declarative provides a SwiftUI-inspired declarative API for building terminal user interfaces. This module enables developers to describe UI hierarchies using a fluent, composable syntax with automatic state management, layout calculation, and reactive updates, bringing modern declarative UI patterns to terminal applications.

## Project Configuration

### Target Framework
- **.NET 8.0**
- **Nullable Reference Types**: Enabled
- **Implicit Usings**: Enabled
- **XML Documentation**: Generated for IntelliSense

### Build Properties
```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<Title>Andy.TUI Declarative UI Framework</Title>
<Description>SwiftUI-like declarative framework for terminal user interfaces</Description>
```

### Dependencies
```xml
<ProjectReference Include="../Andy.TUI.Core/Andy.TUI.Core.csproj" />
<ProjectReference Include="../Andy.TUI.Terminal/Andy.TUI.Terminal.csproj" />
<ProjectReference Include="../Andy.TUI.Layout/Andy.TUI.Layout.csproj" />
```

### Special Configuration
```xml
<Compile Remove="_broken/**" />
```
Excludes experimental/broken code from compilation.

## Architecture Overview

```
    Declarative API Layer
           │
    ┌──────┴──────┐
    │             │
Component    Uses Layout
Definitions  (Andy.TUI.Layout)
    │             │
    └──────┬──────┘
           │
     ViewInstance
     Management
           │
    ┌──────┴──────┐
    │             │
  State        Event
  System      Handling
    │             │
    └──────┬──────┘
           │
      Rendering
      Pipeline
```

## Core Components

### 1. ISimpleComponent Interface
Base abstraction for all declarative components:
- Render method declaration
- Property binding support
- Lifecycle hooks
- State management

### 2. ViewInstance System
Manages component instances and lifecycle:
- Instance creation and disposal
- State preservation
- Update coordination
- Memory management

### 3. DeclarativeContext
Provides runtime context for declarative UI:
- Component tree management
- Global state access
- Theme and styling
- Event propagation

### 4. Layout Integration
Leverages Andy.TUI.Layout for sophisticated positioning:
- Flexbox-based layout engine
- Stack containers (HStack, VStack from Layout project)
- Grid and constraint systems
- See [Andy.TUI.Layout documentation](../Andy.TUI.Layout/Andy.TUI.Layout.md) for details

### 5. State Management
Reactive state handling for declarative UI:
- @State property wrappers
- @Binding for two-way data flow
- @ObservedObject for external state
- Automatic re-rendering

## Component Hierarchy

```
        ISimpleComponent
               │
    ┌──────────┼──────────┐
    │          │          │
Built-in    Custom    Container
Components  Components Components
    │          │          │
    ├─ Text    │          ├─ VStack
    ├─ Button  │          ├─ HStack
    ├─ Input   │          ├─ ZStack
    ├─ List    │          ├─ ScrollView
    └─ Image   │          └─ Grid
               │
         User-defined
         Components
```

## Declarative Syntax Examples

### Example 1: Simple Component
```
VStack {
    Text("Hello, World!")
        .ForegroundColor(Colors.Blue)
        .Bold()
    
    Button("Click Me")
        .OnClick(() => HandleClick())
}
```

### Example 2: Data Binding
```
@State private string username = "";

Input(binding: $username)
    .Placeholder("Enter username")
    .OnChange(value => Validate(value))
```

### Example 3: Conditional Rendering
```
HStack {
    if (isLoggedIn) {
        UserProfile(user)
    } else {
        LoginButton()
    }
}
```

## Layout Integration

The Declarative module uses layout components from the Andy.TUI.Layout project. Stack layouts (VStack, HStack) and other layout primitives are now provided by the dedicated layout engine.

### Using Layout Components
```csharp
// Layout components come from Andy.TUI.Layout
using Andy.TUI.Declarative.Layout;

VStack {
    Text("Using VStack from Layout project")
    HStack {
        Text("Nested layouts")
        Spacer()
    }
}
```

### Layout Features
- **Stack Layouts**: VStack, HStack, ZStack (provided by Andy.TUI.Layout)
- **Grid System**: Advanced grid layouts with flexible sizing
- **Constraints**: Min/max dimensions, flex properties
- **Spacing**: Comprehensive margin and padding control

For detailed layout documentation, see [Andy.TUI.Layout](../Andy.TUI.Layout/Andy.TUI.Layout.md).

## State Management Patterns

### Local State (@State)
```
Component Instance
       │
    @State ──► Value
       │         │
       └─────────┘
      Auto-update
```

### Shared State (@Binding)
```
Parent Component
    @State ─┐
            │
            ▼
    Child Component
        @Binding
            │
      Two-way sync
```

### Observable Objects
```
ObservableObject ──► Multiple Components
        │                    │
    Published           Auto-update
    Properties          on change
```

## Event System

### Event Flow
```
User Input ──► Component ──► Handler ──► State Update ──► Re-render
                  │              │            │
                  └── Bubbling ──┘            └── Declarative
```

### Focus Management
```
IFocusable Components
        │
    Focus System
        │
    ┌───┴───┐
    │       │
Tab Order  Focus
Navigation  State
```

## Rendering Pipeline

### Render Cycle
```
State Change
     │
     ▼
Mark Dirty
     │
     ▼
Recompute View
     │
     ▼
Diff with Previous
     │
     ▼
Apply Updates
     │
     ▼
Terminal Render
```

### Component Bounds
```
ComponentBounds class:
┌─────────────────────────┐
│ • Position (X, Y)       │
│ • Size (Width, Height)  │
│ • Constraints           │
│ • Padding/Margin        │
└─────────────────────────┘
```

## Built-in Components

### Display Components
- **Text**: Static or dynamic text display
- **Image**: ASCII art or Unicode graphics
- **Spacer**: Flexible space
- **Divider**: Visual separators

### Input Components
- **Button**: Clickable actions
- **Input**: Text input fields
- **Checkbox**: Boolean selection
- **RadioButton**: Single selection
- **Slider**: Range selection

### Container Components
- **VStack**: Vertical layout
- **HStack**: Horizontal layout
- **ZStack**: Layered layout
- **ScrollView**: Scrollable content
- **List**: Dynamic item lists
- **Grid**: Table-like layouts

### Navigation Components
- **TabView**: Tabbed interface
- **NavigationView**: Navigation stack
- **Modal**: Overlay dialogs

## Performance Optimizations

### Lazy Evaluation
```
List(items) { item in
    // Only rendered when visible
    ItemView(item)
}
```

### View Recycling
```
ScrollView ──► Viewport ──► Recycle Pool
                  │              │
              Visible         Reuse
              Items          Instances
```

### Memoization
```
Component ──► Should Update? ──► Cache
                  │                │
                  No              Use
                  │              Cached
                  Yes
                  │
                Render
```

## Usage Examples

### Example: Todo App
```
VStack {
    Text("Todo List").Bold()
    
    Input($newTodo)
        .Placeholder("Add todo...")
        .OnSubmit(() => AddTodo())
    
    List(todos) { todo in
        HStack {
            Checkbox($todo.completed)
            Text(todo.title)
            Spacer()
            Button("Delete")
                .OnClick(() => DeleteTodo(todo))
        }
    }
}
```

### Example: Form Layout
```
Form {
    Section("Personal Info") {
        TextField("Name", $name)
        TextField("Email", $email)
    }
    
    Section("Preferences") {
        Toggle("Notifications", $notifications)
        Picker("Theme", $theme) {
            Option("Light", Theme.Light)
            Option("Dark", Theme.Dark)
        }
    }
    
    Button("Save")
        .Disabled(!isValid)
        .OnClick(() => Save())
}
```

## Best Practices

### Component Design
1. Keep components small and focused
2. Use composition over inheritance
3. Leverage built-in components
4. Implement proper lifecycle methods

### State Management
1. Use @State for local component state
2. Use @Binding for parent-child communication
3. Use @ObservedObject for shared state
4. Avoid excessive state nesting

### Performance
1. Use keys for list items
2. Implement shouldComponentUpdate logic
3. Leverage lazy loading for large lists
4. Minimize re-renders with proper state design

### Layout
1. Use stack layouts for simple arrangements
2. Use Grid for complex table-like layouts
3. Leverage Spacer for flexible spacing
4. Consider terminal size constraints