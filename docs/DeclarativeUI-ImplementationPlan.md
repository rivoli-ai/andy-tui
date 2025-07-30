# Andy.TUI Declarative UI Implementation Plan

## Overview

This plan outlines the step-by-step implementation of the declarative UI framework designed in `DeclarativeUI-Design.md`. We'll build this incrementally while maintaining backward compatibility.

## Phase 1: Foundation Infrastructure (Week 1-2)

### 1.1 Collection Initializer Support for Components

**Goal**: Enable `VStack { Text("Hello"), Button("Click") }` syntax

**Implementation**:
```csharp
// New base class for container components
public abstract class ContainerComponent : ComponentBase, IEnumerable<VirtualNode>
{
    protected readonly List<VirtualNode> _children = new();
    
    // Enable collection initializer syntax
    public void Add(VirtualNode child) => _children.Add(child);
    public void Add(ComponentBase component) => _children.Add(component.Render());
    public void Add(string text) => _children.Add(new TextComponent(text).Render());
    
    public IEnumerator<VirtualNode> GetEnumerator() => _children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// VStack implementation
public class VStack : ContainerComponent
{
    public int Spacing { get; set; } = 0;
    public Alignment Alignment { get; set; } = Alignment.Leading;
    
    public VStack(int spacing = 0, Alignment alignment = Alignment.Leading)
    {
        Spacing = spacing;
        Alignment = alignment;
    }
    
    public override VirtualNode Render()
    {
        // Convert children to vertically stacked layout
        return CreateVerticalLayout(_children, Spacing, Alignment);
    }
}
```

**Files to Create**:
- `src/Andy.TUI.Declarative/ContainerComponent.cs`
- `src/Andy.TUI.Declarative/Layout/VStack.cs`
- `src/Andy.TUI.Declarative/Layout/HStack.cs`
- `src/Andy.TUI.Declarative/Layout/ZStack.cs`

### 1.2 State Management System

**Goal**: Enable `[State]` attributes and automatic UI updates

**Implementation**:
```csharp
// State attribute
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class StateAttribute : Attribute { }

// State property wrapper
public class StateProperty<T> : INotifyPropertyChanged
{
    private T _value;
    
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public static implicit operator T(StateProperty<T> state) => state.Value;
    public static implicit operator StateProperty<T>(T value) => new() { Value = value };
}

// Enhanced ComponentBase with state management
public abstract class ComponentBase : IComponent, INotifyPropertyChanged
{
    private readonly Dictionary<string, object> _stateProperties = new();
    
    protected virtual VirtualNode Body => throw new NotImplementedException("Override Body property");
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public ComponentBase()
    {
        // Use reflection to find [State] fields and set up change tracking
        InitializeStateProperties();
    }
    
    private void InitializeStateProperties()
    {
        var stateFields = GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<StateAttribute>() != null);
        
        foreach (var field in stateFields)
        {
            // Wrap field in StateProperty<T> and set up change notification
            SetupStateProperty(field);
        }
    }
    
    protected void SetState<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            RequestRender();
        }
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public sealed override VirtualNode Render()
    {
        return Body; // Subclasses override Body instead of Render
    }
}
```

**Files to Create**:
- `src/Andy.TUI.Declarative/StateAttribute.cs`
- `src/Andy.TUI.Declarative/StateProperty.cs`
- `src/Andy.TUI.Declarative/DeclarativeComponentBase.cs`

### 1.3 Binding System

**Goal**: Enable two-way data binding with `$variable` syntax

**Implementation**:
```csharp
public class Binding<T> : INotifyPropertyChanged
{
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;
    
    public Binding(Func<T> getter, Action<T> setter)
    {
        _getter = getter;
        _setter = setter;
    }
    
    public T Value
    {
        get => _getter();
        set => _setter(value);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public static implicit operator T(Binding<T> binding) => binding.Value;
}

// Extension methods for creating bindings
public static class BindingExtensions
{
    // Create binding from lambda expressions
    public static Binding<T> Bind<T>(Expression<Func<T>> property)
    {
        var getter = property.Compile();
        var setter = CreateSetter(property);
        return new Binding<T>(getter, setter);
    }
}

// Usage in components:
// TextField("Name", text: this.Bind(() => name))
// Or with special syntax: TextField("Name", text: $name) // via source generator
```

