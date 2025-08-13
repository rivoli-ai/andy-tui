using Xunit;
using Andy.TUI.Declarative.State;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Andy.TUI.Declarative.Tests.State;

public class ObservableTests
{
    [Fact]
    public void ObservableProperty_NotifiesOnChange()
    {
        var property = new ObservableProperty<string>("initial");
        var notified = false;
        
        property.PropertyChanged += (sender, e) =>
        {
            notified = true;
            Assert.Equal("Value", e.PropertyName);
        };
        
        property.Value = "updated";
        
        Assert.True(notified);
        Assert.Equal("updated", property.Value);
    }
    
    [Fact]
    public void ObservableProperty_DoesNotNotifyOnSameValue()
    {
        var property = new ObservableProperty<int>(42);
        var notifyCount = 0;
        
        property.PropertyChanged += (sender, e) => notifyCount++;
        
        property.Value = 42; // Same value
        Assert.Equal(0, notifyCount);
        
        property.Value = 43; // Different value
        Assert.Equal(1, notifyCount);
    }
    
    [Fact]
    public void ObservableList_NotifiesOnAdd()
    {
        var list = new ObservableList<string>();
        var collectionNotified = false;
        var propertyNotified = false;
        
        list.CollectionChanged += (sender, e) =>
        {
            collectionNotified = true;
            Assert.Equal(NotifyCollectionChangedAction.Add, e.Action);
            Assert.Equal("item", e.NewItems?[0]);
            Assert.Equal(0, e.NewStartingIndex);
        };
        
        list.PropertyChanged += (sender, e) =>
        {
            propertyNotified = true;
            Assert.Equal("Count", e.PropertyName);
        };
        
        list.Add("item");
        
        Assert.True(collectionNotified);
        Assert.True(propertyNotified);
        Assert.Single(list);
    }
    
    [Fact]
    public void ObservableList_NotifiesOnClear()
    {
        var list = new ObservableList<int> { 1, 2, 3 };
        var notified = false;
        
        list.CollectionChanged += (sender, e) =>
        {
            notified = true;
            Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
        };
        
        list.Clear();
        
        Assert.True(notified);
        Assert.Empty(list);
    }
    
    [Fact]
    public void ObservableList_NotifiesOnRemove()
    {
        var list = new ObservableList<string> { "a", "b", "c" };
        var notified = false;
        
        list.CollectionChanged += (sender, e) =>
        {
            notified = true;
            Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action);
            Assert.Equal("b", e.OldItems?[0]);
            Assert.Equal(1, e.OldStartingIndex);
        };
        
        list.Remove("b");
        
        Assert.True(notified);
        Assert.Equal(2, list.Count);
        Assert.Equal("a", list[0]);
        Assert.Equal("c", list[1]);
    }
}