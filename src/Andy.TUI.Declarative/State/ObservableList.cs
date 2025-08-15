using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Andy.TUI.Declarative.State;

/// <summary>
/// Observable list that notifies when items are added, removed, or changed.
/// </summary>
public class ObservableList<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly List<T> _items = new();
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public int Count => _items.Count;
    public bool IsReadOnly => false;
    
    public T this[int index]
    {
        get => _items[index];
        set
        {
            var oldItem = _items[index];
            _items[index] = value;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, value, oldItem, index));
        }
    }
    
    public void Add(T item)
    {
        _items.Add(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, item, _items.Count - 1));
        OnPropertyChanged(nameof(Count));
    }
    
    public void Clear()
    {
        _items.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(nameof(Count));
    }
    
    public bool Contains(T item) => _items.Contains(item);
    
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
    
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    
    public int IndexOf(T item) => _items.IndexOf(item);
    
    public void Insert(int index, T item)
    {
        _items.Insert(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, item, index));
        OnPropertyChanged(nameof(Count));
    }
    
    public bool Remove(T item)
    {
        var index = _items.IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }
    
    public void RemoveAt(int index)
    {
        var item = _items[index];
        _items.RemoveAt(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, item, index));
        OnPropertyChanged(nameof(Count));
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}