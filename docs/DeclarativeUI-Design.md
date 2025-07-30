# Andy.TUI Declarative UI Framework Design

## Vision: SwiftUI-like Declarative Terminal UI

The goal is to create a declarative UI framework for terminal applications that feels as natural and elegant as SwiftUI, with inspiration from WPF's XAML.

## Core Principles

### 1. Declarative Structure
UI should be declared as a tree structure, not built imperatively with AddChild calls.

**❌ Current (Imperative)**
```csharp
var stack = new Stack();
stack.AddChild(textComponent.Render());
stack.AddChild(buttonComponent.Render());
```

**✅ Target (Declarative)**
```csharp
VStack {
    Text("Hello World")
        .ForegroundColor(Color.Blue)
        .Bold(),
    
    TextField("Name", text: $name)
        .Placeholder("Enter your name"),
    
    Button("Submit") { 
        Submit(); 
    }
    .Disabled(name.IsEmpty)
}
.Padding(2)
```

### 2. State Management with Automatic UI Updates
State changes should automatically trigger UI updates, similar to SwiftUI's `@State` and WPF's data binding.

**✅ SwiftUI-style State**
```csharp
class MyView : ComponentBase 
{
    [State] private string name = "";
    [State] private bool isEnabled = true;
    
    public override VirtualNode Body => 
        VStack {
            TextField("Name", text: $name),  // Automatic binding
            Button("Submit") { Submit(); }
                .Disabled(!isEnabled)        // Reactive to state
        };
}
```

### 3. Component Composition
Components should compose naturally within each other.

**✅ Natural Composition**
```csharp
VStack {
    HeaderSection(),
    ContentSection(),
    FooterSection()
}

private VirtualNode HeaderSection() =>
    HStack {
        Text("Andy.TUI Demo").Title(),
        Spacer(),
        StatusIndicator(isConnected)
    };
```

### 4. Fluent Modifiers
Visual styling should be applied through fluent modifiers.

**✅ Fluent Styling**
```csharp
Text("Important Message")
    .ForegroundColor(Color.Red)
    .Bold()
    .Border(BorderStyle.Double)
    .Padding(1)
    .Background(Color.DarkRed)
```

## Complete Example: Form Input

Here's how a complete form should look:

```csharp
public class InputFormView : ComponentBase
{
    [State] private string name = "";
    [State] private string email = "";
    [State] private string? selectedCountry = null;
    [State] private bool agreedToTerms = false;
    
    private readonly string[] countries = { "USA", "Canada", "UK", "Germany" };
    
    public override VirtualNode Body =>
        VStack(spacing: 1) {
            // Title
            Text("User Registration")
                .Title()
                .ForegroundColor(Color.Cyan)
                .Center(),
            
            Divider(),
            
            // Form Fields
            FormSection {
                TextField("Name", text: $name)
                    .Placeholder("Enter your full name")
                    .Validation(text => !string.IsNullOrEmpty(text), "Name is required"),
                
                TextField("Email", text: $email)
                    .Placeholder("user@example.com")
                    .Validation(IsValidEmail, "Please enter a valid email"),
                
                Picker("Country", selection: $selectedCountry, options: countries)
                    .Placeholder("Select your country"),
                
                Toggle("I agree to the terms", isOn: $agreedToTerms)
            },
            
            Spacer(),
            
            // Actions
            HStack(spacing: 2) {
                Button("Cancel") { 
                    Cancel(); 
                }
                .Secondary(),
                
                Button("Register") { 
                    Register(); 
                }
                .Primary()
                .Disabled(!IsFormValid)
            }
            .Center(),
            
            // Help Text
            Text("Tab/Shift+Tab to navigate • Enter to activate")
                .Caption()
                .ForegroundColor(Color.Gray)
                .Center()
        }
        .Padding(2)
        .MaxWidth(60);
    
    private bool IsFormValid => 
        !string.IsNullOrEmpty(name) && 
        IsValidEmail(email) && 
        selectedCountry != null && 
        agreedToTerms;
    
    private void Register()
    {
        // Handle registration
        ShowSuccess($"Welcome, {name}!");
    }
}
```

## Core Components API

### Layout Components

```csharp
// Vertical Stack
VStack(spacing: int = 0, alignment: Alignment = .leading) {
    // children...
}

// Horizontal Stack  
HStack(spacing: int = 0, alignment: Alignment = .center) {
    // children...
}

// Z-Stack (overlapping)
ZStack(alignment: Alignment = .center) {
    // children...
}

// Grid
Grid(columns: int, spacing: int = 1) {
    // children...
}

// Form (automatic label/input pairing)
FormSection(spacing: int = 1) {
    // form controls...
}
```

### Input Components

```csharp
// Text Field
TextField(title: string, text: Binding<string>)
    .Placeholder(string)
    .Validation(Func<string, bool>, string errorMessage)
    .MaxLength(int)
    .Secure(bool)  // for passwords

// Multi-line Text
TextArea(text: Binding<string>)
    .Placeholder(string)
    .Lines(int)

// Button
Button(title: string, action: Action)
    .Primary() / .Secondary() / .Danger()
    .Disabled(bool)
    .Icon(string)

// Toggle/Checkbox
Toggle(title: string, isOn: Binding<bool>)

// Picker/Dropdown
Picker(title: string, selection: Binding<T>, options: T[])
    .Placeholder(string)

// Slider
Slider(value: Binding<int>, range: Range<int>)
    .Step(int)
```

