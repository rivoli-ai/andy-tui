using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Andy.TUI.Declarative.State;

/// <summary>
/// Simple binding class for two-way data binding in declarative UI.
/// </summary>
public class Binding<T> : INotifyPropertyChanged
{
    private readonly Func<T> _getter;
    private readonly Action<T> _setter;
    private readonly string _propertyName;

    public T Value
    {
        get => _getter();
        set
        {
            var oldValue = _getter();
            if (!EqualityComparer<T>.Default.Equals(oldValue, value))
            {
                _setter(value);
                OnPropertyChanged();
            }
        }
    }

    public string PropertyName => _propertyName;

    public Binding(Func<T> getter, Action<T> setter, string propertyName = "")
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        _propertyName = propertyName;
    }

    public static implicit operator T(Binding<T> binding) => binding.Value;

    public override string ToString() => Value?.ToString() ?? "";
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}