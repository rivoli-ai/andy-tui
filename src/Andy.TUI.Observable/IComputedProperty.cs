namespace Andy.TUI.Core.Observable;

/// <summary>
/// Represents a computed property that automatically updates when its dependencies change.
/// </summary>
public interface IComputedProperty : IObservableProperty
{
    /// <summary>
    /// Gets the dependencies of this computed property.
    /// </summary>
    IReadOnlySet<IObservableProperty> Dependencies { get; }
    
    /// <summary>
    /// Forces the computed property to recalculate its value.
    /// </summary>
    void Invalidate();
    
    /// <summary>
    /// Gets a value indicating whether the computed value is currently valid.
    /// </summary>
    bool IsValid { get; }
}

/// <summary>
/// Represents a strongly-typed computed property.
/// </summary>
/// <typeparam name="T">The type of the computed value.</typeparam>
public interface IComputedProperty<T> : IComputedProperty, IObservableProperty<T>
{
    /// <summary>
    /// Gets the computation function used to calculate the value.
    /// </summary>
    Func<T> Computation { get; }
}