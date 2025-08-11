namespace Andy.TUI.Declarative.State;

/// <summary>
/// Represents an optional value that can be None or Some(value).
/// </summary>
public readonly struct Optional<T>
{
    private readonly bool _hasValue;
    private readonly T? _value;
    
    public bool HasValue => _hasValue;
    public T Value => _hasValue ? _value! : throw new InvalidOperationException("Optional has no value");
    
    public static Optional<T> None => new Optional<T>();
    public static Optional<T> Some(T value) => new Optional<T>(value);
    
    private Optional(T value)
    {
        _hasValue = true;
        _value = value;
    }
    
    public bool TryGetValue(out T value)
    {
        if (_hasValue)
        {
            value = _value!;
            return true;
        }
        value = default!;
        return false;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is Optional<T> other)
        {
            if (!_hasValue && !other._hasValue) return true;
            if (_hasValue && other._hasValue) return EqualityComparer<T>.Default.Equals(_value, other._value);
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return _hasValue ? _value?.GetHashCode() ?? 0 : -1;
    }
    
    public override string ToString()
    {
        return _hasValue ? _value?.ToString() ?? "Some(null)" : "None";
    }
    
    public static implicit operator Optional<T>(T value) => Some(value);
}