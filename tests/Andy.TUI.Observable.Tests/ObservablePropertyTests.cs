using Andy.TUI.Core.Observable;

namespace Andy.TUI.Core.Tests.Observable;

public class ObservablePropertyTests
{
    [Fact]
    public void Constructor_SetsInitialValue()
    {
        // Arrange & Act
        var property = new ObservableProperty<int>(42);
        
        // Assert
        Assert.Equal(42, property.Value);
    }
    
    [Fact]
    public void Value_Set_UpdatesValue()
    {
        // Arrange
        var property = new ObservableProperty<string>("initial");
        
        // Act
        property.Value = "updated";
        
        // Assert
        Assert.Equal("updated", property.Value);
    }
    
    [Fact]
    public void Value_Set_NotifiesObservers()
    {
        // Arrange
        var property = new ObservableProperty<int>(10);
        var notificationCount = 0;
        PropertyChangedEventArgs<int>? lastArgs = null;
        
        property.ValueChanged += (sender, args) =>
        {
            notificationCount++;
            lastArgs = args;
        };
        
        // Act
        property.Value = 20;
        
        // Assert
        Assert.Equal(1, notificationCount);
        Assert.NotNull(lastArgs);
        Assert.Equal(10, lastArgs.OldValue);
        Assert.Equal(20, lastArgs.NewValue);
    }
    
    [Fact]
    public void Value_SetSameValue_DoesNotNotify()
    {
        // Arrange
        var property = new ObservableProperty<int>(10);
        var notificationCount = 0;
        
        property.ValueChanged += (sender, args) => notificationCount++;
        
        // Act
        property.Value = 10; // Same value
        
        // Assert
        Assert.Equal(0, notificationCount);
    }
    
    [Fact]
    public void Subscribe_NotifiesOnChange()
    {
        // Arrange
        var property = new ObservableProperty<string>("initial");
        string? receivedValue = null;
        
        // Act
        using (property.Subscribe(value => receivedValue = value))
        {
            property.Value = "changed";
        }
        
        // Assert
        Assert.Equal("changed", receivedValue);
    }
    
    [Fact]
    public void Subscribe_Dispose_StopsNotifications()
    {
        // Arrange
        var property = new ObservableProperty<int>(0);
        var notificationCount = 0;
        
        // Act
        var subscription = property.Subscribe(value => notificationCount++);
        property.Value = 1;
        Assert.Equal(1, notificationCount);
        
        subscription.Dispose();
        property.Value = 2;
        
        // Assert
        Assert.Equal(1, notificationCount); // Should not increase
    }
    
    [Fact]
    public void Observe_CallsCallbackImmediately()
    {
        // Arrange
        var property = new ObservableProperty<int>(42);
        var values = new List<int>();
        
        // Act
        using (property.Observe(value => values.Add(value)))
        {
            // Should have received initial value
            Assert.Single(values);
            Assert.Equal(42, values[0]);
            
            property.Value = 100;
        }
        
        // Assert
        Assert.Equal(2, values.Count);
        Assert.Equal(100, values[1]);
    }
    
    [Fact]
    public void AddObserver_ReceivesNotifications()
    {
        // Arrange
        var property = new ObservableProperty<string>("initial");
        var observer = new TestObserver();
        
        // Act
        property.AddObserver(observer);
        property.Value = "changed";
        
        // Assert
        Assert.Equal(1, observer.NotificationCount);
        Assert.Equal("initial", observer.LastOldValue);
        Assert.Equal("changed", observer.LastNewValue);
    }
    
    [Fact]
    public void RemoveObserver_StopsNotifications()
    {
        // Arrange
        var property = new ObservableProperty<int>(0);
        var observer = new TestObserver();
        
        property.AddObserver(observer);
        property.Value = 1;
        Assert.Equal(1, observer.NotificationCount);
        
        // Act
        property.RemoveObserver(observer);
        property.Value = 2;
        
        // Assert
        Assert.Equal(1, observer.NotificationCount); // Should not increase
    }
    
    [Fact]
    public void SetValueSilently_DoesNotNotify()
    {
        // Arrange
        var property = new ObservableProperty<int>(10);
        var notified = false;
        property.ValueChanged += (_, _) => notified = true;
        
        // Act
        property.SetValueSilently(20);
        
        // Assert
        Assert.Equal(20, property.Value);
        Assert.False(notified);
    }
    
