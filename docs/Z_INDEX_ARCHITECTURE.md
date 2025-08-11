# Z-Index Architecture for Hierarchical UI Components

## Problem Statement

Current z-index implementation is flat and doesn't handle hierarchical layering properly. When components like TabView change their internal z-ordering, or when they're nested inside modals, we need both:

1. **Relative Z-Index**: Component-local layering (tabs relative to each other)
2. **Absolute Z-Index**: Global layering for rendering (accounting for parent z-indices)

## Real-World Scenarios

### Scenario 1: TabView Component

```csharp
TabView {
    Tab("Home") { HomeContent() },     // Selected: relative z=2
    Tab("Settings") { Settings() },    // Unselected: relative z=1
    Tab("About") { About() }          // Unselected: relative z=1
}
```

When user selects "Settings" tab:
- Previous: Home z=2→1, Settings z=1→2
- Spatial index must update both elements
- Only Settings content area needs re-rendering

### Scenario 2: TabView Inside Modal

```csharp
Modal(zIndex: 1000) {
    TabView {
        Tab("Login") { ... },    // Absolute z=1002 when selected
        Tab("Register") { ... }  // Absolute z=1001 when unselected
    }
}

// Background content
Button("Help", zIndex: 50)      // Still rendered below modal tabs
```

### Scenario 3: Nested Modals with Tabs

```csharp
Modal(zIndex: 1000) {                    // Parent modal
    TabView {
        Tab("General") { 
            Modal(zIndex: 100) {         // Nested modal: absolute z=1100
                Text("Nested")           // Absolute z=1100
            }
        }
    }
}
```

## Proposed Z-Index Resolution System

### 1. Z-Index Context Stack

```csharp
public class ZIndexContext
{
    private readonly Stack<int> _zIndexStack = new();
    
    public int CurrentBase => _zIndexStack.Sum();
    
    public void PushContext(int relativeZIndex)
    {
        _zIndexStack.Push(relativeZIndex);
    }
    
    public void PopContext()
    {
        _zIndexStack.Pop();
    }
    
    public int ResolveAbsolute(int relativeZIndex)
    {
        return CurrentBase + relativeZIndex;
    }
}
```

### 2. Enhanced ViewInstance with Z-Context

```csharp
public abstract class ViewInstance
{
    // Existing properties...
    
    public int RelativeZIndex { get; set; }
    public int AbsoluteZIndex { get; private set; }
    
    public void UpdateAbsoluteZIndex(ZIndexContext context)
    {
        AbsoluteZIndex = context.ResolveAbsolute(RelativeZIndex);
        
        // Propagate to children
        context.PushContext(RelativeZIndex);
        foreach (var child in Children)
        {
            child.UpdateAbsoluteZIndex(context);
        }
        context.PopContext();
    }
}
```

### 3. TabView Implementation

```csharp
public class TabViewInstance : ViewInstance
{
    private int _selectedIndex = 0;
    private List<TabInstance> _tabs = new();
    
    public void SelectTab(int index)
    {
        if (index == _selectedIndex) return;
        
        var oldTab = _tabs[_selectedIndex];
        var newTab = _tabs[index];
        
        // Update relative z-indices
        oldTab.RelativeZIndex = 1;  // Background
        newTab.RelativeZIndex = 2;  // Foreground
        
        // Trigger spatial index updates
        var context = GetCurrentZIndexContext();
        oldTab.UpdateAbsoluteZIndex(context);
        newTab.UpdateAbsoluteZIndex(context);
        
        // Notify spatial index of z-order change
        SpatialIndex.UpdateZIndex(oldTab, oldTab.AbsoluteZIndex);
        SpatialIndex.UpdateZIndex(newTab, newTab.AbsoluteZIndex);
        
        _selectedIndex = index;
    }
}
```

### 4. Enhanced Spatial Index Operations

```csharp
public interface IZIndexAwareSpatialIndex<T>
{
    // Z-order change without position change
    void UpdateZIndex(T element, int oldZ, int newZ);
    
    // Efficient z-order queries
    IEnumerable<T> QueryByZRange(int minZ, int maxZ);
    IEnumerable<T> GetElementsAbove(T element);
    IEnumerable<T> GetElementsBelow(T element);
    
    // Tab-specific optimizations
    void SwapZOrder(T element1, T element2);
}
```

## Rendering Optimizations

### 1. Tab Switch Optimization

When switching tabs, only need to:
1. Clear the old tab content area (not the tab headers)
2. Render the new tab content
3. Update tab header visual states

```csharp
public void RenderTabSwitch(TabInstance oldTab, TabInstance newTab)
{
    // Only clear/render content areas, not entire TabView
    var oldContentBounds = oldTab.GetContentBounds();
    var newContentBounds = newTab.GetContentBounds();
    
    ClearRegion(oldContentBounds);
    RenderRegion(newContentBounds);
    
    // Update just the tab headers for selected state
    UpdateTabHeader(oldTab, selected: false);
    UpdateTabHeader(newTab, selected: true);
}
```

### 2. Z-Order Change Detection

```csharp
public class ZOrderChangeDetector
{
    public bool RequiresFullRender(ZIndexChange change)
    {
        // Tab switches within same container: partial render
        if (change.Type == ZIndexChangeType.TabSwitch)
            return false;
            
        // Modal appearing/disappearing: full render of affected area
        if (change.Type == ZIndexChangeType.ModalToggle)
            return true;
            
        // Focus change affecting z-order: depends on overlap
        return CheckOverlapWithOtherElements(change);
    }
}
```

## Implementation Priority

1. **Phase 1**: Relative/Absolute Z-Index Resolution
   - Implement ZIndexContext stack
   - Update ViewInstance with dual z-index tracking

2. **Phase 2**: TabView Component
   - Create TabView and Tab components
   - Implement tab switching with z-order updates
   - Optimize rendering for tab switches

3. **Phase 3**: Spatial Index Z-Order Support
   - Add UpdateZIndex operation
   - Implement z-order swapping for tabs
   - Add z-range queries

4. **Phase 4**: Complex Nested Scenarios
   - Test modal within tab within modal
   - Validate absolute z-index calculation
   - Performance optimization for deep nesting

## Performance Considerations

### Memory Impact
- Each ViewInstance adds 8 bytes (2 ints) for z-indices
- ZIndexContext stack: minimal overhead
- Spatial index z-order tracking: ~20% additional memory

### CPU Impact  
- Z-index resolution: O(depth) where depth = nesting level
- Tab switch: O(1) z-order update + partial render
- Spatial index z-update: O(log n) for rebalancing

### Optimization Opportunities
1. Cache absolute z-index until parent changes
2. Batch z-order updates for multiple tabs
3. Skip occlusion checks for same-z-level elements
4. Use z-index ranges for component groups

## Testing Scenarios

1. **Simple Tab Switch**: Verify only content area re-renders
2. **Modal Over Tabs**: Ensure tabs remain below modal despite z-changes  
3. **Nested Tab Views**: Parent tab switch doesn't affect child tab z-order
4. **Dynamic Z-Index**: Programmatic z-index changes update spatial index
5. **Performance**: 100+ tabs switching rapidly