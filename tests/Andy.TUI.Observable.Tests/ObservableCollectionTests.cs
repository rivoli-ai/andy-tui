using Andy.TUI.Core.Observable;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Andy.TUI.Core.Tests.Observable;

public class ObservableCollectionTests
{
    [Fact]
    public void Constructor_Empty_CreatesEmptyCollection()
    {
        var collection = new ObservableCollection<int>();
        
        Assert.Empty(collection);
        Assert.Empty(collection);
    }

    [Fact]
    public void Constructor_WithItems_InitializesCollection()
    {
        var items = new[] { 1, 2, 3 };
        var collection = new ObservableCollection<int>(items);
        
        Assert.Equal(3, collection.Count);
        Assert.Equal(items, collection);
    }

    [Fact]
    public void Add_SingleItem_NotifiesChange()
    {
        var collection = new ObservableCollection<string>();
        var collectionChangedCount = 0;
        var propertyChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
            Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
            Assert.Equal(0, e.NewStartingIndex);
            Assert.Single(e.NewItems!);
            Assert.Equal("test", e.NewItems![0]);
        };
        
        collection.PropertyChanged += (s, e) =>
        {
            propertyChangedCount++;
            Assert.Contains(e.PropertyName, new[] { "Count", "Item[]" });
        };
        
        collection.Add("test");
        
        Assert.Single(collection);
        Assert.Equal("test", collection[0]);
        Assert.Equal(1, collectionChangedCount);
        Assert.True(propertyChangedCount >= 2); // Count and Item[] should change
    }

    [Fact]
    public void Remove_ExistingItem_NotifiesChange()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
            Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
            Assert.Equal(1, e.OldStartingIndex);
            Assert.Single(e.OldItems!);
            Assert.Equal(2, e.OldItems![0]);
        };
        
        var removed = collection.Remove(2);
        
        Assert.True(removed);
        Assert.Equal(2, collection.Count);
        Assert.Equal(new[] { 1, 3 }, collection);
        Assert.Equal(1, collectionChangedCount);
    }

    [Fact]
    public void Clear_NotifiesReset()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
            Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
        };
        
        collection.Clear();
        
        Assert.Empty(collection);
        Assert.Equal(1, collectionChangedCount);
    }

    [Fact]
    public void AddRange_MultipleItems_NotifiesOnce()
    {
        var collection = new ObservableCollection<int>();
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
            Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
        };
        
        collection.AddRange(new[] { 1, 2, 3, 4, 5 });
        
        Assert.Equal(5, collection.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, collection);
        Assert.Equal(1, collectionChangedCount); // Should only fire once
    }

    [Fact]
    public void RemoveRange_MultipleItems_NotifiesOnce()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3, 4, 5 });
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
            Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
        };
        
        var removed = collection.RemoveRange(new[] { 2, 4 });
        
        Assert.Equal(2, removed);
        Assert.Equal(3, collection.Count);
        Assert.Equal(new[] { 1, 3, 5 }, collection);
        Assert.Equal(1, collectionChangedCount);
    }

    [Fact]
    public void RemoveAll_WithPredicate_RemovesMatchingItems()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3, 4, 5 });
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
        };
        
        var removed = collection.RemoveAll(x => x % 2 == 0);
        
        Assert.Equal(2, removed);
        Assert.Equal(3, collection.Count);
        Assert.Equal(new[] { 1, 3, 5 }, collection);
        Assert.Equal(1, collectionChangedCount);
    }

    [Fact]
    public void Replace_ExistingItem_NotifiesChange()
    {
        var collection = new ObservableCollection<string>(new[] { "a", "b", "c" });
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
            Assert.Equal(NotifyCollectionChangedAction.Replace, e.Action);
            Assert.Equal(1, e.NewStartingIndex);
            Assert.Equal("x", e.NewItems![0]);
            Assert.Equal("b", e.OldItems![0]);
        };
        
        var replaced = collection.Replace(1, "x");
        
        Assert.Equal("b", replaced);
        Assert.Equal(new[] { "a", "x", "c" }, collection);
        Assert.Equal(1, collectionChangedCount);
    }

    [Fact]
    public void Move_ValidIndices_MovesItem()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3, 4 });
        var notifications = new List<NotifyCollectionChangedEventArgs>();
        
        collection.CollectionChanged += (s, e) => notifications.Add(e);
        
        collection.Move(3, 0);
        
        Assert.Equal(new[] { 4, 1, 2, 3 }, collection);
        Assert.Equal(2, notifications.Count); // Remove and Insert
    }

    [Fact]
    public void SuspendNotifications_BatchOperations_NotifiesOnce()
    {
        var collection = new ObservableCollection<int>();
        var collectionChangedCount = 0;
        
        collection.CollectionChanged += (s, e) =>
        {
            collectionChangedCount++;
        };
        
        using (collection.SuspendNotifications())
        {
            collection.Add(1);
            collection.Add(2);
            collection.Add(3);
            collection.Remove(2);
            collection.Add(4);
        }
        
        Assert.Equal(new[] { 1, 3, 4 }, collection);
        Assert.Equal(1, collectionChangedCount); // Should only fire once after suspension ends
    }

    [Fact]
    public void ObservableProperty_Integration_TracksAsProperty()
    {
        var collection = new ObservableCollection<int>();
        var observer = new TestPropertyObserver();
        
        collection.AddObserver(observer);
        collection.Add(42);
        
        Assert.True(observer.WasNotified);
        Assert.Equal("Items", observer.LastPropertyName);
    }

    [Fact]
    public void ComputedProperty_Integration_UpdatesOnCollectionChange()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var sum = new ComputedProperty<int>(() => 
        {
            var total = 0;
            foreach (var item in collection)
            {
                total += item;
            }
            return total;
        });
        
        Assert.Equal(6, sum.Value);
        
        collection.Add(4);
        Assert.Equal(10, sum.Value);
        
        collection.Remove(2);
        Assert.Equal(8, sum.Value);
        
        collection.Clear();
        Assert.Equal(0, sum.Value);
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        var collection = new ObservableCollection<int>(new[] { 1, 2, 3 });
        var notificationCount = 0;
        
        collection.CollectionChanged += (s, e) => notificationCount++;
        
        collection.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => collection.Add(4));
        Assert.Equal(0, notificationCount);
    }

    [Fact]
#pragma warning disable xUnit1031 // Test methods should not use blocking task operations
    public void ConcurrentAccess_ThreadSafe()
    {
        var collection = new ObservableCollection<int>();
        var tasks = new List<Task>();
        
        // Add items from multiple threads
        for (int i = 0; i < 10; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    collection.Add(taskId * 100 + j);
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());  // This is a test scenario demonstrating thread safety
        
        Assert.Equal(1000, collection.Count);
    }
#pragma warning restore xUnit1031

    private class TestPropertyObserver : IPropertyObserver
    {
        public bool WasNotified { get; private set; }
        public string? LastPropertyName { get; private set; }
        
        public void OnPropertyChanged(IObservableProperty property, Andy.TUI.Core.Observable.PropertyChangedEventArgs args)
        {
            WasNotified = true;
            LastPropertyName = args.PropertyName;
        }
    }
}