**Files to Create**:
- `src/Andy.TUI.Declarative/Binding.cs`
- `src/Andy.TUI.Declarative/BindingExtensions.cs`

## Phase 2: Core Components (Week 2-3)

### 2.1 Declarative Text Component

**Implementation**:
```csharp
public class Text : ComponentBase
{
    private string _content = "";
    private Color? _foregroundColor;
    private Color? _backgroundColor;
    private bool _isBold;
    private bool _isItalic;
    private TextAlignment _alignment = TextAlignment.Left;
    
    public Text(string content)
    {
        _content = content;
    }
    
    // Fluent modifiers
    public Text ForegroundColor(Color color) 
    { 
        _foregroundColor = color; 
        return this; 
    }
    
    public Text Bold() 
    { 
        _isBold = true; 
        return this; 
    }
    
    public Text Center() 
    { 
        _alignment = TextAlignment.Center; 
        return this; 
    }
    
    // Style shortcuts
    public Text Title() => ForegroundColor(Color.Cyan).Bold();
    public Text Caption() => ForegroundColor(Color.Gray);
    
    protected override VirtualNode Body =>
        Element("text")
            .WithProp("x", X)
            .WithProp("y", Y)
            .WithProp("style", CreateStyle())
            .WithChild(VirtualDomBuilder.Text(_content))
            .Build();
    
    private Style CreateStyle()
    {
        var style = Style.Default;
        if (_foregroundColor.HasValue) style = style.WithForegroundColor(_foregroundColor.Value);
        if (_backgroundColor.HasValue) style = style.WithBackgroundColor(_backgroundColor.Value);
        if (_isBold) style = style.WithBold();
        if (_isItalic) style = style.WithItalic();
        return style;
    }
}
```

### 2.2 Declarative TextField Component

**Implementation**:
```csharp
public class TextField : ComponentBase
{
    private readonly string _title;
    private readonly Binding<string> _text;
    private string? _placeholder;
    private Func<string, bool>? _validator;
    private string? _errorMessage;
    private int? _maxLength;
    private bool _isSecure;
    
    public TextField(string title, Binding<string> text)
    {
        _title = title;
        _text = text;
        
        // Subscribe to text changes
        _text.PropertyChanged += (s, e) => RequestRender();
    }
    
    public TextField Placeholder(string placeholder) 
    { 
        _placeholder = placeholder; 
        return this; 
    }
    
    public TextField Validation(Func<string, bool> validator, string errorMessage)
    {
        _validator = validator;
        _errorMessage = errorMessage;
        return this;
    }
    
    public TextField MaxLength(int length) 
    { 
        _maxLength = length; 
        return this; 
    }
    
    public TextField Secure(bool secure = true) 
    { 
        _isSecure = secure; 
        return this; 
    }
    
    protected override VirtualNode Body
    {
        get
        {
            var isValid = _validator?.Invoke(_text.Value) ?? true;
            var displayText = GetDisplayText();
            
            return VStack(spacing: 0)
            {
                // Input field
                Element("box")
                    .WithProp("width", 30)
                    .WithProp("height", 3)
                    .WithProp("border-style", BoxStyle.Single)
                    .WithProp("style", GetInputStyle(isValid))
                    .WithChild(
                        Element("text")
                            .WithProp("x-offset", 1)
                            .WithProp("y-offset", 1)
                            .WithProp("style", GetTextStyle())
                            .WithChild(VirtualDomBuilder.Text(displayText))
                            .Build()
                    )
                    .Build(),
                
                // Error message
                isValid ? null : new Text(_errorMessage ?? "Invalid input")
                    .ForegroundColor(Color.Red)
                    .Render()
            };
        }
    }
    
    private string GetDisplayText()
    {
        var text = _text.Value;
        if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(_placeholder))
            return _placeholder;
        
        return _isSecure ? new string('•', text.Length) : text;
    }
}
```

