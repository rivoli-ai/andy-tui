# Declarative Event System Design

## Overview

This document outlines the event routing and binding system for Andy.TUI.Declarative, inspired by SwiftUI and WPF architectures.

## Key Concepts from SwiftUI and WPF

### SwiftUI Architecture
- **@State**: Property wrapper that makes view state observable
- **@Binding**: Two-way connection between a property and a view
- **View Updates**: Automatic re-rendering when @State changes
- **Event Handling**: Declarative modifiers like `.onTapGesture`, `.onChange`
- **Focus Management**: `@FocusState` for keyboard navigation

### WPF Architecture
- **Routed Events**: Events bubble up or tunnel down the visual tree
- **Data Binding**: INotifyPropertyChanged for change notifications
- **Command Pattern**: ICommand for decoupled action handling
- **Focus Management**: Logical and keyboard focus with tab navigation
- **Hit Testing**: Determining which element is under the mouse

## Proposed Architecture for Andy.TUI

### 1. Component Tree and Context

```csharp
public class DeclarativeContext
{
    public FocusManager FocusManager { get; }
    public EventRouter EventRouter { get; }
    public RenderingContext RenderingContext { get; }
    public ComponentTree Tree { get; }
}
```

### 2. Event Routing System

```csharp
public interface IEventHandler
{
    bool HandleKeyPress(KeyEventArgs args);
    bool HandleMouseEvent(MouseEventArgs args);
}

public class EventRouter
{
    // Routes events from terminal to focused component
    public void RouteKeyPress(ConsoleKeyInfo keyInfo)
    {
        var focused = _focusManager.FocusedComponent;
        if (focused?.HandleKeyPress(keyInfo) == true)
            return;
            
        // Bubble up to parent
        BubbleEvent(focused, keyInfo);
    }
    
    // Hit testing for mouse events
    public void RouteMouseEvent(int x, int y, MouseButton button)
    {
        var component = HitTest(x, y);
        component?.HandleMouseEvent(new MouseEventArgs(x, y, button));
    }
}
```

### 3. Focus Management

```csharp
public class FocusManager
{
    private IFocusable? _focusedComponent;
    private List<IFocusable> _focusableComponents = new();
    
    public void MoveFocus(FocusDirection direction)
    {
        // Tab navigation logic
        var next = GetNextFocusable(direction);
        SetFocus(next);
    }
    
    public void SetFocus(IFocusable? component)
    {
        _focusedComponent?.OnLostFocus();
        _focusedComponent = component;
        _focusedComponent?.OnGotFocus();
    }
}

public interface IFocusable : ISimpleComponent
{
    bool CanFocus { get; }
    bool IsFocused { get; }
    void OnGotFocus();
    void OnLostFocus();
}
```

### 4. Binding System with Change Notifications

```csharp
public class Binding<T> : INotifyPropertyChanged
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
}

// Components subscribe to binding changes
public class TextField : IFocusable
{
    private Binding<string> _text;
    
    public TextField(string placeholder, Binding<string> text)
    {
        _text = text;
        _text.PropertyChanged += OnTextChanged;
    }
    
    private void OnTextChanged(object sender, PropertyChangedEventArgs e)
    {
        // Request re-render
        Context.RequestRender();
    }
}
```

### 5. Component Lifecycle

```csharp
public abstract class DeclarativeComponent : ISimpleComponent, IEventHandler
{
    protected DeclarativeContext? Context { get; private set; }
    
    // Called when component is added to tree
    public virtual void OnMount(DeclarativeContext context)
    {
        Context = context;
        if (this is IFocusable focusable)
            context.FocusManager.RegisterFocusable(focusable);
    }
    
    // Called when component is removed
    public virtual void OnUnmount()
    {
        if (this is IFocusable focusable)
            Context?.FocusManager.UnregisterFocusable(focusable);
        Context = null;
    }
}
```

### 6. Declarative Event Handlers

```csharp
// SwiftUI-style event modifiers
public static class EventModifiers
{
    public static T OnKeyPress<T>(this T component, Action<KeyEventArgs> handler) 
        where T : ISimpleComponent
    {
        // Attach handler to component
        return component;
    }
    
    public static T OnSubmit<T>(this T component, Action handler)
        where T : ISimpleComponent
    {
        // Attach submit handler
        return component;
    }
}

// Usage:
new TextField("Enter name", name)
    .OnSubmit(() => HandleSubmit())
    .OnKeyPress(e => {
        if (e.Key == ConsoleKey.Escape)
            ClearField();
    })
```

### 7. Render Loop Integration

```csharp
public class DeclarativeRenderer
{
    private DeclarativeContext _context;
    private bool _needsRender;
    
    public void Run(ISimpleComponent root)
    {
        // Initial render
        Render(root);
        
        // Event loop
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            
            // Route event
            _context.EventRouter.RouteKeyPress(keyInfo);
            
            // Re-render if needed
            if (_needsRender)
            {
                Render(root);
                _needsRender = false;
            }
        }
    }
    
    public void RequestRender()
    {
        _needsRender = true;
    }
}
```

## Implementation Plan

1. **Phase 1**: Focus Management
   - Implement FocusManager
   - Add IFocusable interface
   - Update TextField and Button to be focusable
   - Add Tab navigation

2. **Phase 2**: Event Routing
   - Implement EventRouter
   - Add keyboard event handling to components
   - Wire up to terminal input

3. **Phase 3**: Binding Improvements
   - Add PropertyChanged notifications
   - Update components to react to binding changes
   - Implement automatic re-rendering

4. **Phase 4**: Mouse Support
   - Add hit testing
   - Implement mouse event routing
   - Update Button for click handling

## Benefits

1. **True Declarative UI**: Components describe what they are, not how to handle events imperatively
2. **Automatic Updates**: Changes to bindings trigger re-renders automatically
3. **Proper Focus Management**: Tab navigation works out of the box
4. **Event Bubbling**: Unhandled events bubble up the component tree
5. **Testable**: Event handling is decoupled from terminal I/O