using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Core;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Focus;
using Andy.TUI.Core.Rendering;
using Andy.TUI.Core.Diagnostics;
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
    private readonly ILogger _logger;
    
    // Z-index constants for tab layering
    private const int UnselectedTabZIndex = 1;
    private const int SelectedTabZIndex = 2;
    private const int TabContentZIndex = 3;
    
    public TabViewInstance(TabView tabView) : base($"TabView_{Guid.NewGuid():N}")
    {
        _tabView = tabView ?? throw new ArgumentNullException(nameof(tabView));
        _selectedIndex = Math.Clamp(tabView.SelectedIndex, 0, Math.Max(0, tabView.Tabs.Count - 1));
        _logger = DebugContext.Logger.ForCategory("TabViewInstance");
        
        // Note: Child instances will be created during OnUpdate
        _logger.Debug("Created TabViewInstance with {0} tabs", tabView.Tabs.Count);
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
            
        var childElements = new List<VirtualNode>();
        
        // Render all tab headers
        for (int i = 0; i < _headerInstances.Count; i++)
        {
            var header = _headerInstances[i];
            
            // Update header's absolute position (calculated in PerformLayout)
            header.Layout.AbsoluteX = layout.AbsoluteX + (int)Math.Round(header.Layout.X);
            header.Layout.AbsoluteY = layout.AbsoluteY + (int)Math.Round(header.Layout.Y);
            
            // Update z-index based on selection
            header.RelativeZIndex = i == _selectedIndex ? SelectedTabZIndex : UnselectedTabZIndex;
            
            // Render header with its layout
            var headerNode = header.Render();
            childElements.Add(headerNode);
        }
        
        // Render selected content
        if (_selectedIndex < _contentInstances.Count)
        {
            var content = _contentInstances[_selectedIndex];
            
            // Update content's absolute position (calculated in PerformLayout)
            content.Layout.AbsoluteX = layout.AbsoluteX + (int)Math.Round(content.Layout.X);
            content.Layout.AbsoluteY = layout.AbsoluteY + (int)Math.Round(content.Layout.Y);
            
            // Set higher z-index for content
            content.RelativeZIndex = TabContentZIndex;
            
            // Render content with its layout
            var contentNode = content.Render();
            childElements.Add(contentNode);
        }
        
        return Fragment(childElements.ToArray());
    }
    
    protected override LayoutBox PerformLayout(LayoutConstraints constraints)
    {
        var layout = new LayoutBox();
        
        if (_tabView.Tabs.Count == 0)
        {
            layout.Width = 0;
            layout.Height = 0;
            return layout;
        }
        
        // Fixed header height
        var headerHeight = 3;
        var headerY = 0;
        var contentY = headerHeight;
        
        // Calculate available space for content
        var contentMaxHeight = Math.Max(0, constraints.MaxHeight - headerHeight);
        
        // First pass: measure and position headers
        var headerX = 0f;
        var totalHeaderWidth = 0f;
        
        for (int i = 0; i < _headerInstances.Count; i++)
        {
            var header = _headerInstances[i];
            var isSelected = i == _selectedIndex;
            
            // Create wrapper for header with selection indicator
            var headerConstraints = LayoutConstraints.Tight(15, headerHeight); // Fixed size headers
            header.CalculateLayout(headerConstraints);
            
            // Position header
            header.Layout.X = headerX;
            header.Layout.Y = headerY;
            
            headerX += header.Layout.Width + 1; // 1 char spacing between tabs
            totalHeaderWidth = headerX - 1; // Don't count last spacing
        }
        
        // Second pass: measure and position all content (but only selected will be rendered)
        var contentWidth = constraints.MaxWidth;
        var contentHeight = 0f;
        
        // Calculate layout for all content instances
        for (int i = 0; i < _contentInstances.Count; i++)
        {
            var content = _contentInstances[i];
            
            // Content gets full width and remaining height
            var contentConstraints = LayoutConstraints.Loose(contentWidth, contentMaxHeight);
            content.CalculateLayout(contentConstraints);
            
            // Position content below headers
            content.Layout.X = 0;
            content.Layout.Y = contentY;
            
            // Track height of selected content
            if (i == _selectedIndex)
            {
                contentHeight = content.Layout.Height;
            }
        }
        
        // Calculate final dimensions
        layout.Width = constraints.ConstrainWidth(Math.Max(totalHeaderWidth, contentWidth));
        layout.Height = constraints.ConstrainHeight(headerHeight + contentHeight);
        
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
    
    public bool CanReceiveFocus => _tabView.Tabs.Count > 0;
    
    public void OnGotFocus()
    {
        _logger.Debug("OnGotFocus called for TabView");
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
        _logger.Debug("HandleKeyPress called with key: {0}, hasFocus: {1}", key.Key, _hasFocus);
        
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow:
                if (_selectedIndex > 0)
                {
                    SelectTab(_selectedIndex - 1);
                    return true;
                }
                else if (_tabView.Tabs.Count > 0)
                {
                    // Wrap to last tab
                    SelectTab(_tabView.Tabs.Count - 1);
                    return true;
                }
                break;
                
            case ConsoleKey.RightArrow:
                if (_selectedIndex < _tabView.Tabs.Count - 1)
                {
                    SelectTab(_selectedIndex + 1);
                    return true;
                }
                else if (_tabView.Tabs.Count > 0)
                {
                    // Wrap to first tab
                    SelectTab(0);
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