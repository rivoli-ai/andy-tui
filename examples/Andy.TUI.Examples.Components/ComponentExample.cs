using System;
using Andy.TUI.Components;
using Andy.TUI.Components.EventHandling;
using Andy.TUI.Core.Observable;
using Andy.TUI.Core.VirtualDom;
using Microsoft.Extensions.DependencyInjection;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Examples.Components;

/// <summary>
/// Example demonstrating basic component usage with lifecycle, state management, and event handling.
/// </summary>
public class ComponentExample
{
    public static void Run()
    {
        Console.WriteLine("=== Andy.TUI Component Example ===\n");
        
        // Setup services
        var services = new ServiceCollection()
            .AddSingleton<IThemeProvider, ThemeProvider>()
            .AddSingleton<ISharedStateManager, SharedStateManager>()
            .AddSingleton<EventManager>()
            .BuildServiceProvider();
        
        // Create and run the example
        var example = new ComponentExample();
        example.RunCounterExample(services);
        example.RunSharedStateExample(services);
        example.RunEventHandlingExample(services);
    }
    
    private void RunCounterExample(IServiceProvider services)
    {
        Console.WriteLine("--- Counter Component Example ---");
        
        var themeProvider = services.GetRequiredService<IThemeProvider>();
        var sharedStateManager = services.GetRequiredService<ISharedStateManager>();
        
        // Create a simple counter component
        var counter = new CounterComponent();
        var context = new ComponentContext(counter, services, themeProvider, sharedStateManager);
        
        // Initialize and mount the component
        counter.Initialize(context);
        counter.OnMount();
        
        // Render initial state
        Console.WriteLine("Initial render:");
        var virtualNode = counter.Render();
        PrintVirtualNode(virtualNode, 0);
        
        // Increment counter and re-render
        counter.Increment();
        Console.WriteLine("\nAfter increment:");
        virtualNode = counter.Render();
        PrintVirtualNode(virtualNode, 0);
        
        // Reset counter and re-render
        counter.Reset();
        Console.WriteLine("\nAfter reset:");
        virtualNode = counter.Render();
        PrintVirtualNode(virtualNode, 0);
        
        // Cleanup
        counter.Dispose();
        Console.WriteLine("\nCounter component disposed.\n");
    }
    
    private void RunSharedStateExample(IServiceProvider services)
    {
        Console.WriteLine("--- Shared State Example ---");
        
        var themeProvider = services.GetRequiredService<IThemeProvider>();
        var sharedStateManager = services.GetRequiredService<ISharedStateManager>();
        
        // Create two components that share state
        var component1 = new SharedStateComponent("Component1");
        var component2 = new SharedStateComponent("Component2");
        
        var context1 = new ComponentContext(component1, services, themeProvider, sharedStateManager);
        var context2 = new ComponentContext(component2, services, themeProvider, sharedStateManager);
        
        // Initialize components
        component1.Initialize(context1);
        component2.Initialize(context2);
        component1.OnMount();
        component2.OnMount();
        
        // Set shared value from first component
        component1.SetSharedMessage("Hello from Component1!");
        
        // Render both components
        Console.WriteLine("Component1 render:");
        PrintVirtualNode(component1.Render(), 0);
        
        Console.WriteLine("Component2 render:");
        PrintVirtualNode(component2.Render(), 0);
        
        // Update shared value from second component
        component2.SetSharedMessage("Hello from Component2!");
        
        Console.WriteLine("\nAfter updating shared state:");
        Console.WriteLine("Component1 render:");
        PrintVirtualNode(component1.Render(), 0);
        
        Console.WriteLine("Component2 render:");
        PrintVirtualNode(component2.Render(), 0);
        
        // Cleanup
        component1.Dispose();
        component2.Dispose();
        Console.WriteLine("\nShared state components disposed.\n");
    }
    
