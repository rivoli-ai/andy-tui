using System;
using Andy.TUI.VirtualDom;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative.State;
using Andy.TUI.Theming;
using FlexDirection = Andy.TUI.Layout.FlexDirection;
using LayoutSpacing = Andy.TUI.Layout.Spacing;

namespace Andy.TUI.Examples.Input;

/// <summary>
/// Demonstrates the theming system with dynamic theme switching in a form application
/// </summary>
class ThemedInputDemoApp
{
    private string name = "";
    private string email = "";
    private string password = "";
    private string selectedCountry = "";
    private string currentThemeName = "Light";
    private bool acceptTerms = false;

    private readonly string[] countries = { "United States", "Canada", "United Kingdom", "Germany", "France", "Japan", "Australia" };
    private readonly string[] themes = { "Light", "Dark", "HighContrast" };

    public void Run()
    {
        // Initialize with Light theme
        ThemeManager.Instance.SetTheme("light");

        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var input = new PollingInputHandler();
        var renderer = new DeclarativeRenderer(renderingSystem, input);

        // Listen for theme changes to trigger re-render
        ThemeManager.Instance.ThemeChanged += (sender, args) =>
        {
            currentThemeName = args.NewTheme.Name;
        };

        renderingSystem.Initialize();
        renderer.Run(() => CreateThemedUI());
    }

    private ISimpleComponent CreateThemedUI()
    {
        var theme = ThemeManager.Instance.CurrentTheme;

        // Using Box as the main container with vertical layout
        var container = new Box
        {
            FlexDirection = FlexDirection.Column,
            Gap = 1
        };

        // Theme selector at the top
        container.Add(CreateThemeSelector());

        container.Add(new Text("━".PadRight(60, '━'))
            .Color(GetThemedColor(theme.Default)));

        // Main form title
        container.Add(new Text("✨ Themed Input Components Demo")
            .Title()
            .Color(GetThemedColor(theme.Primary)));

        container.Add(new Text($"Current Theme: {currentThemeName}")
            .Color(GetThemedColor(theme.Info)));

        container.Add(" ");

        // Form section
        var formBox = new Box
        {
            Padding = 1  // Uses implicit conversion from int to Spacing
        };
        formBox.Add(CreateFormContent());
        container.Add(formBox);

        container.Add(" ");

        // Action buttons
        var buttonRow = new Box
        {
            FlexDirection = FlexDirection.Row,
            Gap = 2
        };

        buttonRow.Add(new Button("Submit", HandleSubmit).Primary());
        buttonRow.Add(new Button("Reset", HandleReset).Secondary());
        buttonRow.Add(new Button("Cancel", HandleCancel));

        container.Add(buttonRow);
        container.Add(" ");

        // Status section
        container.Add(CreateStatusSection());
        container.Add(" ");

        // Help text
        container.Add(new Text("Keyboard shortcuts:")
            .Color(GetThemedColor(theme.Info)));
        container.Add(new Text("  [Tab] - Navigate fields  |  [T] - Toggle theme  |  [Enter] - Submit")
            .Color(GetThemedColor(theme.Disabled)));
        container.Add(new Text("  [Ctrl+C] - Exit")
            .Color(GetThemedColor(theme.Disabled)));

        return container;
    }

    private ISimpleComponent CreateThemeSelector()
    {
        var theme = ThemeManager.Instance.CurrentTheme;

        var row = new Box
        {
            FlexDirection = FlexDirection.Row,
            Gap = 2
        };

        row.Add(new Text("Theme:")
            .Bold()
            .Color(GetThemedColor(theme.Default)));

        var buttonGroup = new Box
        {
            FlexDirection = FlexDirection.Row,
            Gap = 1
        };

        buttonGroup.Add(CreateThemeButton("Light", "light"));
        buttonGroup.Add(new Text("|").Color(GetThemedColor(theme.Disabled)));
        buttonGroup.Add(CreateThemeButton("Dark", "dark"));
        buttonGroup.Add(new Text("|").Color(GetThemedColor(theme.Disabled)));
        buttonGroup.Add(CreateThemeButton("High Contrast", "highcontrast"));

        row.Add(buttonGroup);
        return row;
    }

