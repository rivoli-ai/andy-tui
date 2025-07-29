using Andy.TUI.Components;
using Andy.TUI.Core.VirtualDom;
using Moq;
using Xunit;

namespace Andy.TUI.Components.Tests;

public class ComponentBaseTests
{
    private class TestComponent : ComponentBase
    {
        public string TestProperty { get; set; } = "";
        public bool RenderCalled { get; private set; }
        public bool InitializeCalled { get; private set; }
        public bool MountedCalled { get; private set; }
        public bool UnmountedCalled { get; private set; }
        public bool UpdateCalled { get; private set; }
        public bool ShouldUpdateCalled { get; private set; }
        public bool AfterRenderedCalled { get; private set; }
        public bool DisposeCalled { get; private set; }
        
        protected override void OnInitialize()
        {
            InitializeCalled = true;
            base.OnInitialize();
        }
        
        protected override void OnMounted()
        {
            MountedCalled = true;
            base.OnMounted();
        }
        
        protected override void OnUnmounted()
        {
            UnmountedCalled = true;
            base.OnUnmounted();
        }
        
        protected override VirtualNode OnRender()
        {
            RenderCalled = true;
            return new TextNode(TestProperty);
        }
        
        protected override void OnUpdate()
        {
            UpdateCalled = true;
            base.OnUpdate();
        }
        
        protected override bool OnShouldUpdate()
        {
            ShouldUpdateCalled = true;
            return base.OnShouldUpdate();
        }
        
        protected override void OnAfterRendered()
        {
            AfterRenderedCalled = true;
            base.OnAfterRendered();
        }
        
        protected override void OnDispose()
        {
            DisposeCalled = true;
            base.OnDispose();
        }
    }
    
    [Fact]
    public void Constructor_SetsUniqueId()
    {
        // Arrange & Act
        var component1 = new TestComponent();
        var component2 = new TestComponent();
        
        // Assert
        Assert.NotNull(component1.Id);
        Assert.NotNull(component2.Id);
        Assert.NotEqual(component1.Id, component2.Id);
    }
    
    [Fact]
    public void Initialize_SetsContextAndCallsOnInitialize()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        
        // Act
        component.Initialize(mockContext.Object);
        
        // Assert
        Assert.True(component.IsInitialized);
        Assert.Equal(mockContext.Object, component.Context);
        Assert.True(component.InitializeCalled);
    }
    
    [Fact]
    public void Initialize_ThrowsIfAlreadyInitialized()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => component.Initialize(mockContext.Object));
    }
    
    [Fact]
    public void OnMount_SetsIsMountedAndCallsOnMounted()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        
        // Act
        component.OnMount();
        
        // Assert
        Assert.True(component.IsMounted);
        Assert.True(component.MountedCalled);
    }
    
    [Fact]
    public void OnMount_ThrowsIfNotInitialized()
    {
        // Arrange
        var component = new TestComponent();
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => component.OnMount());
    }
    
    [Fact]
    public void OnUnmount_SetsIsMountedFalseAndCallsOnUnmounted()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        component.OnMount();
        
        // Act
        component.OnUnmount();
        
        // Assert
        Assert.False(component.IsMounted);
        Assert.True(component.UnmountedCalled);
    }
    
    [Fact]
    public void Render_CallsOnRenderAndOnAfterRender()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        component.TestProperty = "Test";
        
        // Act
        var result = component.Render();
        
        // Assert
        Assert.True(component.RenderCalled);
        Assert.True(component.AfterRenderedCalled);
        Assert.IsType<TextNode>(result);
        Assert.Equal("Test", ((TextNode)result).Content);
    }
    
    [Fact]
    public void Render_ThrowsIfNotInitialized()
    {
        // Arrange
        var component = new TestComponent();
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => component.Render());
    }
    
    [Fact]
    public void Update_CallsOnUpdate()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        
        // Act
        component.Update();
        
        // Assert
        Assert.True(component.UpdateCalled);
    }
    
    [Fact]
    public void ShouldUpdate_CallsOnShouldUpdate()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        
        // Act
        var result = component.ShouldUpdate();
        
        // Assert
        Assert.True(component.ShouldUpdateCalled);
        Assert.False(result); // Default implementation returns false
    }
    
    [Fact]
    public void RequestRender_FiresRenderRequestedEvent()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        
        var eventFired = false;
        component.RenderRequested += (_, _) => eventFired = true;
        
        // Act
        component.RequestRender();
        
        // Assert
        Assert.True(eventFired);
        Assert.True(component.ShouldUpdate()); // RequestRender should set update flag
    }
    
    [Fact]
    public void Dispose_CallsOnDisposeAndCleansUp()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        component.OnMount();
        
        // Act
        component.Dispose();
        
        // Assert
        Assert.True(component.DisposeCalled);
        Assert.True(component.UnmountedCalled);
        Assert.False(component.IsMounted);
    }
    
    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var component = new TestComponent();
        var mockContext = new Mock<IComponentContext>();
        component.Initialize(mockContext.Object);
        
        // Act & Assert (Should not throw)
        component.Dispose();
        component.Dispose();
    }
}