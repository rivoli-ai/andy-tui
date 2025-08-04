using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Core.VirtualDom;

namespace Andy.TUI.Examples.Input;

class UIComponentsShowcaseApp
{
    // State
    private bool _acceptTerms = false;
    private bool _enableNotifications = true;
    private bool _darkMode = false;
    private Optional<string> _selectedTheme = Optional<string>.None;
    private float _downloadProgress = 0f;
    private Timer? _progressTimer;
    
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        // Start progress animation
        _progressTimer = new Timer(_ => {
            _downloadProgress = (_downloadProgress + 5) % 101;
        }, null, 0, 200);
        
        renderer.Run(() => CreateUI());
        
        _progressTimer?.Dispose();
    }
    
    private ISimpleComponent CreateUI()
    {
        var themes = new[] { "Light", "Dark", "High Contrast", "Solarized" };
        
        return new VStack(spacing: 2) {
            new Text("UI Components Showcase").Bold().Color(Color.Cyan),
            new Text("Demonstrating Checkbox, RadioGroup, List, ProgressBar, and Spinner").Color(Color.DarkGray),
            new Newline(),
            
            new HStack(spacing: 4) {
                // Left column
                new VStack(spacing: 1) {
                    new Text("Checkboxes:").Bold().Color(Color.Yellow),
                    new Checkbox("Accept Terms & Conditions", 
                        new Binding<bool>(() => _acceptTerms, v => _acceptTerms = v)),
                    new Checkbox("Enable Notifications", 
                        new Binding<bool>(() => _enableNotifications, v => _enableNotifications = v),
                        checkedMark: "[✓]", uncheckedMark: "[×]"),
                    new Checkbox("Dark Mode", 
                        new Binding<bool>(() => _darkMode, v => _darkMode = v),
                        labelFirst: false),
                    
                    new Newline(),
                    new Text("Radio Group:").Bold().Color(Color.Yellow),
                    new RadioGroup<string>(
                        "Select Theme:",
                        themes,
                        new Binding<Optional<string>>(() => _selectedTheme, v => _selectedTheme = v),
                        selectedMark: "(●)",
                        unselectedMark: "( )"
                    ),
                    
                    new Newline(),
                    new Text($"Selected: {(_selectedTheme.HasValue ? _selectedTheme.Value : "None")}").Color(Color.Gray)
                },
                
                // Middle column
                new VStack(spacing: 1) {
                    new Text("Lists:").Bold().Color(Color.Yellow),
                    new List(
                        new ISimpleComponent[] {
                            new Text("Bullet list item 1"),
                            new Text("Bullet list item 2"),
                            new Text("Bullet list item 3").Color(Color.Green)
                        },
                        ListMarkerStyle.Bullet,
                        markerColor: Color.Cyan
                    ),
                    
                    new Newline(),
                    new List(
                        new ISimpleComponent[] {
                            new Text("First numbered item"),
                            new Text("Second numbered item"),
                            new Text("Third numbered item")
                        },
                        ListMarkerStyle.Number,
                        markerColor: Color.Magenta,
                        spacing: 1
                    ),
                    
                    new Newline(),
                    new List(
                        new ISimpleComponent[] {
                            new Text("Arrow item").Bold(),
                            new Text("Another arrow item"),
                            new Box { new Text("Boxed item") }.WithPadding(1)
                        },
                        ListMarkerStyle.Arrow,
                        markerColor: Color.Yellow
                    )
                },
                
                // Right column
                new VStack(spacing: 1) {
                    new Text("Progress & Loading:").Bold().Color(Color.Yellow),
                    
                    new Text("Download Progress:"),
                    new ProgressBar(_downloadProgress, 
                        width: 25,
                        style: ProgressBarStyle.Solid,
                        filledColor: Color.Green,
                        label: "file.zip"),
                    
                    new Newline(),
                    new Text("Processing:"),
                    new ProgressBar(75f, 
                        width: 25,
                        style: ProgressBarStyle.Line,
                        filledColor: Color.Blue,
                        showPercentage: true),
                    
                    new Newline(),
                    new Text("Custom Style:"),
                    new ProgressBar(45f, 
                        width: 25,
                        style: ProgressBarStyle.Dots,
                        filledColor: Color.Magenta),
                    
                    new Newline(),
                    new Text("Spinners:").Bold(),
                    new HStack(spacing: 3) {
                        new Spinner(SpinnerStyle.Dots, color: Color.Cyan, label: "Loading"),
                        new Spinner(SpinnerStyle.Line, color: Color.Green),
                        new Spinner(SpinnerStyle.Arrow, color: Color.Yellow)
                    },
                    
                    new Newline(),
                    new HStack(spacing: 3) {
                        new Spinner(SpinnerStyle.Box, color: Color.Red, label: "Saving", labelFirst: true),
                        new Spinner(SpinnerStyle.Pulse, color: Color.Magenta)
                    }
                }
            },
            
            new Newline(),
            new Box {
                new VStack {
                    new Text("Horizontal Radio Group:").Bold(),
                    new RadioGroup<string>(
                        "",
                        new[] { "Small", "Medium", "Large", "X-Large" },
                        new Binding<Optional<string>>(() => Optional<string>.None, _ => {}),
                        vertical: false
                    )
                }
            }.WithPadding(1),
            
            new Newline(),
            new Text("Press Ctrl+C to exit...").Color(Color.DarkGray)
        };
    }
}