using Andy.TUI.Components;
using Moq;
using Xunit;

namespace Andy.TUI.Components.Tests;

public class ComponentContextTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        
        // Act
        var context = new ComponentContext(
            mockComponent.Object,
            mockServices.Object,
            mockTheme.Object,
            mockSharedState.Object);
        
        // Assert
        Assert.NotNull(context.Id);
        Assert.Equal(mockComponent.Object, context.Component);
        Assert.Equal(mockServices.Object, context.Services);
        Assert.Equal(mockTheme.Object, context.Theme);
        Assert.Equal(mockSharedState.Object, context.SharedState);
        Assert.Null(context.Parent);
        Assert.Empty(context.Children);
    }
    
    [Fact]
    public void Constructor_WithParent_SetsParent()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var mockParent = new Mock<IComponentContext>();
        
        // Act
        var context = new ComponentContext(
            mockComponent.Object,
            mockServices.Object,
            mockTheme.Object,
            mockSharedState.Object,
            mockParent.Object);
        
        // Assert
        Assert.Equal(mockParent.Object, context.Parent);
    }
    
    [Fact]
    public void AddChild_AddsChildToCollection()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var mockChild = new Mock<IComponentContext>();
        
        // Act
        context.AddChild(mockChild.Object);
        
        // Assert
        Assert.Contains(mockChild.Object, context.Children);
    }
    
    [Fact]
    public void AddChild_DoesNotAddDuplicateChild()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var mockChild = new Mock<IComponentContext>();
        
        // Act
        context.AddChild(mockChild.Object);
        context.AddChild(mockChild.Object); // Add same child twice
        
        // Assert
        Assert.Single(context.Children);
        Assert.Contains(mockChild.Object, context.Children);
    }
    
    [Fact]
    public void RemoveChild_RemovesChildFromCollection()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var mockChild = new Mock<IComponentContext>();
        context.AddChild(mockChild.Object);
        
        // Act
        var result = context.RemoveChild(mockChild.Object);
        
        // Assert
        Assert.True(result);
        Assert.DoesNotContain(mockChild.Object, context.Children);
    }
    
    [Fact]
    public void RemoveChild_ReturnsFalseIfChildNotFound()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var mockChild = new Mock<IComponentContext>();
        
        // Act
        var result = context.RemoveChild(mockChild.Object);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void FindChild_ReturnsChildByComponentId()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var mockChildComponent = new Mock<IComponent>();
        mockChildComponent.Setup(c => c.Id).Returns("test-id");
        var mockChild = new Mock<IComponentContext>();
        mockChild.Setup(c => c.Component).Returns(mockChildComponent.Object);
        context.AddChild(mockChild.Object);
        
        // Act
        var result = context.FindChild("test-id");
        
        // Assert
        Assert.Equal(mockChild.Object, result);
    }
    
    [Fact]
    public void FindChild_ReturnsNullIfNotFound()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        // Act
        var result = context.FindChild("non-existent");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void GetService_ReturnsServiceFromProvider()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var testService = "TestService";
        mockServices.Setup(s => s.GetService(typeof(string))).Returns(testService);
        
        // Act
        var result = context.GetService<string>();
        
        // Assert
        Assert.Equal(testService, result);
    }
    
    [Fact]
    public void GetRequiredService_ReturnsServiceFromProvider()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        var testService = "TestService";
        mockServices.Setup(s => s.GetService(typeof(string))).Returns(testService);
        
        // Act
        var result = context.GetRequiredService<string>();
        
        // Assert
        Assert.Equal(testService, result);
    }
    
    [Fact]
    public void GetRequiredService_ThrowsIfServiceNotFound()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        mockServices.Setup(s => s.GetService(typeof(string))).Returns((object?)null);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => context.GetRequiredService<string>());
    }
    
    [Fact]
    public void SetSharedValue_CallsSharedStateManager()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        // Act
        context.SetSharedValue("key", "value");
        
        // Assert
        mockSharedState.Verify(s => s.SetValue("key", "value"), Times.Once);
    }
    
    [Fact]
    public void GetSharedValue_CallsSharedStateManager()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        mockSharedState.Setup(s => s.GetValue<string>("key")).Returns("value");
        
        // Act
        var result = context.GetSharedValue<string>("key");
        
        // Assert
        Assert.Equal("value", result);
        mockSharedState.Verify(s => s.GetValue<string>("key"), Times.Once);
    }
    
    [Fact]
    public void TryGetSharedValue_CallsSharedStateManager()
    {
        // Arrange
        var mockComponent = new Mock<IComponent>();
        var mockServices = new Mock<IServiceProvider>();
        var mockTheme = new Mock<IThemeProvider>();
        var mockSharedState = new Mock<ISharedStateManager>();
        var context = new ComponentContext(mockComponent.Object, mockServices.Object, mockTheme.Object, mockSharedState.Object);
        
        mockSharedState.Setup(s => s.TryGetValue("key", out It.Ref<string?>.IsAny)).Returns(true);
        
        // Act
        var result = context.TryGetSharedValue<string>("key", out var value);
        
        // Assert
        Assert.True(result);
        mockSharedState.Verify(s => s.TryGetValue<string>("key", out It.Ref<string?>.IsAny), Times.Once);
    }
}