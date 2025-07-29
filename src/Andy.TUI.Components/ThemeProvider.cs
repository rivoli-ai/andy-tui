using System.Collections.Concurrent;

namespace Andy.TUI.Components;

/// <summary>
/// Default implementation of IThemeProvider that provides access to theme resources and styling information.
/// </summary>
public class ThemeProvider : IThemeProvider
{
    private readonly ConcurrentDictionary<string, object?> _colors = new();
    private readonly ConcurrentDictionary<string, object?> _styles = new();
    private readonly ConcurrentDictionary<string, object?> _resources = new();
    
    /// <summary>
    /// Gets the current theme name.
    /// </summary>
    public string CurrentTheme { get; private set; } = "Default";
    
    /// <summary>
    /// Initializes a new instance of the ThemeProvider class with default theme values.
    /// </summary>
    public ThemeProvider()
    {
        LoadDefaultTheme();
    }
    
    /// <summary>
    /// Initializes a new instance of the ThemeProvider class with a specific theme.
    /// </summary>
    /// <param name="themeName">The name of the theme to load.</param>
    public ThemeProvider(string themeName)
    {
        CurrentTheme = themeName ?? "Default";
        LoadDefaultTheme();
    }
    
    /// <summary>
    /// Gets a color value from the current theme.
    /// </summary>
    /// <param name="key">The color key.</param>
    /// <returns>The color value, or null if not found.</returns>
    public object? GetColor(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        
        _colors.TryGetValue(key, out var color);
        return color;
    }
    
    /// <summary>
    /// Gets a style value from the current theme.
    /// </summary>
    /// <param name="key">The style key.</param>
    /// <returns>The style value, or null if not found.</returns>
    public object? GetStyle(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;
        
        _styles.TryGetValue(key, out var style);
        return style;
    }
    
    /// <summary>
    /// Gets a theme resource of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <returns>The resource value, or default(T) if not found.</returns>
    public T? GetResource<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            return default(T);
        
        if (_resources.TryGetValue(key, out var resource) && resource is T typedResource)
            return typedResource;
        
        return default(T);
    }
    
    /// <summary>
    /// Sets a color value in the theme.
    /// </summary>
    /// <param name="key">The color key.</param>
    /// <param name="value">The color value.</param>
    public void SetColor(string key, object? value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _colors[key] = value;
        }
    }
    
    /// <summary>
    /// Sets a style value in the theme.
    /// </summary>
    /// <param name="key">The style key.</param>
    /// <param name="value">The style value.</param>
    public void SetStyle(string key, object? value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _styles[key] = value;
        }
    }
    
    /// <summary>
    /// Sets a resource value in the theme.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="key">The resource key.</param>
    /// <param name="value">The resource value.</param>
    public void SetResource<T>(string key, T value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _resources[key] = value;
        }
    }
    
    /// <summary>
    /// Switches to a different theme.
    /// </summary>
    /// <param name="themeName">The name of the theme to switch to.</param>
    public void SwitchTheme(string themeName)
    {
        if (string.IsNullOrEmpty(themeName))
            return;
        
        CurrentTheme = themeName;
        LoadTheme(themeName);
    }
    
    /// <summary>
    /// Loads the default theme with basic color and style definitions.
    /// </summary>
    private void LoadDefaultTheme()
    {
        // Default colors
        SetColor("Primary", "#007ACC");
        SetColor("Secondary", "#6C757D");
        SetColor("Success", "#28A745");
        SetColor("Danger", "#DC3545");
        SetColor("Warning", "#FFC107");
        SetColor("Info", "#17A2B8");
        SetColor("Light", "#F8F9FA");
        SetColor("Dark", "#343A40");
        SetColor("White", "#FFFFFF");
        SetColor("Black", "#000000");
        
        // Default foreground/background colors
        SetColor("Foreground", "#FFFFFF");
        SetColor("Background", "#000000");
        SetColor("AccentForeground", "#007ACC");
        SetColor("AccentBackground", "#1E1E1E");
        
        // Default styles
        SetStyle("BorderStyle", "Single");
        SetStyle("PaddingSize", 1);
        SetStyle("MarginSize", 0);
        SetStyle("BorderRadius", 0);
        
        // Default resources
        SetResource("FontSize", 12);
        SetResource("LineHeight", 1.2);
        SetResource("AnimationDuration", TimeSpan.FromMilliseconds(250));
    }
    
    /// <summary>
    /// Loads a specific theme. Override to implement custom theme loading logic.
    /// </summary>
    /// <param name="themeName">The name of the theme to load.</param>
    protected virtual void LoadTheme(string themeName)
    {
        // Base implementation loads default theme
        // Override in derived classes to load specific themes
        LoadDefaultTheme();
    }
}