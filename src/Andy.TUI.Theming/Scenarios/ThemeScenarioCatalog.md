# Theme Scenario Catalog

This catalog provides examples of common theming scenarios and how to implement them using the Andy.TUI.Theming system.

## 1. Basic Theme Application

### Scenario: Apply default theme to an element
```csharp
using Andy.TUI.Theming;
using Andy.TUI.Terminal.Elements;

var element = new ElementNode();
element.WithTheme(); // Uses current theme's default color scheme
```

### Scenario: Apply specific color scheme
```csharp
var button = new ElementNode();
button.WithTheme("primary"); // Apply primary color scheme
button.WithSuccessTheme(); // Apply success color scheme
```

## 2. Dynamic Theme Switching

### Scenario: Switch between light and dark themes
```csharp
var manager = ThemeManager.Instance;

// Switch to dark theme
manager.SetTheme("dark");

// Listen for theme changes
manager.ThemeChanged += (sender, args) =>
{
    Console.WriteLine($"Theme changed to: {args.NewTheme.Name}");
    RefreshUI(); // Your UI refresh logic
};

// Toggle between themes
var currentTheme = manager.CurrentTheme.Name;
manager.SetTheme(currentTheme == "Light" ? "dark" : "light");
```

## 3. Custom Theme Creation

### Scenario: Create a brand-specific theme
```csharp
public class BrandTheme : ThemeBase
{
    public override string Name => "Brand";
    public override string Description => "Company brand theme";
    
    public override ColorScheme Default => new(
        Foreground: Color.FromArgb(51, 51, 51),
        Background: Color.FromArgb(250, 250, 250),
        BorderColor: Color.FromArgb(204, 204, 204));
    
    public override ColorScheme Primary => new(
        Foreground: Color.White,
        Background: Color.FromArgb(138, 43, 226), // Brand purple
        BorderColor: Color.FromArgb(123, 31, 204),
        AccentColor: Color.FromArgb(155, 89, 182));
    
    // ... other required color schemes
    
    protected override void RegisterDefaultColorSchemes()
    {
        base.RegisterDefaultColorSchemes();
        
        // Add custom color schemes
        RegisterColorScheme("brand-gradient", new(
            Foreground: Color.White,
            Background: Color.FromArgb(138, 43, 226)));
        
        RegisterColorScheme("brand-subtle", new(
            Foreground: Color.FromArgb(138, 43, 226),
            Background: Color.FromArgb(243, 229, 255)));
    }
}

// Register and use the custom theme
ThemeManager.Instance.RegisterTheme(new BrandTheme());
ThemeManager.Instance.SetTheme("brand");
```

## 4. Contextual Theme Application

### Scenario: Apply different themes based on element state
```csharp
public class ThemedButton
{
    private ElementNode _element;
    private bool _isHovered;
    private bool _isFocused;
    private bool _isDisabled;
    
    public void UpdateTheme()
    {
        if (_isDisabled)
        {
            _element.WithDisabledTheme();
        }
        else if (_isFocused)
        {
            _element.WithFocusedTheme();
        }
        else if (_isHovered)
        {
            _element.WithColorScheme("accent");
        }
        else
        {
            _element.WithPrimaryTheme();
        }
    }
}
```

## 5. Typography Styling

### Scenario: Apply different text styles
```csharp
// Heading
var heading = new ElementNode();
heading.WithHeadingTypography(); // Bold text

// Code block
var codeBlock = new ElementNode();
codeBlock.WithCodeTypography();

// Custom typography
var customText = new ElementNode();
customText.WithTypography(new Typography(
    Bold: true,
    Italic: true,
    Underline: false));
```

## 6. Accessibility Theme

### Scenario: Ensure high contrast for visually impaired users
```csharp
// Check user preference
if (UserPreferencesService.RequiresHighContrast)
{
    ThemeManager.Instance.SetTheme("highcontrast");
}

// Apply high contrast to specific elements
var criticalButton = new ElementNode();
var hcTheme = ThemeManager.Instance.GetTheme("highcontrast");
criticalButton.WithTheme(hcTheme, "error"); // High contrast error styling
```

## 7. Theme Persistence

