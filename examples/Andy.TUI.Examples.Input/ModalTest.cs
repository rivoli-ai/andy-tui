using System;
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

class ModalTestApp
{
    private bool showAlertDialog = false;
    private bool showConfirmDialog = false;
    private bool showPromptDialog = false;
    private bool showCustomModal = false;
    private string inputValue = "";
    private string lastAction = "No action taken yet";
    private ModalSize modalSize = ModalSize.Medium;
    
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
        return new ZStack {
            // Main content
            new VStack(spacing: 2) {
                new Text("Modal/Dialog System Demo").Bold().Color(Color.Cyan),
                
                new Text("Basic Dialogs:").Bold(),
                new HStack(spacing: 2) {
                    new Button("Alert", () => showAlertDialog = true).Primary(),
                    new Button("Confirm", () => showConfirmDialog = true).Primary(),
                    new Button("Prompt", () => showPromptDialog = true).Primary()
                },
                
                new Text("Custom Modal:").Bold(),
                new HStack(spacing: 2) {
                    new Button("Small", () => { modalSize = ModalSize.Small; showCustomModal = true; }).Secondary(),
                    new Button("Medium", () => { modalSize = ModalSize.Medium; showCustomModal = true; }).Secondary(),
                    new Button("Large", () => { modalSize = ModalSize.Large; showCustomModal = true; }).Secondary(),
                    new Button("Full", () => { modalSize = ModalSize.FullScreen; showCustomModal = true; }).Secondary()
                },
                
                new Box {
                    new Text($"Last Action: {lastAction}").Color(Color.Yellow)
                }
                .WithPadding(1)
                .WithMargin(2),
                
                new Text("Instructions:").Bold().Color(Color.Gray),
                new Text("• Click buttons to open different modal types").Color(Color.DarkGray),
                new Text("• Press ESC to close modals").Color(Color.DarkGray),
                new Text("• Some modals can be closed by clicking backdrop").Color(Color.DarkGray),
                new Text("• Ctrl+C to exit").Color(Color.DarkGray)
            },
            
            // Modals (rendered on top when visible)
            Dialog.Alert(
                "Alert Dialog",
                "This is an alert message. It can only be closed by clicking OK or pressing ESC.",
                this.Bind(() => showAlertDialog),
                "Got it!"
            ),
            
            Dialog.Confirm(
                "Confirm Action",
                "Are you sure you want to proceed with this action?",
                this.Bind(() => showConfirmDialog),
                () => lastAction = "User confirmed the action",
                "Yes, proceed",
                "Cancel"
            ),
            
            Dialog.Prompt(
                "Enter Information",
                "Please enter your name:",
                this.Bind(() => showPromptDialog),
                this.Bind(() => inputValue),
                (value) => lastAction = $"User entered: '{value}'",
                "Type your name...",
                "Submit",
                "Cancel"
            ),
            
            // Custom modal with rich content
            new Modal(
                "Custom Modal",
                CreateCustomModalContent(),
                this.Bind(() => showCustomModal)
            )
            .Size(modalSize)
        };
    }
    
    private ISimpleComponent CreateCustomModalContent()
    {
        return new VStack(spacing: 2) {
            new Text("This is a custom modal with rich content").Bold(),
            
            new Box {
                new VStack(spacing: 1) {
                    new Text("Features:").Bold().Color(Color.Cyan),
                    new Text("• Customizable size (Small/Medium/Large/FullScreen)"),
                    new Text("• Can contain any component"),
                    new Text("• Supports nested layouts"),
                    new Text("• Full keyboard navigation")
                }
            }
            .WithPadding(1),
            
            new HStack(spacing: 1) {
                new Text("Modal Size: ").Bold(),
                new Text(modalSize.ToString()).Color(Color.Green)
            },
            
            new Table<SampleData>(
                new[] {
                    new SampleData { Name = "Item 1", Value = 100 },
                    new SampleData { Name = "Item 2", Value = 200 },
                    new SampleData { Name = "Item 3", Value = 300 }
                },
                new[] {
                    new TableColumn<SampleData>("Name", d => d.Name),
                    new TableColumn<SampleData>("Value", d => d.Value.ToString())
                },
                visibleRows: 3
            ).HideBorder(),
            
            new HStack(spacing: 2) {
                new Spacer(),
                new Button("Close", () => showCustomModal = false).Primary()
            }
        };
    }
    
    private class SampleData
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }
}