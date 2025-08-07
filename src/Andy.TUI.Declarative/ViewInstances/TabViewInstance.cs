using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Core;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.Core.Rendering;
using static Andy.TUI.Core.VirtualDom.VirtualDomBuilder;

namespace Andy.TUI.Declarative.ViewInstances;

/// <summary>
/// Runtime instance of a TabView that manages tab selection and z-index ordering.
/// </summary>
public class TabViewInstance : ViewInstance, IFocusable
{
    private TabView _tabView;
    private readonly List<ViewInstance> _headerInstances = new();
    private readonly List<ViewInstance> _contentInstances = new();
    private int _selectedIndex;
    private bool _hasFocus;
    
    // Z-index constants for tab layering
    private const int UnselectedTabZIndex = 1;
    private const int SelectedTabZIndex = 2;
    private const int TabContentZIndex = 3;
    
    public TabViewInstance(TabView tabView) : base($"TabView_{Guid.NewGuid():N}")
    {
        _tabView = tabView ?? throw new ArgumentNullException(nameof(tabView));
        _selectedIndex = Math.Clamp(tabView.SelectedIndex, 0, Math.Max(0, tabView.Tabs.Count - 1));
        
        // Note: Child instances will be created during OnUpdate
    }
    
    #region Tab Selection
    
    public int SelectedIndex => _selectedIndex;
    
    public void SelectTab(int index)
    {
        if (index < 0 || index >= _tabView.Tabs.Count)
            return;
            
        if (index == _selectedIndex)
            return;
            
        var previousIndex = _selectedIndex;
        _selectedIndex = index;
        
        // Update z-indices for affected tabs
        UpdateTabZIndices(previousIndex, index);
        
        // Trigger callback
        _tabView.GetOnTabSelected()?.Invoke(index);
        
        // Mark for re-render
        InvalidateView();
    }
    
    private void UpdateTabZIndices(int oldIndex, int newIndex)
    {
        // Update relative z-indices for tab headers
        if (oldIndex < _headerInstances.Count)
        {
            _headerInstances[oldIndex].RelativeZIndex = UnselectedTabZIndex;
        }
        
        if (newIndex < _headerInstances.Count)
        {
            _headerInstances[newIndex].RelativeZIndex = SelectedTabZIndex;
        }
        
        // Content z-index is handled during render
    }
    
    #endregion
    
    #region ViewInstance Implementation
    
    protected override void OnUpdate(ISimpleComponent viewDeclaration)
    {
        if (viewDeclaration is not TabView tabView)
            throw new ArgumentException("Expected TabView declaration");
            
        _tabView = tabView;
        _selectedIndex = Math.Clamp(tabView.SelectedIndex, 0, Math.Max(0, tabView.Tabs.Count - 1));
        
        // Update child instances
        var manager = Context?.ViewInstanceManager;
        if (manager != null)
        {
            _headerInstances.Clear();
            _contentInstances.Clear();
            
            for (int i = 0; i < tabView.Tabs.Count; i++)
            {
                var tab = tabView.Tabs[i];
                
                // Create header instance
                var headerPath = $"{Id}/header/{i}";
                var headerInstance = manager.GetOrCreateInstance(tab.Header, headerPath);
                _headerInstances.Add(headerInstance);
                
                // Create content instance
                var contentPath = $"{Id}/content/{i}";
                var contentInstance = manager.GetOrCreateInstance(tab.Content, contentPath);
                _contentInstances.Add(contentInstance);
            }
        }
    }
    