    private void RunEventHandlingExample(IServiceProvider services)
    {
        Console.WriteLine("--- Event Handling Example ---");
        
        var eventManager = services.GetRequiredService<EventManager>();
        var themeProvider = services.GetRequiredService<IThemeProvider>();
        var sharedStateManager = services.GetRequiredService<ISharedStateManager>();
        
        // Create a component that publishes events
        var publisher = new EventPublisherComponent();
        var context = new ComponentContext(publisher, services, themeProvider, sharedStateManager);
        
        publisher.Initialize(context);
        publisher.OnMount();
        
        // Subscribe to events
        var subscription = eventManager.Subscribe<InteractionEventArgs>(
            publisher.Id,
            eventArgs => Console.WriteLine($"Received event: {eventArgs.InteractionType} with data: {eventArgs.Data}"));
        
        // Trigger some events
        publisher.TriggerClick("Button clicked!");
        publisher.TriggerKeyPress("Enter key pressed!");
        
        // Cleanup
        subscription.Dispose();
        publisher.Dispose();
        Console.WriteLine("Event handling example completed.\n");
    }
    
    private static void PrintVirtualNode(VirtualNode node, int indent)
    {
        var indentStr = new string(' ', indent * 2);
        
        switch (node)
        {
            case TextNode textNode:
                Console.WriteLine($"{indentStr}Text: \"{textNode.Content}\"");
                break;
            case ElementNode elementNode:
                Console.WriteLine($"{indentStr}Element: {elementNode.TagName}");
                foreach (var child in elementNode.Children)
                {
                    PrintVirtualNode(child, indent + 1);
                }
                break;
            case FragmentNode fragmentNode:
                Console.WriteLine($"{indentStr}Fragment:");
                foreach (var child in fragmentNode.Children)
                {
                    PrintVirtualNode(child, indent + 1);
                }
                break;
            default:
                Console.WriteLine($"{indentStr}Unknown node type: {node.GetType().Name}");
                break;
        }
    }
}

/// <summary>
/// Example counter component demonstrating state management and property binding.
/// </summary>
public class CounterComponent : ComponentBase
{
    private ObservableProperty<int> _count = null!;
    
    protected override void OnInitialize()
    {
        _count = CreateObservableProperty("Count", 0);
        base.OnInitialize();
    }
    
    protected override VirtualNode OnRender()
    {
        return VBox()
            .WithChildren(
                Text($"Count: {_count.Value}"),
                HBox()
                    .WithChildren(
                        Element("button")
                            .WithChildren(Text("Increment")),
                        Element("button")
                            .WithChildren(Text("Reset"))
                    )
            )
            .Build();
    }
    
    public void Increment()
    {
        _count.Value++;
    }
    
    public void Reset()
    {
        _count.Value = 0;
    }
}

/// <summary>
/// Example component demonstrating shared state usage.
/// </summary>
public class SharedStateComponent : ComponentBase
{
    private readonly string _name;
    
    public SharedStateComponent(string name)
    {
        _name = name;
    }
    
    protected override VirtualNode OnRender()
    {
        var sharedMessage = Context.GetSharedValue<string>("SharedMessage") ?? "No message";
        
        return Element("div")
            .WithChildren(Text($"{_name}: {sharedMessage}"))
            .Build();
    }
    
    public void SetSharedMessage(string message)
    {
        Context.SetSharedValue("SharedMessage", message);
        RequestRender();
    }
}

/// <summary>
/// Example component demonstrating event publishing.
/// </summary>
public class EventPublisherComponent : ComponentBase
{
    protected override VirtualNode OnRender()
    {
        return Element("div")
            .WithChildren(Text("Event Publisher Component"))
            .Build();
    }
    
    public void TriggerClick(string data)
    {
        // In a real implementation, this would be triggered by user interaction
        var eventArgs = new InteractionEventArgs(this, InteractionType.Click, data);
        Console.WriteLine($"Publishing click event with data: {data}");
        
        // Simulate event publishing (normally handled by the framework)
        // For this example, we'll just demonstrate the event structure
    }
    
    public void TriggerKeyPress(string data)
    {
        var eventArgs = new InteractionEventArgs(this, InteractionType.KeyPress, data);
        Console.WriteLine($"Publishing key press event with data: {data}");
    }
}