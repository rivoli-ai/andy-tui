using System.Drawing;

namespace Andy.TUI.Theming;

public abstract class ThemeBase : ITheme
{
    private readonly Dictionary<string, ColorScheme> _colorSchemes = new();

    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract ColorScheme Default { get; }
    public abstract ColorScheme Primary { get; }
    public abstract ColorScheme Secondary { get; }
    public abstract ColorScheme Success { get; }
    public abstract ColorScheme Warning { get; }
    public abstract ColorScheme Error { get; }
    public abstract ColorScheme Info { get; }
    public abstract ColorScheme Disabled { get; }

    public abstract BorderStyle DefaultBorder { get; }
    public abstract BorderStyle FocusedBorder { get; }

    public virtual Spacing DefaultSpacing => Spacing.All(1);
    public virtual Spacing CompactSpacing => Spacing.All(0);
    public virtual Spacing RelaxedSpacing => Spacing.All(2);

    public virtual Typography DefaultTypography => new();
    public virtual Typography HeadingTypography => new(Bold: true);
    public virtual Typography CodeTypography => new();

    protected ThemeBase()
    {
        RegisterDefaultColorSchemes();
    }

    protected virtual void RegisterDefaultColorSchemes()
    {
        RegisterColorScheme("default", Default);
        RegisterColorScheme("primary", Primary);
        RegisterColorScheme("secondary", Secondary);
        RegisterColorScheme("success", Success);
        RegisterColorScheme("warning", Warning);
        RegisterColorScheme("error", Error);
        RegisterColorScheme("info", Info);
        RegisterColorScheme("disabled", Disabled);
    }

    protected void RegisterColorScheme(string key, ColorScheme colorScheme)
    {
        _colorSchemes[key.ToLowerInvariant()] = colorScheme;
    }

    public ColorScheme GetColorScheme(string key)
    {
        if (TryGetColorScheme(key, out var colorScheme))
            return colorScheme;

        throw new KeyNotFoundException($"Color scheme '{key}' not found in theme '{Name}'");
    }

    public bool TryGetColorScheme(string key, out ColorScheme colorScheme)
    {
        return _colorSchemes.TryGetValue(key.ToLowerInvariant(), out colorScheme!);
    }
}