using Andy.TUI.Declarative.State;

namespace Andy.TUI.Examples.ZIndex;

/// <summary>
/// Helper methods for creating state bindings in examples.
/// </summary>
public static class StateHelpers
{
    /// <summary>
    /// Creates a simple state binding with a backing field.
    /// </summary>
    public static Binding<T> State<T>(T initialValue)
    {
        var value = initialValue;
        return new Binding<T>(
            () => value,
            v => value = v
        );
    }
    
    /// <summary>
    /// Converts a regular binding to an optional binding.
    /// </summary>
    public static Binding<Optional<T>> ConvertToOptionalBinding<T>(Binding<T> binding)
    {
        return new Binding<Optional<T>>(
            () => Optional<T>.Some(binding.Value),
            v => 
            {
                if (v.HasValue)
                    binding.Value = v.Value;
            }
        );
    }
}