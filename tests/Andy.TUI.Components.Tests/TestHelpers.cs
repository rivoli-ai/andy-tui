using Andy.TUI.Components;

namespace Andy.TUI.Components.Tests;

public static class TestHelpers
{
    public class MockComponentContext : IComponentContext
    {
        private readonly List<IComponentContext> _children = new();
        
        public string Id { get; } = Guid.NewGuid().ToString();
        public IComponent Component { get; }
        public IComponentContext? Parent { get; set; }
        public IReadOnlyCollection<IComponentContext> Children => _children.AsReadOnly();
        public IServiceProvider Services { get; } = new MockServiceProvider();
        public IThemeProvider Theme { get; } = new ThemeProvider();
        public ISharedStateManager SharedState { get; } = new SharedStateManager();
        
        public MockComponentContext(IComponent? component = null)
        {
            Component = component ?? new MockComponent();
        }
        
        public void AddChild(IComponentContext child)
        {
            _children.Add(child);
            if (child is MockComponentContext mockChild)
            {
                mockChild.Parent = this;
            }
        }
        
        public bool RemoveChild(IComponentContext child)
        {
            return _children.Remove(child);
        }
        
        public IComponentContext? FindChild(string componentId)
        {
            return _children.FirstOrDefault(c => c.Component.Id == componentId);
        }
        
        public T? GetService<T>() where T : class
        {
            return Services.GetService(typeof(T)) as T;
        }
        
        public T GetRequiredService<T>() where T : class
        {
            return GetService<T>() ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
        }
        
        public void SetSharedValue<T>(string key, T value)
        {
            SharedState.SetValue(key, value);
        }
        
        public T? GetSharedValue<T>(string key)
        {
            return SharedState.GetValue<T>(key);
        }
        
        public bool TryGetSharedValue<T>(string key, out T? value)
        {
            return SharedState.TryGetValue(key, out value);
        }
    }
    
    public class MockComponent : ComponentBase
    {
        protected override Core.VirtualDom.VirtualNode OnRender()
        {
            return new Core.VirtualDom.TextNode("Mock Component");
        }
    }
    
    public class MockServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();
        
        public MockServiceProvider()
        {
        }
        
        public void RegisterService<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }
        
        public object? GetService(Type serviceType)
        {
            return _services.TryGetValue(serviceType, out var service) ? service : null;
        }
    }
}