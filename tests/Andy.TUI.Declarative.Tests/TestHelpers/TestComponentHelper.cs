using System;
using System.Collections.Generic;
using System.Reflection;
using Andy.TUI.VirtualDom;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using static Andy.TUI.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

/// <summary>
/// Helper for working with test components in the declarative system.
/// </summary>
public static class TestComponentHelper
{
    /// <summary>
    /// Creates a ViewInstance for test components by using the existing CreateGenericInstance path.
    /// </summary>
    public static ViewInstance GetOrCreateTestInstance(DeclarativeContext context, ISimpleComponent component, string path)
    {
        var manager = context.ViewInstanceManager;

        // For our test components, we'll create wrapper components that the system can handle
        ISimpleComponent wrappedComponent = component switch
        {
            FixedSizeComponent fixedSize => new TestComponentWrapper(fixedSize, () => new FixedSizeInstance(path)),
            AutoSizeComponent autoSize => new TestComponentWrapper(autoSize, () => new AutoSizeInstance(path)),
            TestContainer container => new TestComponentWrapper(container, () => new TestContainerInstance(path)),
            ExtremeValueComponent extreme => new TestComponentWrapper(extreme, () => new ExtremeValueInstance(path)),
            _ => component
        };

        // Use reflection to call CreateGenericInstance which will use our wrapper
        var createGenericMethod = manager.GetType().GetMethod("CreateGenericInstance",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (createGenericMethod != null && wrappedComponent is TestComponentWrapper)
        {
            var wrapper = (TestComponentWrapper)wrappedComponent;
            var instance = wrapper.InstanceFactory();
            instance.Context = context;
            instance.Update(wrapper.InnerComponent);
            return instance;
        }

        // Fallback to regular GetOrCreateInstance
        return manager.GetOrCreateInstance(component, path);
    }
}

/// <summary>
/// Wrapper component that helps integrate test components into the system.
/// </summary>
internal class TestComponentWrapper : ISimpleComponent
{
    public ISimpleComponent InnerComponent { get; }
    public Func<ViewInstance> InstanceFactory { get; }

    public TestComponentWrapper(ISimpleComponent innerComponent, Func<ViewInstance> instanceFactory)
    {
        InnerComponent = innerComponent;
        InstanceFactory = instanceFactory;
    }

    public ISimpleComponent Copy() => new TestComponentWrapper(InnerComponent, InstanceFactory);

    public VirtualNode Render() => Fragment();
}

/// <summary>
/// Test-friendly DeclarativeContext that simplifies creating test instances.
/// </summary>
public class TestDeclarativeContext : DeclarativeContext
{
    private readonly Dictionary<string, ViewInstance> _testInstances = new();

    public TestDeclarativeContext() : base(() => { })
    {
    }

    /// <summary>
    /// Gets or creates an instance, handling test components specially.
    /// </summary>
    public ViewInstance GetTestInstance(ISimpleComponent component, string path)
    {
        // For test components, create them directly
        if (component is FixedSizeComponent ||
            component is AutoSizeComponent ||
            component is TestContainer ||
            component is ExtremeValueComponent)
        {
            var key = $"{path}:{component.GetType().Name}";

            if (!_testInstances.TryGetValue(key, out var instance))
            {
                instance = component switch
                {
                    FixedSizeComponent => new FixedSizeInstance(key),
                    AutoSizeComponent => new AutoSizeInstance(key),
                    TestContainer => new TestContainerInstance(key),
                    ExtremeValueComponent => new ExtremeValueInstance(key),
                    _ => throw new NotSupportedException($"Unsupported test component: {component.GetType().Name}")
                };

                instance.Context = this;
                _testInstances[key] = instance;
            }

            instance.Update(component);
            return instance;
        }

        // For regular components, use the standard manager
        return ViewInstanceManager.GetOrCreateInstance(component, path);
    }
}