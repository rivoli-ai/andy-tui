using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Andy.TUI.Declarative.State;

/// <summary>
/// Observable property that notifies when its value changes.
/// </summary>
public class ObservableProperty<T> : INotifyPropertyChanged
{
    private T _value;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
    
    public ObservableProperty(T initialValue = default!)
    {
        _value = initialValue;
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public static implicit operator T(ObservableProperty<T> property) => property.Value;
    
    public override string ToString() => _value?.ToString() ?? "";
}