### Scenario: Save and restore user theme preference
```csharp
public class ThemePreferences
{
    private const string THEME_KEY = "user_theme";
    
    public void SaveThemePreference(string themeName)
    {
        // Save to user settings/config
        UserSettings.Set(THEME_KEY, themeName);
    }
    
    public void RestoreThemePreference()
    {
        var savedTheme = UserSettings.Get(THEME_KEY, "light");
        
        if (ThemeManager.Instance.TrySetTheme(savedTheme))
        {
            Console.WriteLine($"Restored theme: {savedTheme}");
        }
        else
        {
            // Fallback to default
            ThemeManager.Instance.SetTheme("light");
        }
    }
}
```

## 8. Conditional Theme Application

### Scenario: Apply themes based on time of day
```csharp
public class AutoThemeSelector
{
    public void ApplyTimeBasedTheme()
    {
        var hour = DateTime.Now.Hour;
        
        if (hour >= 6 && hour < 18)
        {
            ThemeManager.Instance.SetTheme("light");
        }
        else
        {
            ThemeManager.Instance.SetTheme("dark");
        }
    }
    
    public void ApplySeasonalTheme()
    {
        var month = DateTime.Now.Month;
        
        switch (month)
        {
            case 12:
            case 1:
            case 2:
                // Winter theme
                ApplyCustomColorScheme("winter");
                break;
            case 3:
            case 4:
            case 5:
                // Spring theme
                ApplyCustomColorScheme("spring");
                break;
            // ... etc
        }
    }
}
```

## 9. Component-Level Theming

### Scenario: Create consistently themed components
```csharp
public class ThemedCard
{
    private ElementNode _container;
    private ElementNode _header;
    private ElementNode _content;
    private ElementNode _footer;
    
    public void ApplyCardTheme(string variant = "default")
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        var applicator = new ThemeApplicator(theme);
        
        // Container styling
        applicator.ApplyToElement(_container, variant);
        applicator.ApplyBorderStyle(_container as IHasBorder);
        applicator.ApplySpacing(_container as IHasSpacing, theme.DefaultSpacing);
        
        // Header styling
        applicator.ApplyToElement(_header, "primary");
        applicator.ApplyTypography(_header, theme.HeadingTypography);
        
        // Content styling
        applicator.ApplyToElement(_content, variant);
        applicator.ApplySpacing(_content as IHasSpacing, theme.RelaxedSpacing);
        
        // Footer styling
        applicator.ApplyToElement(_footer, "muted");
    }
}
```

## 10. Theme Validation

### Scenario: Validate theme accessibility
```csharp
public class ThemeValidator
{
    public bool ValidateContrast(ColorScheme scheme)
    {
        var contrastRatio = CalculateContrastRatio(
            scheme.Foreground, 
            scheme.Background);
        
        // WCAG AA standard requires 4.5:1 for normal text
        return contrastRatio >= 4.5;
    }
    
    public void ValidateTheme(ITheme theme)
    {
        var issues = new List<string>();
        
        if (!ValidateContrast(theme.Default))
            issues.Add("Default color scheme has insufficient contrast");
        
        if (!ValidateContrast(theme.Error))
            issues.Add("Error color scheme has insufficient contrast");
        
        // Check all required schemes exist
        var required = new[] { "default", "primary", "error", "disabled" };
        foreach (var key in required)
        {
            if (!theme.TryGetColorScheme(key, out _))
                issues.Add($"Missing required color scheme: {key}");
        }
        
        if (issues.Any())
        {
            throw new InvalidOperationException(
                $"Theme validation failed:\n{string.Join("\n", issues)}");
        }
    }
    
    private double CalculateContrastRatio(Color fg, Color bg)
    {
        // Implement WCAG contrast ratio calculation
        // This is a simplified example
        var fgLuminance = GetRelativeLuminance(fg);
        var bgLuminance = GetRelativeLuminance(bg);
        
        var lighter = Math.Max(fgLuminance, bgLuminance);
        var darker = Math.Min(fgLuminance, bgLuminance);
        
        return (lighter + 0.05) / (darker + 0.05);
    }
    
    private double GetRelativeLuminance(Color color)
    {
        // Simplified luminance calculation
        return (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255;
    }
}
```

## Usage Tips

1. **Start with built-in themes**: Use Light, Dark, or HighContrast as a foundation
2. **Be consistent**: Use the same color scheme keys across your application
3. **Test accessibility**: Always verify contrast ratios meet WCAG standards
4. **Provide user choice**: Let users select their preferred theme
5. **Handle theme changes**: Update UI when themes change dynamically
6. **Use semantic names**: Choose color scheme names that describe purpose, not color
7. **Document custom themes**: Clearly describe the purpose of custom color schemes