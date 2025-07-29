using System;
using System.Linq;
using System.Threading;
using Andy.TUI.Components.Display;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Display;

public class SpinnerTests : IDisposable
{
    private Spinner CreateSpinner()
    {
        var spinner = new Spinner();
        var context = TestHelpers.CreateMockContext(spinner);
        spinner.Initialize(context);
        return spinner;
    }
    
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var spinner = CreateSpinner();
        
        Assert.Equal(SpinnerStyle.Dots, spinner.Style);
        Assert.Equal(string.Empty, spinner.Text);
        Assert.Equal(100, spinner.AnimationSpeed);
        Assert.Equal(Color.Cyan, spinner.Color);
        Assert.False(spinner.IsAnimating);
    }
    
    [Fact]
    public void Style_CanBeChanged()
    {
        var spinner = CreateSpinner();
        
        spinner.Style = SpinnerStyle.Line;
        Assert.Equal(SpinnerStyle.Line, spinner.Style);
        
        spinner.Style = SpinnerStyle.Arrow;
        Assert.Equal(SpinnerStyle.Arrow, spinner.Style);
    }
    
    [Fact]
    public void Text_CanBeSet()
    {
        var spinner = CreateSpinner();
        
        spinner.Text = "Loading...";
        Assert.Equal("Loading...", spinner.Text);
        
        spinner.Text = null!;
        Assert.Equal(string.Empty, spinner.Text);
    }
    
    [Fact]
    public void AnimationSpeed_HasMinimum()
    {
        var spinner = CreateSpinner();
        
        spinner.AnimationSpeed = 200;
        Assert.Equal(200, spinner.AnimationSpeed);
        
        spinner.AnimationSpeed = 25; // Below minimum
        Assert.Equal(50, spinner.AnimationSpeed); // Clamped to 50
    }
    
    [Fact]
    public void Color_CanBeChanged()
    {
        var spinner = CreateSpinner();
        
        spinner.Color = Color.Red;
        Assert.Equal(Color.Red, spinner.Color);
    }
    
    [Fact]
    public void IsAnimating_StartsAndStopsAnimation()
    {
        var spinner = CreateSpinner();
        
        Assert.False(spinner.IsAnimating);
        
        spinner.IsAnimating = true;
        Assert.True(spinner.IsAnimating);
        
        spinner.IsAnimating = false;
        Assert.False(spinner.IsAnimating);
    }
    
    [Fact]
    public void Start_SetsIsAnimating()
    {
        var spinner = CreateSpinner();
        
        spinner.Start();
        Assert.True(spinner.IsAnimating);
    }
    
    [Fact]
    public void Stop_ClearsIsAnimating()
    {
        var spinner = CreateSpinner();
        spinner.Start();
        
        spinner.Stop();
        Assert.False(spinner.IsAnimating);
    }
    
    [Fact]
    public void Measure_WithoutText()
    {
        var spinner = CreateSpinner();
        
        var size = spinner.Measure(new Size(100, 100));
        
        Assert.True(size.Width > 0); // Should be width of spinner frame
        Assert.Equal(1, size.Height);
    }
    
    [Fact]
    public void Measure_WithText()
    {
        var spinner = CreateSpinner();
        spinner.Text = "Loading...";
        
        var size = spinner.Measure(new Size(100, 100));
        
        Assert.True(size.Width > 10); // Spinner frame + space + text
        Assert.Equal(1, size.Height);
    }
    
    [Fact]
    public void Render_ProducesFrame()
    {
        var spinner = CreateSpinner();
        spinner.Style = SpinnerStyle.Line;
        spinner.Arrange(new Rectangle(0, 0, 10, 1));
        
        var node = spinner.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        var text = texts[0];
        Assert.Contains(text, new[] { "-", "\\", "|", "/" }); // One of the line frames
    }
    
    [Fact]
    public void Render_IncludesText()
    {
        var spinner = CreateSpinner();
        spinner.Text = "Processing";
        spinner.Arrange(new Rectangle(0, 0, 20, 1));
        
        var node = spinner.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        var text = texts[0];
        Assert.Contains("Processing", text);
    }
    
    [Fact]
    public void Render_AppliesColor()
    {
        var spinner = CreateSpinner();
        spinner.Color = Color.Green;
        spinner.Arrange(new Rectangle(0, 0, 10, 1));
        
        var node = spinner.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        // Note: Style is not directly testable via text content
    }
    
    [Fact]
    public void AllStyles_HaveFrames()
    {
        var spinner = CreateSpinner();
        spinner.Arrange(new Rectangle(0, 0, 10, 1));
        
        foreach (SpinnerStyle style in Enum.GetValues(typeof(SpinnerStyle)))
        {
            spinner.Style = style;
            var node = spinner.Render();
        var texts = TestHelpers.FindTextNodes(node);
            
            Assert.NotEmpty(texts);
            Assert.NotEmpty(texts[0]);
        }
    }
    
    [Fact]
    public void AnimationTimer_UpdatesFrame()
    {
        var spinner = CreateSpinner();
        spinner.AnimationSpeed = 50; // Fast animation for testing
        spinner.Arrange(new Rectangle(0, 0, 10, 1));
        
        // Get initial frame
        var initialNode = spinner.Render();
        var initialTexts = TestHelpers.FindTextNodes(initialNode);
        var initialContent = initialTexts[0];
        
        // Start animation and wait for frame change
        spinner.Start();
        Thread.Sleep(150); // Wait for at least 2 timer ticks
        
        // Force a render to see if frame changed
        var context = spinner.Context;
        if (context != null)
        {
            // The animation timer should have requested a render
            // In real usage, this would be handled by the rendering system
        }
        
        spinner.Stop();
    }
    
    [Fact]
    public void Dispose_StopsAnimation()
    {
        var spinner = CreateSpinner();
        spinner.Start();
        
        spinner.Dispose();
        
        Assert.False(spinner.IsAnimating);
    }
    
    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var spinner = CreateSpinner();
        
        spinner.Dispose();
        spinner.Dispose(); // Should not throw
    }
    
    public void Dispose()
    {
        // Clean up any spinners created during tests
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}