### 2.3 Declarative Button Component

**Implementation**:
```csharp
public class Button : ComponentBase
{
    private readonly string _title;
    private readonly Action _action;
    private ButtonStyle _style = ButtonStyle.Default;
    private bool _isDisabled;
    private string? _icon;
    
    public Button(string title, Action action)
    {
        _title = title;
        _action = action;
    }
    
    public Button Primary() { _style = ButtonStyle.Primary; return this; }
    public Button Secondary() { _style = ButtonStyle.Secondary; return this; }
    public Button Danger() { _style = ButtonStyle.Danger; return this; }
    public Button Disabled(bool disabled = true) { _isDisabled = disabled; return this; }
    public Button Icon(string icon) { _icon = icon; return this; }
    
    protected override VirtualNode Body
    {
        get
        {
            var buttonText = string.IsNullOrEmpty(_icon) ? _title : $"{_icon} {_title}";
            
            return Element("box")
                .WithProp("width", buttonText.Length + 4)
                .WithProp("height", 3)
                .WithProp("border-style", GetBorderStyle())
                .WithProp("style", GetButtonStyle())
                .WithChild(
                    Element("text")
                        .WithProp("x-offset", 2)
                        .WithProp("y-offset", 1)
                        .WithProp("style", GetTextStyle())
                        .WithChild(VirtualDomBuilder.Text(buttonText))
                        .Build()
                )
                .WithProp("on-click", _isDisabled ? null : _action)
                .Build();
        }
    }
}
```

**Files to Create**:
- `src/Andy.TUI.Declarative/Components/Text.cs`
- `src/Andy.TUI.Declarative/Components/TextField.cs`
- `src/Andy.TUI.Declarative/Components/Button.cs`
- `src/Andy.TUI.Declarative/Components/Toggle.cs`
- `src/Andy.TUI.Declarative/Components/Picker.cs`

## Phase 3: Advanced Layout & Features (Week 3-4)

### 3.1 Enhanced Layout Components

**Grid Layout**:
```csharp
public class Grid : ContainerComponent
{
    public int Columns { get; set; }
    public int Spacing { get; set; } = 1;
    
    public Grid(int columns, int spacing = 1)
    {
        Columns = columns;
        Spacing = spacing;
    }
    
    protected override VirtualNode Body =>
        CreateGridLayout(_children, Columns, Spacing);
}
```

**Form Section**:
```csharp
public class FormSection : ContainerComponent
{
    protected override VirtualNode Body
    {
        get
        {
            var formRows = _children.Select((child, index) =>
                HStack(spacing: 2)
                {
                    // Auto-generate labels for form fields
                    GetLabelForChild(child),
                    child
                }
            );
            
            return VStack(spacing: 1) { formRows };
        }
    }
}
```

### 3.2 Modifiers System

**Universal Modifiers**:
```csharp
public static class ComponentModifiers
{
    public static T Padding<T>(this T component, int padding) where T : ComponentBase
    {
        component.SetProperty("padding", padding);
        return component;
    }
    
    public static T Background<T>(this T component, Color color) where T : ComponentBase
    {
        component.SetProperty("background", color);
        return component;
    }
    
    public static T Border<T>(this T component, BorderStyle style, Color color) where T : ComponentBase
    {
        component.SetProperty("border-style", style);
        component.SetProperty("border-color", color);
        return component;
    }
    
    public static T MaxWidth<T>(this T component, int width) where T : ComponentBase
    {
        component.SetProperty("max-width", width);
        return component;
    }
    
    public static T Center<T>(this T component) where T : ComponentBase
    {
        component.SetProperty("alignment", Alignment.Center);
        return component;
    }
}
```

### 3.3 Focus Management

