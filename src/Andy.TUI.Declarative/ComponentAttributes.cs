using System;
using System.ComponentModel;

namespace Andy.TUI.Declarative;

/// <summary>
/// Attribute to mark methods that should only be used by collection initializers
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal class CollectionInitializerOnlyAttribute : Attribute
{
    public string Message { get; }

    public CollectionInitializerOnlyAttribute(string message = "This method should only be used through collection initializer syntax { }.")
    {
        Message = message;
    }
}

/// <summary>
/// Base class for components that support SwiftUI-style collection initializers
/// </summary>
public abstract class CollectionInitializerComponent
{
    private bool _isInitialized = false;

    protected void EnsureNotInitialized()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException(
                "Cannot add children after initialization. Use collection initializer syntax: new Component { child1, child2 }");
        }
    }

    protected void MarkInitialized()
    {
        _isInitialized = true;
    }
}