using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Layout;

/// <summary>
/// Tests for Text component layout behavior.
/// </summary>
public class TextLayoutTests
{
    private readonly ITestOutputHelper _output;
    private readonly DeclarativeContext _context;
    
    public TextLayoutTests(ITestOutputHelper output)
    {
        _output = output;
        _context = new DeclarativeContext(() => { });
    }
    
    #region Basic Text Layout Tests
    
    [Fact]
    public void Text_WithShortContent_ShouldSizeToContent()
    {
        // Arrange
        var text = new Text("Hello World");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(11, result.RootLayout.Width); // "Hello World" = 11 chars
        Assert.Equal(1, result.RootLayout.Height); // Single line
    }
    
    [Fact]
    public void Text_WithEmptyContent_ShouldHaveZeroWidth()
    {
        // Arrange
        var text = new Text("");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(1, result.RootLayout.Height); // Still one line high
    }
    
    [Fact]
    public void Text_WithNewlines_ShouldCalculateMultilineHeight()
    {
        // Arrange
        var text = new Text("Line 1\nLine 2\nLine 3");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(6, result.RootLayout.Width); // Longest line "Line 1" = 6 chars
        Assert.Equal(3, result.RootLayout.Height); // 3 lines
    }
    
    #endregion
    
    #region Text Wrapping Tests
    
    [Fact]
    public void Text_ExceedingWidth_WithWrapEnabled_ShouldWrap()
    {
        // Arrange
        var text = new Text("This is a long text that should wrap when it exceeds the available width")
            .Wrap(TextWrap.Word);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(20, 10);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Text wrapped to {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width); // Should use max available width
        Assert.True(result.RootLayout.Height > 1); // Should wrap to multiple lines
    }
    
