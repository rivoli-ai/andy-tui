using System;
using System.Linq;
using Andy.TUI.Components.Display;
using Andy.TUI.Components.Layout;
using Andy.TUI.Core.VirtualDom;
using Andy.TUI.Terminal;
using Xunit;

namespace Andy.TUI.Components.Tests.Display;

public class ProgressBarTests
{
    private ProgressBar CreateProgressBar()
    {
        var progressBar = new ProgressBar();
        var context = TestHelpers.CreateMockContext(progressBar);
        progressBar.Initialize(context);
        return progressBar;
    }
    
    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var progressBar = CreateProgressBar();
        
        Assert.Equal(0, progressBar.Value);
        Assert.Equal(0, progressBar.Minimum);
        Assert.Equal(100, progressBar.Maximum);
        Assert.Equal(ProgressBarStyle.Blocks, progressBar.Style);
        Assert.True(progressBar.ShowPercentage);
        Assert.Equal(string.Empty, progressBar.Label);
        Assert.Equal(Color.Green, progressBar.FillColor);
        Assert.Equal(Color.DarkGray, progressBar.BackgroundColor);
        Assert.Equal(0, progressBar.Percentage);
        Assert.False(progressBar.IsComplete);
    }
    
    [Fact]
    public void Value_ClampedToMinMax()
    {
        var progressBar = CreateProgressBar();
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        
        progressBar.Value = 150;
        Assert.Equal(100, progressBar.Value);
        
        progressBar.Value = -50;
        Assert.Equal(0, progressBar.Value);
    }
    
    [Fact]
    public void Minimum_UpdatesValue()
    {
        var progressBar = CreateProgressBar();
        progressBar.Value = 25;
        
        progressBar.Minimum = 50;
        
        Assert.Equal(50, progressBar.Value); // Value clamped to new minimum
    }
    
    [Fact]
    public void Maximum_UpdatesValue()
    {
        var progressBar = CreateProgressBar();
        progressBar.Value = 75;
        
        progressBar.Maximum = 50;
        
        Assert.Equal(50, progressBar.Value); // Value clamped to new maximum
    }
    
    [Fact]
    public void Percentage_CalculatedCorrectly()
    {
        var progressBar = CreateProgressBar();
        
        progressBar.Value = 50;
        Assert.Equal(50, progressBar.Percentage);
        
        progressBar.Value = 25;
        Assert.Equal(25, progressBar.Percentage);
        
        progressBar.Value = 100;
        Assert.Equal(100, progressBar.Percentage);
    }
    
    [Fact]
    public void Percentage_WithCustomRange()
    {
        var progressBar = CreateProgressBar();
        progressBar.Minimum = 50;
        progressBar.Maximum = 150;
        
        progressBar.Value = 100;
        Assert.Equal(50, progressBar.Percentage); // Halfway between 50 and 150
        
        progressBar.Value = 75;
        Assert.Equal(25, progressBar.Percentage);
    }
    
    [Fact]
    public void SetPercentage_SetsCorrectValue()
    {
        var progressBar = CreateProgressBar();
        
        progressBar.SetPercentage(75);
        Assert.Equal(75, progressBar.Value);
        
        progressBar.Minimum = 100;
        progressBar.Maximum = 200;
        progressBar.SetPercentage(50);
        Assert.Equal(150, progressBar.Value); // 50% between 100 and 200
    }
    
    [Fact]
    public void Increment_IncreasesValue()
    {
        var progressBar = CreateProgressBar();
        progressBar.Value = 10;
        
        progressBar.Increment();
        Assert.Equal(11, progressBar.Value);
        
        progressBar.Increment(5);
        Assert.Equal(16, progressBar.Value);
    }
    
    [Fact]
    public void Increment_RespectsMaximum()
    {
        var progressBar = CreateProgressBar();
        progressBar.Value = 95;
        
        progressBar.Increment(10);
        Assert.Equal(100, progressBar.Value); // Clamped to maximum
    }
    
    [Fact]
    public void IsComplete_TrueWhenAtMaximum()
    {
        var progressBar = CreateProgressBar();
        
        Assert.False(progressBar.IsComplete);
        
        progressBar.Value = 100;
        Assert.True(progressBar.IsComplete);
        
        progressBar.Value = 99.9999;
        Assert.False(progressBar.IsComplete);
    }
    
    [Fact]
    public void Measure_WithoutLabel()
    {
        var progressBar = CreateProgressBar();
        
        var size = progressBar.Measure(new Size(100, 100));
        
        Assert.Equal(40, size.Width); // Default width
        Assert.Equal(1, size.Height);
    }
    
    [Fact]
    public void Measure_WithLabel()
    {
        var progressBar = CreateProgressBar();
        progressBar.Label = "Loading...";
        
        var size = progressBar.Measure(new Size(100, 100));
        
        Assert.Equal(40, size.Width);
        Assert.Equal(2, size.Height); // Extra line for label
    }
    
    [Fact]
    public void Render_BlockStyle()
    {
        var progressBar = CreateProgressBar();
        progressBar.Style = ProgressBarStyle.Blocks;
        progressBar.Value = 50;
        progressBar.ShowPercentage = false;
        progressBar.Arrange(new Rectangle(0, 0, 10, 1));
        
        var node = progressBar.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        var text = texts[0];
        Assert.Contains('█', text); // Full block character
        Assert.Contains('░', text); // Light shade character
    }
    
    [Fact]
    public void Render_LineStyle()
    {
        var progressBar = CreateProgressBar();
        progressBar.Style = ProgressBarStyle.Line;
        progressBar.Value = 50;
        progressBar.ShowPercentage = false;
        progressBar.Arrange(new Rectangle(0, 0, 12, 1));
        
        var node = progressBar.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        var text = texts[0];
        Assert.StartsWith("[", text);
        Assert.EndsWith("]", text);
        Assert.Contains("=", text);
        Assert.Contains(">", text);
    }
    
    [Fact]
    public void Render_DotStyle()
    {
        var progressBar = CreateProgressBar();
        progressBar.Style = ProgressBarStyle.Dots;
        progressBar.Value = 50;
        progressBar.ShowPercentage = false;
        progressBar.Arrange(new Rectangle(0, 0, 10, 1));
        
        var node = progressBar.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        var text = texts[0];
        Assert.Contains('●', text); // Black circle
        Assert.Contains('○', text); // White circle
    }
    
    [Fact]
    public void Render_ShowsPercentage()
    {
        var progressBar = CreateProgressBar();
        progressBar.Value = 75;
        progressBar.ShowPercentage = true;
        progressBar.Arrange(new Rectangle(0, 0, 20, 1));
        
        var node = progressBar.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.NotEmpty(texts);
        var text = texts[0];
        Assert.Contains(" 75%", text);
    }
    
    [Fact]
    public void Render_WithLabel()
    {
        var progressBar = CreateProgressBar();
        progressBar.Label = "Processing...";
        progressBar.Value = 30;
        progressBar.Arrange(new Rectangle(0, 0, 20, 2));
        
        var node = progressBar.Render();
        var texts = TestHelpers.FindTextNodes(node);
        
        Assert.Equal(2, texts.Count);
        Assert.Equal("Processing...", texts[0]);
    }
    
    [Fact]
    public void Style_ChangeCausesRender()
    {
        var progressBar = CreateProgressBar();
        var renderCount = 0;
        progressBar.RenderRequested += (s, e) => renderCount++;
        
        progressBar.Style = ProgressBarStyle.Line;
        
        Assert.True(renderCount > 0);
    }
    
    [Fact]
    public void Value_ChangeCausesRender()
    {
        var progressBar = CreateProgressBar();
        var renderCount = 0;
        progressBar.RenderRequested += (s, e) => renderCount++;
        
        progressBar.Value = 50;
        
        Assert.True(renderCount > 0);
    }
    
    [Fact]
    public void Colors_CanBeCustomized()
    {
        var progressBar = CreateProgressBar();
        
        progressBar.FillColor = Color.Blue;
        progressBar.BackgroundColor = Color.Red;
        
        Assert.Equal(Color.Blue, progressBar.FillColor);
        Assert.Equal(Color.Red, progressBar.BackgroundColor);
    }
}