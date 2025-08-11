using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.State;

namespace Andy.TUI.Examples.Input;

/// <summary>
/// Comprehensive showcase of the Andy.TUI Declarative framework features.
/// </summary>

class DeclarativeShowcaseApp
{
    private readonly AppState _state = new();

    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);

        renderingSystem.Initialize();
        terminal.Clear();

        // Main UI composition
        var ui = new VStack(spacing: 2) {
            // Header
            CreateHeader(),
            
            // Tab navigation
            CreateTabBar(),
            
            // Content area
            CreateContentArea(),
            
            // Footer
            CreateFooter()
        };

        // Run the declarative renderer
        var renderer = new DeclarativeRenderer(renderingSystem, this);
        renderer.Run(() => ui);
    }

    private ISimpleComponent CreateHeader()
    {
        return new Box {
            new HStack {
                new Text("Andy.TUI Declarative Showcase")
                    .Bold()
                    .Color(Color.Cyan),
                new Spacer(),
                new Text($"Tab: {_state.ActiveTab}")
                    .Color(Color.Gray)
            }
        }
        .WithPadding(1)
        .WithWidth(Length.Percentage(100));
    }

    private ISimpleComponent CreateTabBar()
    {
        return new HStack(spacing: 2) {
            CreateTabButton("Layout", 0),
            CreateTabButton("Forms", 1),
            CreateTabButton("Grid", 2),
            CreateTabButton("Animation", 3)
        };
    }

    private ISimpleComponent CreateTabButton(string label, int index)
    {
        var isActive = _state.ActiveTab == index;
        var button = new Button(label, () => _state.ActiveTab = index);
        return isActive ? button.Primary() : button.Secondary();
    }

    private ISimpleComponent CreateContentArea()
    {
        return new Box {
            _state.ActiveTab switch
            {
                0 => CreateLayoutDemo(),
                1 => CreateFormsDemo(),
                2 => CreateGridDemo(),
                3 => CreateAnimationDemo(),
                _ => new Text("Unknown tab")
            }
        }
        .WithPadding(2)
        .WithMinHeight(20);
    }

    private ISimpleComponent CreateLayoutDemo()
    {
        return new VStack(spacing: 2) {
            new Text("Layout System Demo").Bold(),
            
            // Flexbox demo
            new Text("Flexbox Layout:").Color(Color.Gray),
            new Box {
                new Text("Flex: 1").Color(Color.Green),
                new Text("Flex: 2").Color(Color.Yellow),
                new Text("Fixed").Color(Color.Red)
            }
            .Direction(FlexDirection.Row)
            .WithGap(2)
            .WithPadding(1),
            
            // Spacer demo
            new Text("Spacer Demo:").Color(Color.Gray),
            new HStack {
                new Text("Left"),
                new Spacer(),
                new Text("Center"),
                new Spacer(),
                new Text("Right")
            },
            
            // ZStack demo
            new Text("ZStack (Layered) Demo:").Color(Color.Gray),
            new ZStack {
                new Box { new Text(" ") }
                    .WithWidth(30)
                    .WithHeight(5)
                    .WithPadding(1),
                new Text("Overlaid Text")
                    .Bold()
                    .Color(Color.Yellow)
            },
            
            // Overflow demo
            new Text("Overflow Hidden:").Color(Color.Gray),
            new Box {
                new Text("This text is too long and will be clipped by the container")
            }
            .WithWidth(25)
            .WithHeight(1)
            .WithOverflow(Overflow.Hidden)
            .WithPadding(1)
        };
    }

    private ISimpleComponent CreateFormsDemo()
    {
        return new VStack(spacing: 2) {
            new Text("Forms Demo").Bold(),
            
            // Text input
            new VStack(spacing: 1) {
                new Text("Name:").Color(Color.Gray),
                new TextField("Enter your name", this.Bind(() => _state.Name))
            },
            
            // Dropdown
            new VStack(spacing: 1) {
                new Text("Country:").Color(Color.Gray),
                new Dropdown<string>(
                    "Select a country",
                    _state.Countries,
                    this.Bind(() => _state.SelectedCountry)
                )
            },
            
            // Checkbox-like toggle
            new HStack(spacing: 2) {
                new Button(
                    _state.AcceptTerms ? "[X]" : "[ ]",
                    () => _state.AcceptTerms = !_state.AcceptTerms
                ),
                new Text("Accept terms and conditions")
            },
            
            // Submit button
            new Spacer(minLength: 2),
            new HStack(spacing: 2) {
                new Button("Submit", () => HandleSubmit())
                    .Primary(),
                new Button("Clear", () => ClearForm())
            },
            
            // Form data display
            new Spacer(minLength: 2),
            new Text("Form Data:").Bold(),
            new Text($"Name: {_state.Name}").Color(Color.Gray),
            new Text($"Country: {(_state.SelectedCountry == "" ? "None" : _state.SelectedCountry)}").Color(Color.Gray),
            new Text($"Terms: {(_state.AcceptTerms ? "Accepted" : "Not accepted")}").Color(Color.Gray)
        };
    }

    private ISimpleComponent CreateGridDemo()
    {
        // Create basic grid
        var basicGrid = new Grid()
            .WithColumns(GridTrackSize.Fr(1), GridTrackSize.Fr(1), GridTrackSize.Fr(1))
            .WithRows(GridTrackSize.Auto, GridTrackSize.Auto, GridTrackSize.Auto)
            .WithGap(1);

        // Add cells to basic grid
        for (int i = 1; i <= 9; i++)
        {
            basicGrid.Add(CreateGridCell(i.ToString()));
        }

        // Create complex grid with spans
        var complexGrid = new Grid()
            .WithColumns(GridTrackSize.Pixels(10), GridTrackSize.Fr(2), GridTrackSize.Fr(1))
            .WithRows(GridTrackSize.Auto, GridTrackSize.Auto, GridTrackSize.Auto)
            .WithGap(1);

        // Add children to complex grid
        complexGrid.Add(new Text("Header - Full Width")
            .Bold()
            .Color(Color.Cyan)
            .GridArea(1, 1, columnSpan: 3));

        complexGrid.Add(new Box { new Text("Sidebar") }
            .WithPadding(1)
            .GridArea(2, 1, rowSpan: 2));

        complexGrid.Add(new Box { new Text("Main Content Area") }
            .WithPadding(2)
            .GridArea(2, 2));

        complexGrid.Add(new Box { new Text("Side") }
            .WithPadding(1)
            .GridArea(2, 3));

        complexGrid.Add(new Text("Footer")
            .Color(Color.Gray)
            .GridArea(3, 2, columnSpan: 2));

        return new VStack(spacing: 2) {
            new Text("Grid Layout Demo").Bold(),
            
            // Basic grid
            new Text("3x3 Grid:").Color(Color.Gray),
            basicGrid,
            
            // Complex grid with spans
            new Spacer(minLength: 2),
            new Text("Grid with Spanning:").Color(Color.Gray),
            complexGrid
        };
    }

    private ISimpleComponent CreateAnimationDemo()
    {
        return new VStack(spacing: 2) {
            new Text("Animation & State Demo").Bold(),
            
            // Counter demo
            new HStack(spacing: 2) {
                new Button("-", () => _state.Counter--)
                    .Secondary(),
                new Text($"Counter: {_state.Counter}")
                    .Bold()
                    .Color(Color.Yellow),
                new Button("+", () => _state.Counter++)
            },
            
            // Progress bar simulation
            new Spacer(minLength: 2),
            new Text($"Progress: {_state.Progress}%").Color(Color.Gray),
            new Box {
                new Box { new Text(" ") }
                    .WithWidth(Length.Percentage(_state.Progress))
                    .WithHeight(1)
            }
            .WithWidth(50)
            .WithHeight(1)
            .WithPadding(0),

            new HStack(spacing: 2) {
                new Button("Start", () => StartProgress()),
                new Button("Reset", () => _state.Progress = 0)
            },
            
            // Dynamic list
            new Spacer(minLength: 2),
            new Text("Dynamic List:").Bold(),
            CreateDynamicList(),
            new HStack(spacing: 2) {
                new TextField("Add item", this.Bind(() => _state.NewItem)),
                new Button("Add", () => AddItem())
            }
        };
    }

    private ISimpleComponent CreateGridCell(string content)
    {
        return new Box {
            new Text(content).Color(Color.Yellow)
        }
        .WithPadding(1)
        .Align(AlignItems.Center)
        .Justify(JustifyContent.Center);
    }

    private ISimpleComponent CreateFooter()
    {
        return new HStack {
            new Text("Tab: Navigate sections | ↑↓: Select | Enter: Activate | Esc: Exit")
                .Color(Color.DarkGray),
            new Spacer(),
            new Text("Andy.TUI v1.0")
                .Color(Color.DarkGray)
        };
    }

    private void HandleSubmit()
    {
        // Form submission logic
        Console.Beep();
    }

    private void ClearForm()
    {
        _state.Name = "";
        _state.SelectedCountry = "";
        _state.AcceptTerms = false;
    }

    private async void StartProgress()
    {
        for (int i = _state.Progress; i <= 100; i += 5)
        {
            _state.Progress = i;
            await Task.Delay(100);
        }
    }

    private void AddItem()
    {
        if (!string.IsNullOrWhiteSpace(_state.NewItem))
        {
            _state.Items.Add(_state.NewItem);
            _state.NewItem = "";
        }
    }

    private VStack CreateDynamicList()
    {
        var vstack = new VStack(spacing: 1);
        for (int i = 0; i < _state.Items.Count; i++)
        {
            var index = i; // Capture for closure
            var item = _state.Items[index];
            vstack.Add(new HStack {
                new Text($"• {item}"),
                new Spacer(),
                new Button("X", () => _state.Items.RemoveAt(index))
            });
        }
        return vstack;
    }
}