    private ISimpleComponent CreateThemeButton(string label, string themeKey)
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        var isActive = theme.Name.ToLower() == themeKey;

        var button = new Button(label, () =>
        {
            ThemeManager.Instance.SetTheme(themeKey);
        });

        // Use Primary/Secondary styling for active/inactive states
        if (isActive)
        {
            return button.Primary();
        }

        return button;
    }

    private ISimpleComponent CreateFormContent()
    {
        var theme = ThemeManager.Instance.CurrentTheme;

        var form = new Box
        {
            FlexDirection = FlexDirection.Column,
            Gap = 1
        };

        // Name field
        var nameRow = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        nameRow.Add(new Text("      Name:").Bold().Color(GetThemedColor(theme.Default)));
        nameRow.Add(new TextField("Enter your name...", this.Bind(() => name)));
        form.Add(nameRow);

        // Email field
        var emailRow = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        emailRow.Add(new Text("     Email:").Bold().Color(GetThemedColor(theme.Default)));
        emailRow.Add(new TextField("Enter your email...", this.Bind(() => email)));
        form.Add(emailRow);

        // Password field
        var passRow = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        passRow.Add(new Text("  Password:").Bold().Color(GetThemedColor(theme.Default)));
        passRow.Add(new TextField("Enter password...", this.Bind(() => password)).Secure());
        form.Add(passRow);

        // Country dropdown
        var countryRow = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        countryRow.Add(new Text("   Country:").Bold().Color(GetThemedColor(theme.Default)));
        countryRow.Add(new Dropdown<string>("Select a country...", countries, this.Bind(() => selectedCountry))
            .Color(GetThemedColor(theme.Default))
            .PlaceholderColor(GetThemedColor(theme.Disabled)));
        form.Add(countryRow);

        // Terms checkbox (simulated with text)
        var termsRow = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        termsRow.Add(new Text("     Terms:").Bold().Color(GetThemedColor(theme.Default)));
        termsRow.Add(new Text(acceptTerms ? "[✓] I accept the terms" : "[ ] I accept the terms")
            .Color(acceptTerms ? GetThemedColor(theme.Success) : GetThemedColor(theme.Default)));
        form.Add(termsRow);

        return form;
    }

    private ISimpleComponent CreateStatusSection()
    {
        var theme = ThemeManager.Instance.CurrentTheme;
        var hasData = !string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(email) ||
                      !string.IsNullOrEmpty(password) || !string.IsNullOrEmpty(selectedCountry);

        if (!hasData)
        {
            return new Text("No data entered yet")
                .Color(GetThemedColor(theme.Disabled));
        }

        var status = new Box
        {
            FlexDirection = FlexDirection.Column,
            Gap = 0
        };

        status.Add(new Text("Current Values:").Bold().Color(GetThemedColor(theme.Info)));

        var nameStatus = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        nameStatus.Add(new Text("  • Name:"));
        nameStatus.Add(new Text(string.IsNullOrEmpty(name) ? "<empty>" : name)
            .Color(string.IsNullOrEmpty(name) ? GetThemedColor(theme.Disabled) : GetThemedColor(theme.Success)));
        status.Add(nameStatus);

        var emailStatus = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        emailStatus.Add(new Text("  • Email:"));
        emailStatus.Add(new Text(string.IsNullOrEmpty(email) ? "<empty>" : email)
            .Color(string.IsNullOrEmpty(email) ? GetThemedColor(theme.Disabled) : GetThemedColor(theme.Success)));
        status.Add(emailStatus);

        var passStatus = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        passStatus.Add(new Text("  • Password:"));
        passStatus.Add(new Text(string.IsNullOrEmpty(password) ? "<empty>" : new string('•', password.Length))
            .Color(string.IsNullOrEmpty(password) ? GetThemedColor(theme.Disabled) : GetThemedColor(theme.Success)));
        status.Add(passStatus);

        var countryStatus = new Box { FlexDirection = FlexDirection.Row, Gap = 2 };
        countryStatus.Add(new Text("  • Country:"));
        countryStatus.Add(new Text(string.IsNullOrEmpty(selectedCountry) ? "<not selected>" : selectedCountry)
            .Color(string.IsNullOrEmpty(selectedCountry) ? GetThemedColor(theme.Disabled) : GetThemedColor(theme.Success)));
        status.Add(countryStatus);

        return status;
    }

    private Color GetThemedColor(ColorScheme scheme)
    {
        // Convert System.Drawing.Color to Andy.TUI.Terminal.Color
        return new Color(scheme.Foreground.R, scheme.Foreground.G, scheme.Foreground.B);
    }

    private Color GetThemedBackgroundColor(ColorScheme scheme)
    {
        // Convert System.Drawing.Color to Andy.TUI.Terminal.Color
        return new Color(scheme.Background.R, scheme.Background.G, scheme.Background.B);
    }

    private void HandleSubmit()
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(selectedCountry))
        {
            // In a real app, show validation error
            return;
        }

        Console.Clear();
        Console.WriteLine("=== Form Submitted ===");
        Console.WriteLine($"Theme: {currentThemeName}");
        Console.WriteLine($"Name: {name}");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Password: {new string('•', password.Length)}");
        Console.WriteLine($"Country: {selectedCountry}");
        Console.WriteLine($"Terms Accepted: {acceptTerms}");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
        Environment.Exit(0);
    }

    private void HandleReset()
    {
        name = "";
        email = "";
        password = "";
        selectedCountry = "";
        acceptTerms = false;
    }

    private void HandleCancel()
    {
        Environment.Exit(0);
    }
}

