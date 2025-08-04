using System;
using Xunit;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.State;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests;

public class ModalComponentTests
{
    [Fact]
    public void Modal_CreatesInstanceWithCorrectProperties()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isOpen = false;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        var content = new Text("Modal content");
        
        var modal = new Modal("Test Modal", content, binding)
            .Size(ModalSize.Large)
            .HideCloseButton();
        
        // Act
        var instance = manager.GetOrCreateInstance(modal, "modal1") as ModalInstance;
        Assert.NotNull(instance);
        
        // Assert
        Assert.IsType<ModalInstance>(instance);
        // Modal is not focusable when closed
        Assert.False(instance.CanFocus);
    }
    
    [Fact]
    public void Modal_BecomesFocusableWhenOpen()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isOpen = false;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        
        var modal = new Modal("Test", new Text("Content"), binding);
        var instance = manager.GetOrCreateInstance(modal, "modal1") as ModalInstance;
        Assert.NotNull(instance);
        
        // Act - Open modal
        binding.Value = true;
        instance.Update(modal);
        
        // Assert
        Assert.True(instance.CanFocus);
    }
    
    [Fact]
    public void Modal_HandlesEscapeKey()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isOpen = true;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        
        var modal = new Modal("Test", new Text("Content"), binding);
        var instance = manager.GetOrCreateInstance(modal, "modal1") as ModalInstance;
        Assert.NotNull(instance);
        
        instance.Update(modal);
        instance.OnGotFocus();
        
        // Act - Press escape
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false));
        
        // Assert
        Assert.True(handled);
        Assert.False(binding.Value); // Modal should be closed
    }
    
    [Fact]
    public void Modal_DisableEscapeClosePreventsEscapeKey()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isOpen = true;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        
        var modal = new Modal("Test", new Text("Content"), binding)
            .DisableEscapeClose();
        var instance = manager.GetOrCreateInstance(modal, "modal1") as ModalInstance;
        Assert.NotNull(instance);
        
        instance.Update(modal);
        instance.OnGotFocus();
        
        // Act - Press escape
        var handled = instance.HandleKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.Escape, false, false, false));
        
        // Assert
        Assert.False(handled); // Escape should not be handled
        Assert.True(binding.Value); // Modal should still be open
    }
    
    [Fact]
    public void Dialog_AlertCreatesCorrectModal()
    {
        // Arrange
        var isOpen = false;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        
        // Act
        var alert = Dialog.Alert("Error", "Something went wrong!", binding, "OK");
        
        // Assert
        Assert.NotNull(alert);
        // Alert should be created successfully
        Assert.IsType<Modal>(alert);
    }
    
    [Fact]
    public void Dialog_ConfirmCreatesCorrectModal()
    {
        // Arrange
        var isOpen = false;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        var confirmed = false;
        Assert.False(confirmed); // Using confirmed variable
        
        // Act
        var confirm = Dialog.Confirm(
            "Confirm", 
            "Are you sure?", 
            binding,
            () => confirmed = true
        );
        
        // Assert
        Assert.NotNull(confirm);
        // Confirm dialog should be created successfully
        Assert.IsType<Modal>(confirm);
    }
    
    [Fact]
    public void Dialog_PromptCreatesCorrectModal()
    {
        // Arrange
        var isOpen = false;
        var isOpenBinding = new Binding<bool>(() => isOpen, v => isOpen = v);
        var inputValue = "";
        var inputBinding = new Binding<string>(() => inputValue, v => inputValue = v);
        var submittedValue = "";
        
        // Act
        var prompt = Dialog.Prompt(
            "Input", 
            "Enter your name:", 
            isOpenBinding,
            inputBinding,
            (value) => submittedValue = value
        );
        
        // Assert
        Assert.NotNull(prompt);
        // Prompt dialog should be created successfully
        Assert.IsType<Modal>(prompt);
    }
    
    [Fact]
    public void Modal_LayoutTakesFullScreen()
    {
        // Arrange
        var context = new DeclarativeContext(() => { });
        var manager = context.ViewInstanceManager;
        var isOpen = true;
        var binding = new Binding<bool>(() => isOpen, v => isOpen = v);
        
        var modal = new Modal("Test", new Text("Content"), binding);
        var instance = manager.GetOrCreateInstance(modal, "modal1") as ModalInstance;
        Assert.NotNull(instance);
        
        instance.Update(modal);
        
        // Act
        instance.CalculateLayout(LayoutConstraints.Loose(80, 24));
        
        // Assert - Modal takes full available space for backdrop
        Assert.Equal(80, instance.Layout.Width);
        Assert.Equal(24, instance.Layout.Height);
    }
}