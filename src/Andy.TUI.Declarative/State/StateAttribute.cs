namespace Andy.TUI.Declarative.State;

/// <summary>
/// Marks a field or property as state that should trigger UI updates when changed.
/// Similar to SwiftUI's @State property wrapper.
/// </summary>
/// <example>
/// <code>
/// [State] private string name = "";
/// [State] private bool isEnabled = true;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class StateAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the state property for debugging purposes.
    /// If not specified, the field or property name will be used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether this state should be persisted across component recreations.
    /// Default is false.
    /// </summary>
    public bool Persistent { get; set; } = false;

    /// <summary>
    /// Initializes a new StateAttribute.
    /// </summary>
    public StateAttribute()
    {
    }

    /// <summary>
    /// Initializes a new StateAttribute with a custom name.
    /// </summary>
    /// <param name="name">The name of the state property for debugging.</param>
    public StateAttribute(string name)
    {
        Name = name;
    }
}