using Xunit;
using Moq;
using Andy.TUI.Terminal;
using System.Text;

namespace Andy.TUI.Terminal.Tests.Rendering;

public class IntegrationTest
{
    [Fact]
    public void RenderingSystem_Integration_Works()
    {
        // Arrange
        var mockTerminal = new Mock<ITerminal>();
        var output = new StringBuilder();
        
        mockTerminal.Setup(t => t.Width).Returns(10);
        mockTerminal.Setup(t => t.Height).Returns(5);
        mockTerminal.Setup(t => t.Write(It.IsAny<string>()))
            .Callback<string>(s => output.Append(s));
        mockTerminal.Setup(t => t.SupportsColor).Returns(true);
        mockTerminal.Setup(t => t.SupportsAnsi).Returns(true);
        
        // Create rendering system
        var renderingSystem = new RenderingSystem(mockTerminal.Object);
        renderingSystem.Initialize();
        
        // Act
        renderingSystem.WriteText(0, 0, "Hello", new Style { Foreground = Color.Red });
        renderingSystem.DrawBox(0, 1, 8, 3, new Style { Foreground = Color.Blue }, BoxStyle.Single);
        renderingSystem.Render(); // Force immediate render
        
        // Wait a bit for render
        System.Threading.Thread.Sleep(100);
        
        // Assert
        var outputStr = output.ToString();
        Assert.NotEmpty(outputStr);
        Assert.Contains("H", outputStr); // Should contain the text characters
        Assert.Contains("e", outputStr);
        Assert.Contains("l", outputStr);
        Assert.Contains("o", outputStr);
        Assert.Contains("\x1b[", outputStr); // Should contain ANSI escape sequences
        
        // Cleanup
        renderingSystem.Shutdown();
    }
    
    [Fact]
    public void TerminalBuffer_DoubleBuffering_Works()
    {
        // Arrange
        var buffer = new TerminalBuffer(10, 5);
        
        // Act
        buffer.SetCell(0, 0, 'A');
        buffer.SetCell(1, 0, 'B');
        
        // Assert before swap
        Assert.True(buffer.IsDirty);
        
        // Swap buffers
        var regions = buffer.SwapBuffers().ToList();
        
        // Assert after swap
        Assert.False(buffer.IsDirty);
        Assert.Equal(2, regions.Count);
        Assert.Equal('A', regions[0].NewCell.Character);
        Assert.Equal('B', regions[1].NewCell.Character);
    }
    
    [Fact]
    public void AnsiRenderer_DirectUsage_Works()
    {
        // Arrange
        var mockTerminal = new Mock<ITerminal>();
        var output = new StringBuilder();
        
        mockTerminal.Setup(t => t.Width).Returns(80);
        mockTerminal.Setup(t => t.Height).Returns(24);
        mockTerminal.Setup(t => t.Write(It.IsAny<string>()))
            .Callback<string>(s => output.Append(s));
        mockTerminal.Setup(t => t.SupportsColor).Returns(true);
        mockTerminal.Setup(t => t.SupportsAnsi).Returns(true);
        
        var renderer = new AnsiRenderer(mockTerminal.Object);
        
        // Act
        renderer.BeginFrame();
        renderer.DrawText(0, 0, "Test", new Style { Foreground = Color.Green });
        renderer.EndFrame();
        
        // Assert
        var outputStr = output.ToString();
        Assert.NotEmpty(outputStr);
        Assert.Contains("Test", outputStr);
        Assert.Contains("\x1b[1;1H", outputStr); // Cursor positioning
        Assert.Contains("\x1b[92m", outputStr); // Bright Green color (ConsoleColor.Green maps to bright green in ANSI)
        
        // Verify Flush was called
        mockTerminal.Verify(t => t.Flush(), Times.Once);
    }
}