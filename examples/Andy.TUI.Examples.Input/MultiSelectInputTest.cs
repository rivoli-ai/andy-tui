using System;
using System.Collections.Generic;
using System.Linq;
using Andy.TUI.Terminal;
using Andy.TUI.Terminal.Rendering;
using Andy.TUI.Declarative.Rendering;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.State;
using Andy.TUI.Declarative.Extensions;
using Andy.TUI.Layout;
using Andy.TUI.VirtualDom;

namespace Andy.TUI.Examples.Input;

class MultiSelectInputTestApp
{
    private ISet<string> selectedLanguages = new HashSet<string> { "C#", "TypeScript" };
    private ISet<string> selectedColors = new HashSet<string>();
    private ISet<int> selectedNumbers = new HashSet<int>();
    
    public void Run()
    {
        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        var renderer = new DeclarativeRenderer(renderingSystem);
        
        renderingSystem.Initialize();
        
        renderer.Run(() => CreateUI());
    }
    
    private ISimpleComponent CreateUI()
    {
        var languages = new[] { "C#", "JavaScript", "Python", "Go", "Rust", "TypeScript", "Java", "C++" };
        var colors = new[] { "Red", "Green", "Blue", "Yellow", "Purple", "Orange" };
        var numbers = Enumerable.Range(1, 10).ToArray();
        
        return new VStack(spacing: 2) {
            new Text("MultiSelectInput Demo").Bold().Color(Color.Cyan),
            new Text("Use ↑/↓ to navigate, Space/Enter to toggle, Tab to switch between inputs").Color(Color.DarkGray),
            new Newline(),
            
            new HStack(spacing: 4) {
                new VStack {
                    new Text("Programming Languages:").Bold(),
                    new Box {
                        new MultiSelectInput<string>(
                            languages,
                            this.Bind(() => selectedLanguages)
                        )
                    }
                    .WithPadding(1),
                    
                    new Text($"Selected: {string.Join(", ", selectedLanguages)}").Color(Color.Gray)
                },
                
                new VStack {
                    
                    new Text("Favorite Colors:").Bold(),
                    new Box {
                        new MultiSelectInput<string>(
                            colors,
                            this.Bind(() => selectedColors),
                            checkedMark: "[✓]",
                            uncheckedMark: "[ ]"
                        )
                    }
                    .WithPadding(1),
                    
                    new Text($"Selected: {string.Join(", ", selectedColors)}").Color(Color.Gray)
                },
                
                new VStack {
                    new Text("Lucky Numbers:").Bold(),
                    new Box {
                        new MultiSelectInput<int>(
                            numbers,
                            this.Bind(() => selectedNumbers),
                            item => $"Number {item}"
                        )
                    }
                    .WithPadding(1),
                    
                    new Text($"Selected: {string.Join(", ", selectedNumbers)}").Color(Color.Gray)
                }
            },
            
            new Newline(),
            new Text("Real-world Example: Task Selection").Bold().Color(Color.Yellow),
            new Box {
                CreateTaskSelector()
            }
            .WithPadding(1),
            
            new Newline(),
            new Text("Press Ctrl+C to exit...").Color(Color.DarkGray)
        };
    }
    
    private ISet<TaskSelector.Task> selectedTasks = new HashSet<TaskSelector.Task>();
    
    private ISimpleComponent CreateTaskSelector()
    {
        var tasks = new[]
        {
            new TaskSelector.Task("Fix login bug", "High", "Open"),
            new TaskSelector.Task("Update documentation", "Medium", "Open"),
            new TaskSelector.Task("Refactor database layer", "Low", "Open"),
            new TaskSelector.Task("Add unit tests", "High", "In Progress"),
            new TaskSelector.Task("Deploy to staging", "Medium", "Blocked")
        };
        
        return new HStack(spacing: 2) {
            new MultiSelectInput<TaskSelector.Task>(
                tasks,
                this.Bind(() => selectedTasks),
                task => $"{task.Name} [{task.Priority}] - {task.Status}"
            ),
            new VStack {
                new Text("Selected Tasks:").Bold(),
                new Newline(),
                selectedTasks.Count == 0 
                    ? new Text("No tasks selected").Color(Color.DarkGray)
                    : new Text($"Selected: {string.Join(", ", selectedTasks.Select(t => t.Name))}")
            }
        };
    }
}

class TaskSelector
{
    public class Task
    {
        public string Name { get; }
        public string Priority { get; }
        public string Status { get; }
        
        public Task(string name, string priority, string status)
        {
            Name = name;
            Priority = priority;
            Status = status;
        }
        
        public override string ToString() => Name;
    }
    
    private static Color GetPriorityColor(string priority) => priority switch
    {
        "High" => Color.Red,
        "Medium" => Color.Yellow,
        "Low" => Color.Green,
        _ => Color.Gray
    };
}