### Display Components

```csharp
// Text
Text(string content)
    .Title() / .Subtitle() / .Caption()
    .Bold() / .Italic()
    .ForegroundColor(Color)
    .Background(Color)

// Progress
ProgressBar(value: double, max: double = 1.0)
    .Animated(bool)

// List
List(items: T[], itemBuilder: Func<T, VirtualNode>)
    .Selectable(Binding<T>)

// Table
Table(data: T[][], headers: string[])
    .Sortable(bool)
    .Selectable(bool)
```

### Utility Components

```csharp
// Spacer (flexible space)
Spacer(minLength: int = 0)

// Divider
Divider()
    .Color(Color)
    .Style(LineStyle)

// Border
Border(child: VirtualNode)
    .Style(BorderStyle)
    .Color(Color)
    .Padding(int)
```

## State Management System

### State Binding
```csharp
// Property wrapper for reactive state
[State] private string name = "";

// Computed properties that automatically update
private string DisplayName => string.IsNullOrEmpty(name) ? "Guest" : name;

// Two-way binding syntax
TextField("Name", text: $name)  // $ creates Binding<string>
```

### Observable Properties
```csharp
public class UserModel : INotifyPropertyChanged
{
    private string _name = "";
    public string Name 
    { 
        get => _name; 
        set => SetProperty(ref _name, value); 
    }
}

// Bind to model properties
TextField("Name", text: $userModel.Name)
```

## Event Handling

### Declarative Events
```csharp
Button("Save") { 
    SaveData(); 
}
.OnLongPress(() => SaveAsDialog())
.OnHover(() => ShowTooltip("Save current document"))

TextField("Search", text: $searchText)
    .OnTextChanged(text => PerformSearch(text))
    .OnSubmit(() => OpenFirstResult())
```

### Keyboard Navigation
```csharp
VStack {
    TextField("Name", text: $name)
        .FocusOrder(1),
    
    TextField("Email", text: $email)
        .FocusOrder(2),
    
    Button("Submit") { Submit(); }
        .FocusOrder(3)
        .Default(true)  // Activates on Enter from anywhere
}
.OnKeyPress(key => HandleGlobalKey(key))
```

## Animation & Transitions

```csharp
Text("Loading...")
    .Opacity(isLoading ? 1.0 : 0.0)
    .Animation(.easeInOut(duration: 0.3))

VStack {
    if (showDetails) {
        DetailPanel()
            .Transition(.slideDown)
    }
}
```

## Styling & Theming

### Style Modifiers
```csharp
Text("Error Message")
    .ForegroundColor(.red)
    .Background(.darkRed)
    .Bold()
    .Border(.single, .red)
    .Padding(1)
    .Corner(.rounded)
```

### Theme System
```csharp
// Define themes
public static class AppTheme 
{
    public static Theme Dark => new Theme
    {
        Primary = Color.Blue,
        Secondary = Color.Gray,
        Background = Color.Black,
        Text = Color.White,
        Error = Color.Red
    };
}

// Apply themes
VStack {
    // content...
}
.Theme(AppTheme.Dark)
```

## Implementation Plan

### Phase 1: Core Infrastructure
1. **Declarative Syntax Parser**
   - C# collection initializer syntax for component children
   - Fluent modifier chain support
   - Type-safe component composition

2. **State Management**
   - `[State]` attribute implementation
   - Automatic change detection and UI updates
   - Binding<T> system for two-way data flow

3. **Component Base Architecture**
   - Abstract ComponentBase with Body property
   - VirtualNode generation from declarative syntax
   - Automatic re-rendering on state changes

### Phase 2: Layout & Basic Components
1. **Layout Containers**
   - VStack, HStack, ZStack
   - Grid, FormSection
   - Spacer, Divider

2. **Input Components**
   - TextField with validation
   - Button with styling
   - Toggle, Picker

3. **Display Components**
   - Text with modifiers
   - ProgressBar, List

### Phase 3: Advanced Features
1. **Focus Management**
   - Automatic tab navigation
   - Focus order control
   - Keyboard shortcuts

2. **Animation System**
   - Smooth transitions
   - Property animations
   - Layout animations

3. **Theme System**
   - Global theme support
   - Component-level styling
   - Dynamic theme switching

### Phase 4: Developer Experience
1. **Hot Reload**
   - Live UI updates during development
   - State preservation across reloads

2. **Debugging Tools**
   - Component tree inspector
   - State change tracking
   - Performance profiler

3. **Documentation & Examples**
   - Comprehensive API documentation
   - Interactive tutorials
   - Real-world examples

## Migration Path

The new declarative API should coexist with the current imperative API, allowing gradual migration:

```csharp
// Legacy support
var oldStack = new Stack();
oldStack.AddChild(text.Render());

// New declarative (preferred)
var newStack = VStack {
    Text("Hello World")
};

// Mixed usage during migration
VStack {
    Text("New declarative"),
    oldStack.ToDeclarative()  // Adapter method
}
```

This design provides the elegance and power of SwiftUI while being tailored for terminal applications, making it a joy to build complex TUI applications.