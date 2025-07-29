using Andy.TUI.Core.Components;
using Xunit;

namespace Andy.TUI.Core.Tests.Components;

public class SharedStateManagerTests
{
    [Fact]
    public void SetValue_StoresValue()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        manager.SetValue("key", "value");
        
        // Assert
        var result = manager.GetValue<string>("key");
        Assert.Equal("value", result);
    }
    
    [Fact]
    public void SetValue_FiresValueChangedEvent()
    {
        // Arrange
        var manager = new SharedStateManager();
        SharedStateChangedEventArgs? eventArgs = null;
        manager.ValueChanged += (_, args) => eventArgs = args;
        
        // Act
        manager.SetValue("key", "value");
        
        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal("key", eventArgs.Key);
        Assert.Null(eventArgs.OldValue);
        Assert.Equal("value", eventArgs.NewValue);
    }
    
    [Fact]
    public void SetValue_DoesNotFireEventIfValueUnchanged()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", "value");
        
        var eventFired = false;
        manager.ValueChanged += (_, _) => eventFired = true;
        
        // Act
        manager.SetValue("key", "value"); // Same value
        
        // Assert
        Assert.False(eventFired);
    }
    
    [Fact]
    public void SetValue_FiresEventWithOldValue()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", "oldValue");
        
        SharedStateChangedEventArgs? eventArgs = null;
        manager.ValueChanged += (_, args) => eventArgs = args;
        
        // Act
        manager.SetValue("key", "newValue");
        
        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal("key", eventArgs.Key);
        Assert.Equal("oldValue", eventArgs.OldValue);
        Assert.Equal("newValue", eventArgs.NewValue);
    }
    
    [Fact]
    public void GetValue_ReturnsStoredValue()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", "value");
        
        // Act
        var result = manager.GetValue<string>("key");
        
        // Assert
        Assert.Equal("value", result);
    }
    
    [Fact]
    public void GetValue_ReturnsDefaultIfNotFound()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.GetValue<string>("nonexistent");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void GetValue_ReturnsDefaultIfWrongType()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", 42);
        
        // Act
        var result = manager.GetValue<string>("key");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void TryGetValue_ReturnsTrueIfValueExists()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", "value");
        
        // Act
        var result = manager.TryGetValue<string>("key", out var value);
        
        // Assert
        Assert.True(result);
        Assert.Equal("value", value);
    }
    
    [Fact]
    public void TryGetValue_ReturnsFalseIfValueNotFound()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.TryGetValue<string>("nonexistent", out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void TryGetValue_ReturnsFalseIfWrongType()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", 42);
        
        // Act
        var result = manager.TryGetValue<string>("key", out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void RemoveValue_RemovesValueAndFiresEvent()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key", "value");
        
        SharedStateChangedEventArgs? eventArgs = null;
        manager.ValueChanged += (_, args) => eventArgs = args;
        
        // Act
        var result = manager.RemoveValue("key");
        
        // Assert
        Assert.True(result);
        Assert.Null(manager.GetValue<string>("key"));
        Assert.NotNull(eventArgs);
        Assert.Equal("key", eventArgs.Key);
        Assert.Equal("value", eventArgs.OldValue);
        Assert.Null(eventArgs.NewValue);
    }
    
    [Fact]
    public void RemoveValue_ReturnsFalseIfKeyNotFound()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.RemoveValue("nonexistent");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void Clear_RemovesAllValuesAndFiresEvents()
    {
        // Arrange
        var manager = new SharedStateManager();
        manager.SetValue("key1", "value1");
        manager.SetValue("key2", "value2");
        
        var eventCount = 0;
        manager.ValueChanged += (_, _) => eventCount++;
        
        // Act
        manager.Clear();
        
        // Assert
        Assert.Null(manager.GetValue<string>("key1"));
        Assert.Null(manager.GetValue<string>("key2"));
        Assert.Equal(2, eventCount); // One event per cleared key
    }
    
    [Fact]
    public void SetValue_HandlesNullKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => manager.SetValue(null!, "value"));
    }
    
    [Fact]
    public void SetValue_HandlesEmptyKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => manager.SetValue("", "value"));
    }
    
    [Fact]
    public void GetValue_HandlesNullKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.GetValue<string>(null!);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void GetValue_HandlesEmptyKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.GetValue<string>("");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void TryGetValue_HandlesNullKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.TryGetValue<string>(null!, out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void TryGetValue_HandlesEmptyKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.TryGetValue<string>("", out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }
    
    [Fact]
    public void RemoveValue_HandlesNullKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.RemoveValue(null!);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void RemoveValue_HandlesEmptyKey()
    {
        // Arrange
        var manager = new SharedStateManager();
        
        // Act
        var result = manager.RemoveValue("");
        
        // Assert
        Assert.False(result);
    }
}