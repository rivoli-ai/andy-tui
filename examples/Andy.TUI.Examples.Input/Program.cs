using System;
using System.Linq;
using Andy.TUI.Components;
using Andy.TUI.Components.Input;
using Andy.TUI.Components.Layout;
using Andy.TUI.Terminal;
using Microsoft.Extensions.DependencyInjection;

namespace Andy.TUI.Examples.Input;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var terminal = new AnsiTerminal();
            using var renderingSystem = new RenderingSystem(terminal);
            
            // Setup services
            var services = new ServiceCollection()
                .AddSingleton<IThemeProvider, ThemeProvider>()
                .AddSingleton<ISharedStateManager, SharedStateManager>()
                .BuildServiceProvider();
            
            var themeProvider = services.GetRequiredService<IThemeProvider>();
            var sharedStateManager = services.GetRequiredService<ISharedStateManager>();
            
            // Initialize rendering
            renderingSystem.Initialize();
            
            // Title
            renderingSystem.WriteText(2, 1, "Andy.TUI Input Components Demo", Style.Default.WithForegroundColor(Color.Cyan).WithBold());
            renderingSystem.WriteText(2, 2, "==============================", Style.Default.WithForegroundColor(Color.Cyan));
            
            // Create components
            var textInput = new TextInput
            {
                Placeholder = "Enter your name...",
                Width = 40,
                MaxLength = 50
            };
            var textInputContext = new ComponentContext(textInput, services, themeProvider, sharedStateManager);
            textInput.Initialize(textInputContext);
            textInput.IsFocused = true;
            
            var passwordInput = new TextInput
            {
                Placeholder = "Enter password...",
                Width = 40,
                PasswordChar = '*'
            };
            var passwordContext = new ComponentContext(passwordInput, services, themeProvider, sharedStateManager);
            passwordInput.Initialize(passwordContext);
            
            var countrySelect = new Select<string>
            {
                Placeholder = "Select a country...",
                Width = 40,
                AllowFiltering = true
            };
            countrySelect.Items = new[]
            {
                new SelectItem<string>("United States"),
                new SelectItem<string>("Canada"),
                new SelectItem<string>("United Kingdom"),
                new SelectItem<string>("Germany"),
                new SelectItem<string>("France")
            };
            var selectContext = new ComponentContext(countrySelect, services, themeProvider, sharedStateManager);
            countrySelect.Initialize(selectContext);
            
            var submitButton = new Button
            {
                Text = "Submit",
                Style = ButtonStyle.Primary,
                MinWidth = 12
            };
            var submitContext = new ComponentContext(submitButton, services, themeProvider, sharedStateManager);
            submitButton.Initialize(submitContext);
            
            var cancelButton = new Button
            {
                Text = "Cancel",
                Style = ButtonStyle.Default,
                MinWidth = 12
            };
            var cancelContext = new ComponentContext(cancelButton, services, themeProvider, sharedStateManager);
            cancelButton.Initialize(cancelContext);
            
            // Component positions
            var nameY = 5;
            var passwordY = 9;
            var countryY = 13;
            var buttonY = 17;
            
            // Arrange components
            textInput.Arrange(new Rectangle(12, nameY, 40, 3));
            passwordInput.Arrange(new Rectangle(12, passwordY, 40, 3));
            countrySelect.Arrange(new Rectangle(12, countryY, 40, 10)); // Extra height for dropdown
            submitButton.Arrange(new Rectangle(12, buttonY, 12, 3));
            cancelButton.Arrange(new Rectangle(27, buttonY, 12, 3));
            
            // Status message position
            var statusY = 21;
            var statusMessage = "";
            
            // Event handlers
            submitButton.Click += (s, e) =>
            {
                statusMessage = $"Submitted: Name={textInput.Text}, Password={'*' * passwordInput.Text.Length}, Country={countrySelect.SelectedItem ?? "None"}";
                RenderStatus();
            };
            
            cancelButton.Click += (s, e) =>
            {
                Environment.Exit(0);
            };
            
            // Instructions
            renderingSystem.WriteText(2, 24, "Tab/Shift+Tab: Navigate | Enter: Activate | Esc: Close dropdown | Ctrl+C: Exit", 
                Style.Default.WithForegroundColor(Color.Gray));
            
            // Focusable components
            var focusableComponents = new InputComponent[] 
            { 
                textInput, passwordInput, countrySelect, submitButton, cancelButton 
            };
            var focusIndex = 0;
            var firstRender = true;
            
            void UpdateFocus(int newIndex)
            {
                focusableComponents[focusIndex].IsFocused = false;
                focusIndex = newIndex;
                focusableComponents[focusIndex].IsFocused = true;
            }
            
            // Track previous dropdown state
            var wasDropdownOpen = false;
            var previousHighlightedIndex = -1;
            
            void RenderAll()
            {
                // Only clear and re-render static elements on first render
                if (firstRender)
                {
                    renderingSystem.Clear();
                    
                    // Render title
                    renderingSystem.WriteText(2, 1, "Andy.TUI Input Components Demo", Style.Default.WithForegroundColor(Color.Cyan).WithBold());
                    renderingSystem.WriteText(2, 2, "==============================", Style.Default.WithForegroundColor(Color.Cyan));
                    
                    // Labels - align with middle of input boxes
                    renderingSystem.WriteText(2, nameY + 1, "Name:", Style.Default.WithBold());
                    renderingSystem.WriteText(2, passwordY + 1, "Password:", Style.Default.WithBold());
                    renderingSystem.WriteText(2, countryY + 1, "Country:", Style.Default.WithBold());
                    
                    // Instructions
                    renderingSystem.WriteText(2, 24, "Tab/Shift+Tab: Navigate | Enter: Activate | Esc: Close dropdown | Ctrl+C: Exit",
                        Style.Default.WithForegroundColor(Color.Gray));
                    
                    firstRender = false;
                }
                
                // Clear dropdown area if it was open but now closed
                if (wasDropdownOpen && !countrySelect.IsOpen)
                {
                    var maxDropdownHeight = 7;
                    renderingSystem.FillRect(12, countryY + 2, 40, maxDropdownHeight, ' ', Style.Default);
                    wasDropdownOpen = false;
                    previousHighlightedIndex = -1;
                    
                    // Force re-render of buttons after clearing dropdown
                    // This ensures buttons are displayed correctly
                }
                
                // Render text input
                renderingSystem.DrawBox(12, nameY, 40, 3, 
                    textInput.IsFocused ? Style.Default.WithForegroundColor(Color.Cyan) : Style.Default,
                    BoxStyle.Single);
                var maxWidth = 38; // 40 - 2 for borders
                var displayText = string.IsNullOrEmpty(textInput.Text) ? textInput.Placeholder : textInput.Text;
                var scrollOffset = Math.Max(0, textInput.CursorPosition - maxWidth + 1);
                
                // Apply scrolling to the display text
                if (!string.IsNullOrEmpty(textInput.Text) && scrollOffset > 0)
                {
                    displayText = textInput.Text.Substring(scrollOffset);
                }
                
                if (displayText.Length > maxWidth)
                    displayText = displayText.Substring(0, maxWidth);
                    
                renderingSystem.WriteText(13, nameY + 1, 
                    displayText,
                    string.IsNullOrEmpty(textInput.Text) ? Style.Default.WithForegroundColor(Color.DarkGray) : Style.Default);
                
                // Show cursor for focused text input
                if (textInput.IsFocused && !string.IsNullOrEmpty(textInput.Text))
                {
                    var cursorX = 13 + (textInput.CursorPosition - scrollOffset);
                    if (cursorX >= 13 && cursorX < 13 + maxWidth)
                    {
                        renderingSystem.WriteText(cursorX, nameY + 1, "|", Style.Default.WithForegroundColor(Color.Cyan));
                    }
                }
                else if (textInput.IsFocused && string.IsNullOrEmpty(textInput.Text))
                {
                    // Show cursor at start when empty
                    renderingSystem.WriteText(13, nameY + 1, "|", Style.Default.WithForegroundColor(Color.Cyan));
                }
                
                // Render password input
                renderingSystem.DrawBox(12, passwordY, 40, 3,
                    passwordInput.IsFocused ? Style.Default.WithForegroundColor(Color.Cyan) : Style.Default,
                    BoxStyle.Single);
                var passwordDisplay = string.IsNullOrEmpty(passwordInput.Text) 
                    ? passwordInput.Placeholder 
                    : new string('•', passwordInput.Text.Length);
                if (passwordDisplay.Length > maxWidth)
                    passwordDisplay = passwordDisplay.Substring(passwordDisplay.Length - maxWidth);
                renderingSystem.WriteText(13, passwordY + 1,
                    passwordDisplay,
                    string.IsNullOrEmpty(passwordInput.Text) ? Style.Default.WithForegroundColor(Color.DarkGray) : Style.Default);
                
                // Render select box first (when closed)
                if (!countrySelect.IsOpen)
                {
                    renderingSystem.DrawBox(12, countryY, 40, 3,
                        countrySelect.IsFocused ? Style.Default.WithForegroundColor(Color.Cyan) : Style.Default,
                        BoxStyle.Single);
                    var selectDisplay = countrySelect.SelectedItem ?? countrySelect.Placeholder;
                    renderingSystem.WriteText(13, countryY + 1,
                        selectDisplay,
                        countrySelect.SelectedItem == null ? Style.Default.WithForegroundColor(Color.DarkGray) : Style.Default);
                    renderingSystem.WriteText(50, countryY + 1, "▼", Style.Default);
                }
                
                // Render buttons
                if (submitButton.IsFocused)
                {
                    // Focused button - white text on blue background
                    renderingSystem.DrawBox(12, buttonY, 12, 3, 
                        Style.Default.WithForegroundColor(Color.White).WithBackgroundColor(Color.Blue),
                        BoxStyle.Double);
                    renderingSystem.FillRect(13, buttonY + 1, 10, 1, ' ', Style.Default.WithBackgroundColor(Color.Blue));
                    renderingSystem.WriteText(15, buttonY + 1, "Submit", Style.Default.WithForegroundColor(Color.White));
                }
                else
                {
                    // Unfocused button - blue text on default background
                    renderingSystem.DrawBox(12, buttonY, 12, 3, 
                        Style.Default.WithForegroundColor(Color.Blue),
                        BoxStyle.Single);
                    renderingSystem.WriteText(15, buttonY + 1, "Submit", Style.Default.WithForegroundColor(Color.Blue));
                }
                
                if (cancelButton.IsFocused)
                {
                    // Focused button - white text on gray background
                    renderingSystem.DrawBox(27, buttonY, 12, 3, 
                        Style.Default.WithForegroundColor(Color.White).WithBackgroundColor(Color.DarkGray),
                        BoxStyle.Double);
                    renderingSystem.FillRect(28, buttonY + 1, 10, 1, ' ', Style.Default.WithBackgroundColor(Color.DarkGray));
                    renderingSystem.WriteText(30, buttonY + 1, "Cancel", Style.Default.WithForegroundColor(Color.White));
                }
                else
                {
                    // Unfocused button - default text on default background
                    renderingSystem.DrawBox(27, buttonY, 12, 3, 
                        Style.Default,
                        BoxStyle.Single);
                    renderingSystem.WriteText(30, buttonY + 1, "Cancel", Style.Default);
                }
                
                // Render open dropdown
                if (countrySelect.IsOpen)
                {
                    // Render open dropdown
                    renderingSystem.DrawBox(12, countryY, 40, 3,
                        Style.Default.WithForegroundColor(Color.Cyan),
                        BoxStyle.Single);
                    renderingSystem.WriteText(13, countryY + 1, countrySelect.Placeholder,
                        Style.Default.WithForegroundColor(Color.DarkGray));
                    renderingSystem.WriteText(50, countryY + 1, "▲", Style.Default);
                    
                    var items = countrySelect.Items.ToList();
                    var dropdownHeight = Math.Min(items.Count + 2, 7);
                    
                    // Fill the dropdown area with solid background
                    for (int i = 0; i < dropdownHeight; i++)
                    {
                        renderingSystem.FillRect(12, countryY + 2 + i, 40, 1, ' ', Style.Default.WithBackgroundColor(Color.Black));
                    }
                    
                    // Draw dropdown box
                    renderingSystem.DrawBox(12, countryY + 2, 40, dropdownHeight,
                        Style.Default.WithBackgroundColor(Color.Black),
                        BoxStyle.Single);
                    
                    // Only re-render dropdown items if highlight changed or first time
                    if (!wasDropdownOpen || previousHighlightedIndex != countrySelect.HighlightedIndex)
                    {
                        for (int i = 0; i < Math.Min(items.Count, 5); i++)
                        {
                            var isHighlighted = i == countrySelect.HighlightedIndex;
                            var itemStyle = isHighlighted 
                                ? Style.Default.WithBackgroundColor(Color.DarkBlue).WithForegroundColor(Color.White)
                                : Style.Default.WithBackgroundColor(Color.Black).WithForegroundColor(Color.White);
                            renderingSystem.FillRect(13, countryY + 3 + i, 38, 1, ' ', itemStyle);
                            renderingSystem.WriteText(13, countryY + 3 + i, items[i].Value, itemStyle);
                        }
                        previousHighlightedIndex = countrySelect.HighlightedIndex;
                    }
                    
                    wasDropdownOpen = true;
                }
                
                // Status message
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    renderingSystem.WriteText(2, statusY, statusMessage,
                        Style.Default.WithForegroundColor(Color.Green));
                }
            }
            
            void RenderStatus()
            {
                renderingSystem.FillRect(2, statusY, 78, 1, ' ');
                renderingSystem.WriteText(2, statusY, statusMessage,
                    Style.Default.WithForegroundColor(Color.Green));
            }
            
            // Input handling
            var inputHandler = new ConsoleInputHandler();
            
            inputHandler.KeyPressed += (sender, args) =>
            {
                if (args.Key == ConsoleKey.C && args.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    Environment.Exit(0);
                }
                else if (args.Key == ConsoleKey.Tab)
                {
                    // Close dropdown if open
                    if (countrySelect.IsOpen)
                    {
                        countrySelect.IsOpen = false;
                    }
                    
                    // Navigate focus
                    if (args.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        UpdateFocus((focusIndex - 1 + focusableComponents.Length) % focusableComponents.Length);
                    }
                    else
                    {
                        UpdateFocus((focusIndex + 1) % focusableComponents.Length);
                    }
                    RenderAll();
                }
                else
                {
                    // Let focused component handle input
                    var handled = focusableComponents[focusIndex].HandleKeyPress(args);
                    if (handled)
                    {
                        RenderAll();
                    }
                }
            };
            
            inputHandler.Start();
            
            // Initial render
            RenderAll();
            
            // Keep running
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}