    [Fact]
    public void Text_ExceedingWidth_WithWrapDisabled_ShouldClip()
    {
        // Arrange
        var text = new Text("This is a long text that should not wrap");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(20, 10);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width); // Clipped to max width
        Assert.Equal(1, result.RootLayout.Height); // Single line
    }
    
    [Fact]
    public void Text_WithWrap_AndInfiniteConstraints_ShouldNotWrap()
    {
        // Arrange
        var text = new Text("This text should not wrap with infinite constraints")
            .Wrap(TextWrap.Word);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Unconstrained();
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Width, "Width should not be infinite");
        Assert.Equal(51, result.RootLayout.Width); // Full text width
        Assert.Equal(1, result.RootLayout.Height); // Single line
    }
    
    #endregion
    
    #region Text in Containers Tests
    
    [Fact]
    public void Text_InBox_WithAutoHeight_ShouldDetermineBoxHeight()
    {
        // Arrange
        var box = new Box { Width = 30, Height = Length.Auto };
        box.Add(new Text("This is some text that will wrap in the box").Wrap(TextWrap.Word));
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(box, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(30, result.RootLayout.Width);
        Assert.True(result.RootLayout.Height > 1); // Should have wrapped text height
    }
    
    [Fact]
    public void Text_InVStack_ShouldContributeToStackHeight()
    {
        // Arrange
        var vstack = new VStack()
        {
            new Text("Line 1"),
            new Text("Line 2\nWith newline"),
            new Text("Line 3")
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(4, result.RootLayout.Height); // 1 + 2 + 1 = 4 lines total
    }
    
    #endregion
    
    #region Constraint Tests
    
    [Fact]
    public void Text_WithTightConstraints_ShouldRespectExactSize()
    {
        // Arrange
        var text = new Text("Some text content");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Tight(10, 5);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(10, result.RootLayout.Width);
        Assert.Equal(5, result.RootLayout.Height);
    }
    
    [Fact]
    public void Text_WithMinConstraints_ShouldExpandToMinimum()
    {
        // Arrange
        var text = new Text("Hi");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Custom(10, 50, 5, 10);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(10, result.RootLayout.Width); // Expanded to min width
        Assert.Equal(5, result.RootLayout.Height); // Expanded to min height
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact]
    public void Text_WithOnlyWhitespace_ShouldMaintainWidth()
    {
        // Arrange
        var text = new Text("     "); // 5 spaces
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(5, result.RootLayout.Width);
        Assert.Equal(1, result.RootLayout.Height);
    }
    
    [Fact]
    public void Text_WithMixedNewlinesAndWrapping_ShouldHandleCorrectly()
    {
        // Arrange
        var text = new Text("First line\nSecond line is very long and should wrap\nThird line")
            .Wrap(TextWrap.Word);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(20, 20);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Result: {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width);
        Assert.True(result.RootLayout.Height >= 3); // At least 3 lines due to explicit newlines
    }
    
    [Fact]
    public void Text_WithZeroWidthConstraint_ShouldHandleGracefully()
    {
        // Arrange
        var text = new Text("Text");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Tight(0, 10);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(10, result.RootLayout.Height);
    }
    
    [Fact]
    public void Text_WithVeryLongWord_ShouldNotExceedConstraints()
    {
        // Arrange
        var text = new Text("ThisIsAVeryLongWordWithoutAnySpacesToBreakItProperly")
            .Wrap(TextWrap.Character);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(20, 10);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width); // Should not exceed max width
    }
    
    #endregion
    
    #region Unicode and Special Characters
    
    [Fact]
    public void Text_WithUnicodeCharacters_ShouldCalculateCorrectWidth()
    {
        // Arrange
        var text = new Text("Hello 世界"); // Mixed ASCII and Unicode
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        // Note: Unicode width calculation may vary, but should be reasonable
        Assert.True(result.RootLayout.Width > 0);
        Assert.Equal(1, result.RootLayout.Height);
    }
    
    [Fact]
    public void Text_WithTabs_ShouldHandleTabWidth()
    {
        // Arrange
        var text = new Text("Hello\tWorld");
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        // Tab should expand to some width (typically 4 or 8 spaces)
        Assert.True(result.RootLayout.Width > 11); // More than just the character count
        Assert.Equal(1, result.RootLayout.Height);
    }
    
    #endregion
    
    #region Max Width Tests
    
    [Fact]
    public void Text_WithMaxWidth_ShouldConstrainWidth()
    {
        // Arrange
        var text = new Text("This is a long text that should be constrained by max width property")
            .Wrap(TextWrap.Word)
            .MaxWidth(30);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Result: {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(30, result.RootLayout.Width); // Should respect max width
        Assert.True(result.RootLayout.Height > 1); // Should wrap to multiple lines
    }
    
    [Fact]
    public void Text_WithMaxWidth_SmallerThanContent_ShouldForceWrap()
    {
        // Arrange
        var text = new Text("Short text")
            .Wrap(TextWrap.Word)
            .MaxWidth(5);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Result: {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(5, result.RootLayout.Width); // Should respect max width
        Assert.True(result.RootLayout.Height >= 2); // "Short" and "text" on separate lines
    }
    
    [Fact]
    public void Text_WithMaxWidth_AndNoWrap_ShouldClip()
    {
        // Arrange
        var text = new Text("This text will be clipped")
            .MaxWidth(10); // No wrap enabled
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(10, result.RootLayout.Width); // Should be constrained to max width
        Assert.Equal(1, result.RootLayout.Height); // Single line (clipped)
    }
    
    #endregion
    
    #region Max Lines Tests
    
    [Fact]
    public void Text_WithMaxLines_ShouldLimitHeight()
    {
        // Arrange
        var text = new Text("Line 1\nLine 2\nLine 3\nLine 4\nLine 5")
            .MaxLines(3);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Result: {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(3, result.RootLayout.Height); // Should be limited to 3 lines
    }
    
    [Fact]
    public void Text_WithMaxLines_AndWrapping_ShouldLimitWrappedLines()
    {
        // Arrange
        var text = new Text("This is a very long text that will wrap to multiple lines when rendered")
            .Wrap(TextWrap.Word)
            .MaxLines(2);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(20, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Result: {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width);
        Assert.Equal(2, result.RootLayout.Height); // Should be limited to 2 lines
    }
    
    [Fact]
    public void Text_WithMaxLines_Zero_ShouldShowNothing()
    {
        // Arrange
        var text = new Text("This text should not be visible")
            .MaxLines(0);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Height); // No lines shown
    }
    
    #endregion
    
    #region Truncation Mode Tests
    
    [Fact]
    public void Text_WithTruncationTail_ShouldShowEllipsisAtEnd()
    {
        // Arrange
        var text = new Text("This is a long text that will be truncated")
            .MaxWidth(20)
            .Truncate(TruncationMode.Tail);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        _output.WriteLine($"Result: {result.RootLayout.Width}x{result.RootLayout.Height}");
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width);
        Assert.Equal(1, result.RootLayout.Height); // Single line with truncation
    }
    
    [Fact]
    public void Text_WithTruncationHead_ShouldShowEllipsisAtStart()
    {
        // Arrange
        var text = new Text("This is a long text that will be truncated")
            .MaxWidth(20)
            .Truncate(TruncationMode.Head);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(20, result.RootLayout.Width);
        Assert.Equal(1, result.RootLayout.Height);
    }
    
    [Fact]
    public void Text_WithTruncationMiddle_ShouldShowEllipsisInMiddle()
    {
        // Arrange
        var text = new Text("This is a long text that will be truncated in the middle")
            .MaxWidth(25)
            .Truncate(TruncationMode.Middle);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(25, result.RootLayout.Width);
        Assert.Equal(1, result.RootLayout.Height);
    }
    
    [Fact]
    public void Text_WithTruncation_AndMaxLines_ShouldTruncateLastVisibleLine()
    {
        // Arrange
        var text = new Text("Line 1\nLine 2\nLine 3\nLine 4")
            .MaxLines(2)
            .Truncate(TruncationMode.Tail);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(2, result.RootLayout.Height); // Only 2 lines shown
        // The second line should show truncation indicator
    }
    
    [Fact]
    public void Text_WithTruncation_ShorterThanMaxWidth_ShouldNotTruncate()
    {
        // Arrange
        var text = new Text("Short")
            .MaxWidth(20)
            .Truncate(TruncationMode.Tail);
        var root = _context.ViewInstanceManager.GetOrCreateInstance(text, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(5, result.RootLayout.Width); // Natural width, no truncation
        Assert.Equal(1, result.RootLayout.Height);
    }
    
    #endregion
}