    [Fact]
    public void ForceNotify_NotifiesEvenWithoutChange()
    {
        // Arrange
        var property = new ObservableProperty<string>("value");
        var notificationCount = 0;
        property.ValueChanged += (_, _) => notificationCount++;
        
        // Act
        property.ForceNotify();
        
        // Assert
        Assert.Equal(1, notificationCount);
    }
    
    [Fact]
    public void HasObservers_ReturnsCorrectValue()
    {
        // Arrange
        var property = new ObservableProperty<int>(0);
        
        // Initially no observers
        Assert.False(property.HasObservers);
        
        // Add event handler
        EventHandler<PropertyChangedEventArgs<int>> handler = (_, _) => { };
        property.ValueChanged += handler;
        Assert.True(property.HasObservers);
        
        // Remove event handler
        property.ValueChanged -= handler;
        Assert.False(property.HasObservers);
        
        // Add callback
        var subscription = property.Subscribe(_ => { });
        Assert.True(property.HasObservers);
        
        // Remove callback
        subscription.Dispose();
        Assert.False(property.HasObservers);
    }
    
    [Fact]
    public void ImplicitConversion_ReturnsValue()
    {
        // Arrange
        var property = new ObservableProperty<int>(42);
        
        // Act
        int value = property;
        
        // Assert
        Assert.Equal(42, value);
    }
    
    [Fact]
    public void Create_StaticFactory_CreatesProperty()
    {
        // Act
        var property = ObservableProperty<string>.Create("test", "MyProperty");
        
        // Assert
        Assert.Equal("test", property.Value);
    }
    
    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var property = new ObservableProperty<int>(0);
        var notified = false;
        property.ValueChanged += (_, _) => notified = true;
        
        // Act
        property.Dispose();
        
        // Assert
        Assert.Throws<ObjectDisposedException>(() => property.Value);
        Assert.Throws<ObjectDisposedException>(() => property.Value = 1);
        Assert.False(notified);
    }
    
    [Fact]
    public void WeakReferences_AllowCallbacksToBeGarbageCollected()
    {
        // Arrange
        var property = new ObservableProperty<int>(0);
        var strongRef = new object();
        WeakReference weakRef;
        
        // Create callback in separate method to ensure it can be collected
        void SubscribeWithWeakReference()
        {
            var localObj = new object();
            weakRef = new WeakReference(localObj);
            property.Subscribe(_ => { var temp = localObj; });
        }
        
        SubscribeWithWeakReference();
        weakRef = null!; // Will be set by the method
        
        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        // Trigger cleanup by checking HasObservers
        var hasObservers = property.HasObservers;
        
        // Assert - weak reference should allow garbage collection
        // Note: This test might be flaky in some environments
        Assert.False(hasObservers);
    }
    
    [Fact]
    public void CustomComparer_UsedForEquality()
    {
        // Arrange
        var comparer = new CaseInsensitiveStringComparer();
        var property = new ObservableProperty<string>("test", "", comparer);
        var notified = false;
        property.ValueChanged += (_, _) => notified = true;
        
        // Act
        property.Value = "TEST"; // Different case but equal according to comparer
        
        // Assert
        Assert.False(notified); // Should not notify because values are equal
        Assert.Equal("test", property.Value); // Value should not change
    }
    
    [Fact]
    public async Task ThreadSafety_ConcurrentAccess()
    {
        // Arrange
        var property = new ObservableProperty<int>(0);
        var notificationCount = 0;
        var errors = new List<Exception>();
        
        property.ValueChanged += (_, _) => Interlocked.Increment(ref notificationCount);
        
        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var value = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        property.Value = value * 100 + j;
                        _ = property.Value;
                    }
                }
                catch (Exception ex)
                {
                    lock (errors)
                    {
                        errors.Add(ex);
                    }
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Assert
        Assert.Empty(errors);
        Assert.True(notificationCount > 0);
    }
    
    private class TestObserver : IPropertyObserver
    {
        public int NotificationCount { get; private set; }
        public object? LastOldValue { get; private set; }
        public object? LastNewValue { get; private set; }
        
        public void OnPropertyChanged(IObservableProperty property, PropertyChangedEventArgs args)
        {
            NotificationCount++;
            LastOldValue = args.OldValue;
            LastNewValue = args.NewValue;
        }
    }
    
    private class CaseInsensitiveStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }
        
        public int GetHashCode(string obj)
        {
            return obj?.ToUpperInvariant().GetHashCode() ?? 0;
        }
    }
}