// Application state
class AppState
{
    private int _activeTab = 0;
    private string _name = "";
    private string _selectedCountry = "";
    private bool _acceptTerms = false;
    private int _counter = 0;
    private int _progress = 0;
    private string _newItem = "";

    public int ActiveTab
    {
        get => _activeTab;
        set { _activeTab = value; OnPropertyChanged?.Invoke(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged?.Invoke(); }
    }

    public string SelectedCountry
    {
        get => _selectedCountry;
        set { _selectedCountry = value; OnPropertyChanged?.Invoke(); }
    }

    public bool AcceptTerms
    {
        get => _acceptTerms;
        set { _acceptTerms = value; OnPropertyChanged?.Invoke(); }
    }

    public int Counter
    {
        get => _counter;
        set { _counter = value; OnPropertyChanged?.Invoke(); }
    }

    public int Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged?.Invoke(); }
    }

    public string NewItem
    {
        get => _newItem;
        set { _newItem = value; OnPropertyChanged?.Invoke(); }
    }

    public List<string> Countries { get; } = new()
    {
        "United States",
        "Canada",
        "United Kingdom",
        "Germany",
        "France",
        "Japan",
        "Australia"
    };

    public List<string> Items { get; } = new()
    {
        "First item",
        "Second item",
        "Third item"
    };

    public event Action? OnPropertyChanged;
}