**Automatic Tab Order**:
```csharp
public class FocusManager
{
    private readonly List<ComponentBase> _focusableComponents = new();
    private int _currentFocusIndex = 0;
    
    public void RegisterFocusable(ComponentBase component, int? order = null)
    {
        // Auto-assign tab order or use explicit order
    }
    
    public void MoveFocus(int direction)
    {
        // Handle Tab/Shift+Tab navigation
    }
}

// Usage:
TextField("Name", text: $name)
    .FocusOrder(1)
    .OnFocus(() => ShowTooltip("Enter your full name"))
```

## Phase 4: Input Example Rewrite (Week 4)

**Target Implementation**:
```csharp
public class InputDemoApp : ComponentBase
{
    [State] private string name = "";
    [State] private string password = "";
    [State] private string? selectedCountry = null;
    [State] private bool agreedToTerms = false;
    
    private readonly string[] countries = { "USA", "Canada", "UK", "Germany" };
    
    protected override VirtualNode Body =>
        VStack(spacing: 1)
        {
            Text("Andy.TUI Input Components Demo")
                .Title()
                .Center(),
            
            Divider(),
            
            FormSection(spacing: 1)
            {
                TextField("Name", this.Bind(() => name))
                    .Placeholder("Enter your full name")
                    .Validation(n => !string.IsNullOrEmpty(n), "Name is required"),
                
                TextField("Password", this.Bind(() => password))
                    .Placeholder("Enter password")
                    .Secure()
                    .Validation(p => p.Length >= 6, "Password must be at least 6 characters"),
                
                Picker("Country", this.Bind(() => selectedCountry), countries)
                    .Placeholder("Select your country"),
                
                Toggle("I agree to the terms", this.Bind(() => agreedToTerms))
            },
            
            Spacer(),
            
            HStack(spacing: 2)
            {
                Button("Cancel") { Environment.Exit(0); }
                    .Secondary(),
                
                Button("Register") { HandleRegistration(); }
                    .Primary()
                    .Disabled(!IsFormValid)
            }
            .Center(),
            
            Text("Tab/Shift+Tab to navigate • Enter to activate")
                .Caption()
                .Center()
        }
        .Padding(2)
        .MaxWidth(60);
    
    private bool IsFormValid => 
        !string.IsNullOrEmpty(name) && 
        password.Length >= 6 && 
        selectedCountry != null && 
        agreedToTerms;
    
    private void HandleRegistration()
    {
        // Show success message
        ShowNotification($"Welcome, {name}!");
    }
}
```

## Phase 5: Testing & Documentation (Week 5)

### 5.1 Unit Tests
- Test declarative syntax compilation
- Test state binding and updates
- Test component composition
- Test modifier chains

### 5.2 Integration Tests
- Complete form scenarios
- Focus navigation
- State synchronization
- Performance benchmarks

### 5.3 Documentation
- API reference with examples
- Migration guide from imperative to declarative
- Best practices guide
- Interactive tutorials

## Phase 6: Advanced Features (Week 6+)

### 6.1 Animation System
```csharp
Text("Loading...")
    .Opacity(isLoading ? 1.0 : 0.0)
    .Animation(duration: 0.3, curve: .easeInOut)

VStack {
    if (showDetails) {
        DetailPanel()
            .Transition(.slideDown, duration: 0.2)
    }
}
```

### 6.2 Theme System
```csharp
VStack {
    // content...
}
.Theme(AppTheme.Dark)
.AccentColor(Color.Blue)
```

### 6.3 Hot Reload Support
- File watcher for component changes
- State preservation across reloads
- Live preview in development

## Migration Strategy

1. **Backward Compatibility**: Keep existing imperative API working
2. **Gradual Migration**: Allow mixing declarative and imperative components
3. **Conversion Tools**: Provide utilities to convert existing components
4. **Documentation**: Clear migration examples and patterns

## Success Metrics

1. **Developer Experience**: Time to build common UI patterns reduced by 60%
2. **Code Readability**: UI code becomes self-documenting
3. **Maintainability**: State management bugs reduced significantly
4. **Performance**: No degradation in rendering performance
5. **Adoption**: New projects prefer declarative API over imperative

This implementation plan provides a clear roadmap to build a truly declarative, SwiftUI-like terminal UI framework that will transform how developers build TUI applications.