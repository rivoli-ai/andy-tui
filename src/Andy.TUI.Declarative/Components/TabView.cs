using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Declarative.Components;

/// <summary>
/// Represents a single tab in a TabView.
/// </summary>
public class Tab
{
    /// <summary>
    /// Gets the header text or component for the tab.
    /// </summary>
    public ISimpleComponent Header { get; }

    /// <summary>
    /// Gets the content component for the tab.
    /// </summary>
    public ISimpleComponent Content { get; }

    /// <summary>
    /// Gets the unique key for this tab.
    /// </summary>
    public string Key { get; }

    public Tab(string headerText, ISimpleComponent content)
        : this(new Text(headerText), content, headerText)
    {
    }

    public Tab(ISimpleComponent header, ISimpleComponent content, string? key = null)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Key = key ?? Guid.NewGuid().ToString();
    }
}

/// <summary>
/// A component that displays multiple tabs with switchable content.
/// Manages z-index for proper layering of selected vs unselected tabs.
/// </summary>
public class TabView : ISimpleComponent
{
    private readonly List<Tab> _tabs = new();
    private readonly int _selectedIndex;
    private readonly Action<int>? _onTabSelected;

    /// <summary>
    /// Gets the tabs in this TabView.
    /// </summary>
    public IReadOnlyList<Tab> Tabs => _tabs;

    /// <summary>
    /// Gets the currently selected tab index.
    /// </summary>
    public int SelectedIndex => _selectedIndex;

    /// <summary>
    /// Creates a new TabView with the specified selected index.
    /// </summary>
    /// <param name="selectedIndex">The initially selected tab index.</param>
    /// <param name="onTabSelected">Optional callback when a tab is selected.</param>
    public TabView(int selectedIndex = 0, Action<int>? onTabSelected = null)
    {
        _selectedIndex = selectedIndex;
        _onTabSelected = onTabSelected;
    }

    /// <summary>
    /// DO NOT CALL DIRECTLY. Use collection initializer syntax or constructor parameters.
    /// This method is only for collection initializer support.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Add(Tab tab)
    {
        if (tab == null)
            throw new ArgumentNullException(nameof(tab));

        _tabs.Add(tab);
    }

    /// <summary>
    /// DO NOT CALL DIRECTLY. Use collection initializer syntax or constructor parameters.
    /// This method is only for collection initializer support.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Add(string headerText, ISimpleComponent content)
    {
        Add(new Tab(headerText, content));
    }

    public VirtualNode Render()
    {
        // TabView should be rendered via TabViewInstance
        throw new InvalidOperationException(
            "TabView should not be rendered directly. Use ViewInstanceManager.");
    }

    // Internal methods for view instance
    internal Action<int>? GetOnTabSelected() => _onTabSelected;
}