    protected override VirtualNode RenderWithLayout(LayoutBox layout)
    {
        if (_tabView.Tabs.Count == 0)
            return Fragment();
            
        var elements = new List<VirtualNode>();
        
        // Calculate layout for headers
        var headerHeight = 3; // Fixed height for tab headers
        var headerY = 0;
        var contentY = headerHeight;
        
        // Available space for content
        var contentHeight = Math.Max(0, layout.ContentHeight - headerHeight);
        
        // Layout and render tab headers
        var headerX = 0;
        for (int i = 0; i < _headerInstances.Count; i++)
        {
            var header = _headerInstances[i];
            var tab = _tabView.Tabs[i];
            var isSelected = i == _selectedIndex;
            
            // Calculate header width - fixed width for now
            var headerWidth = 15; // Fixed width for tab headers
            
            // Arrange header
            var headerLayout = new LayoutBox
            {
                X = headerX,
                Y = headerY,
                Width = headerWidth,
                Height = headerHeight,
                AbsoluteX = layout.AbsoluteX + headerX,
                AbsoluteY = layout.AbsoluteY + headerY
            };
            
            // Layout will be calculated in PerformLayout
            
            // Create header wrapper with selection styling
            var headerContent = header.Render();
            // Create header wrapper with selection styling
            var headerNode = Fragment(headerContent);
            
            elements.Add(headerNode);
            headerX += headerWidth + 1; // 1 char spacing
        }
        
        // Layout and render selected content
        if (_selectedIndex < _contentInstances.Count)
        {
            var content = _contentInstances[_selectedIndex];
            
            // Arrange content
            var contentLayout = new LayoutBox
            {
                X = 0,
                Y = contentY,
                Width = layout.ContentWidth,
                Height = contentHeight,
                AbsoluteX = layout.AbsoluteX,
                AbsoluteY = layout.AbsoluteY + contentY
            };
            
            // Layout will be calculated in PerformLayout
            
            // Render content with appropriate z-index
            var contentNode = content.Render();
            if (contentNode is ElementNode elem)
            {
                var props = new Dictionary<string, object?>(elem.Props)
                {
                    ["z-index"] = AbsoluteZIndex + TabContentZIndex
                };
                contentNode = new ElementNode(elem.TagName, props, elem.Children.ToArray());
            }
            
            elements.Add(contentNode);
        }
        
        return Fragment(elements.ToArray());
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();
        
        // Fixed header height
        var headerHeight = 3;
        
        // Measure headers
        var headerWidth = 0f;
        foreach (var header in _headerInstances)
        {
            var headerConstraints = LayoutConstraints.Loose(20, headerHeight);
            header.CalculateLayout(headerConstraints);
            headerWidth += Math.Max(10, header.Layout.Width + 4) + 1; // Min width + spacing
        }
        
        // Measure selected content
        var contentHeight = 0f;
        if (_selectedIndex < _contentInstances.Count)
        {
            var contentConstraints = LayoutConstraints.Loose(
                constraints.MaxWidth,
                Math.Max(0, constraints.MaxHeight - headerHeight)
            );
            _contentInstances[_selectedIndex].CalculateLayout(contentConstraints);
            contentHeight = _contentInstances[_selectedIndex].Layout.Height;
        }
        
        layout.Width = Math.Max(headerWidth, constraints.MaxWidth);
        layout.Height = headerHeight + contentHeight;
        
        return layout;
    }
    
    protected override IEnumerable<ViewInstance> GetChildren()
    {
        // Return all child instances for z-index propagation
        return _headerInstances.Concat(_contentInstances);
    }
    
    #endregion
    
    #region IFocusable Implementation
    
    public bool CanFocus => _tabView.Tabs.Count > 0;
    public bool IsFocused => _hasFocus;
    
    public void OnGotFocus()
    {
        _hasFocus = true;
        InvalidateView();
    }
    
    public void OnLostFocus()
    {
        _hasFocus = false;
        InvalidateView();
    }
    
    public bool HandleKeyPress(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow:
                if (_selectedIndex > 0)
                {
                    SelectTab(_selectedIndex - 1);
                    return true;
                }
                break;
                
            case ConsoleKey.RightArrow:
                if (_selectedIndex < _tabView.Tabs.Count - 1)
                {
                    SelectTab(_selectedIndex + 1);
                    return true;
                }
                break;
                
            case ConsoleKey.Tab:
                // Let focus manager handle tab navigation
                return false;
        }
        
        // Pass key to selected content if it's focusable
        if (_selectedIndex < _contentInstances.Count)
        {
            var content = _contentInstances[_selectedIndex];
            if (content is IFocusable focusableContent && focusableContent.IsFocused)
            {
                return focusableContent.HandleKeyPress(key);
            }
        }
        
        return false;
    }
    
    #endregion
}