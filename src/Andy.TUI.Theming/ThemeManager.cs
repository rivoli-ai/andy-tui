namespace Andy.TUI.Theming;

public sealed class ThemeManager
{
    private static readonly Lazy<ThemeManager> _instance = new(() => new ThemeManager());
    private readonly Dictionary<string, ITheme> _themes = new();
    private ITheme _currentTheme;

    public static ThemeManager Instance => _instance.Value;

    public ITheme CurrentTheme
    {
        get => _currentTheme;
        set
        {
            _currentTheme = value ?? throw new ArgumentNullException(nameof(value));
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(_currentTheme));
        }
    }

    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    private ThemeManager()
    {
        RegisterBuiltInThemes();
        _currentTheme = _themes["light"];
    }

    private void RegisterBuiltInThemes()
    {
        RegisterTheme(new LightTheme());
        RegisterTheme(new DarkTheme());
        RegisterTheme(new HighContrastTheme());
    }

    public void RegisterTheme(ITheme theme)
    {
        if (theme == null)
            throw new ArgumentNullException(nameof(theme));

        _themes[theme.Name.ToLowerInvariant()] = theme;
    }

    public ITheme GetTheme(string name)
    {
        if (TryGetTheme(name, out var theme) && theme != null)
            return theme;

        throw new KeyNotFoundException($"Theme '{name}' not found");
    }

    public bool TryGetTheme(string name, out ITheme? theme)
    {
        return _themes.TryGetValue(name.ToLowerInvariant(), out theme!);
    }

    public IEnumerable<ITheme> GetAllThemes()
    {
        return _themes.Values;
    }

    public void SetTheme(string name)
    {
        CurrentTheme = GetTheme(name);
    }

    public bool TrySetTheme(string name)
    {
        if (TryGetTheme(name, out var theme) && theme != null)
        {
            CurrentTheme = theme;
            return true;
        }

        return false;
    }
}

public class ThemeChangedEventArgs : EventArgs
{
    public ITheme NewTheme { get; }

    public ThemeChangedEventArgs(ITheme newTheme)
    {
        NewTheme = newTheme;
    }
}