/// <summary>
/// Custom theme example that can be registered
/// </summary>
public class CorporateTheme : ThemeBase
{
    public override string Name => "Corporate";
    public override string Description => "Professional corporate branding theme";

    public override ColorScheme Default => new(
        Foreground: System.Drawing.Color.FromArgb(44, 62, 80),
        Background: System.Drawing.Color.FromArgb(250, 250, 250),
        BorderColor: System.Drawing.Color.FromArgb(189, 195, 199));

    public override ColorScheme Primary => new(
        Foreground: System.Drawing.Color.White,
        Background: System.Drawing.Color.FromArgb(52, 73, 94),
        BorderColor: System.Drawing.Color.FromArgb(44, 62, 80));

    public override ColorScheme Secondary => new(
        Foreground: System.Drawing.Color.White,
        Background: System.Drawing.Color.FromArgb(149, 165, 166),
        BorderColor: System.Drawing.Color.FromArgb(127, 140, 141));

    public override ColorScheme Success => new(
        Foreground: System.Drawing.Color.White,
        Background: System.Drawing.Color.FromArgb(46, 204, 113),
        BorderColor: System.Drawing.Color.FromArgb(39, 174, 96));

    public override ColorScheme Warning => new(
        Foreground: System.Drawing.Color.FromArgb(44, 62, 80),
        Background: System.Drawing.Color.FromArgb(241, 196, 15),
        BorderColor: System.Drawing.Color.FromArgb(243, 156, 18));

    public override ColorScheme Error => new(
        Foreground: System.Drawing.Color.White,
        Background: System.Drawing.Color.FromArgb(231, 76, 60),
        BorderColor: System.Drawing.Color.FromArgb(192, 57, 43));

    public override ColorScheme Info => new(
        Foreground: System.Drawing.Color.White,
        Background: System.Drawing.Color.FromArgb(52, 152, 219),
        BorderColor: System.Drawing.Color.FromArgb(41, 128, 185));

    public override ColorScheme Disabled => new(
        Foreground: System.Drawing.Color.FromArgb(149, 165, 166),
        Background: System.Drawing.Color.FromArgb(236, 240, 241),
        BorderColor: System.Drawing.Color.FromArgb(189, 195, 199));

    public override BorderStyle DefaultBorder => new(
        Type: BorderType.Single,
        Color: System.Drawing.Color.FromArgb(189, 195, 199));

    public override BorderStyle FocusedBorder => new(
        Type: BorderType.Single,
        Color: System.Drawing.Color.FromArgb(52, 73, 94),
        Width: 2);
}