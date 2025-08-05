using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Declarative;
using Andy.TUI.Declarative.Layout;
using Andy.TUI.Declarative.Components;
using Andy.TUI.Declarative.ViewInstances;
using Andy.TUI.Declarative.Tests.TestHelpers;

namespace Andy.TUI.Declarative.Tests.Layout;

/// <summary>
/// Tests for VStack and HStack component layout behavior.
/// </summary>
public class StackLayoutTests
{
    private readonly ITestOutputHelper _output;
    private readonly DeclarativeContext _context;
    
    public StackLayoutTests(ITestOutputHelper output)
    {
        _output = output;
        _context = new DeclarativeContext(() => { });
    }
    
    #region VStack Tests
    
    [Fact]
    public void VStack_WithFixedSizeChildren_ShouldStackVertically()
    {
        // Arrange
        var vstack = new VStack()
        {
            new Box { Width = 100, Height = 50 },
            new Box { Width = 150, Height = 30 },
            new Box { Width = 120, Height = 40 }
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        root.CalculateLayout(constraints);
        root.Layout.AbsoluteX = 0;
        root.Layout.AbsoluteY = 0;
        root.Render();
        
        // Assert
        Assert.Equal(150, root.Layout.Width); // Max child width
        Assert.Equal(120, root.Layout.Height); // Sum of heights
        
        // Verify children positions
        var vstackInstance = root as VStackInstance;
        Assert.NotNull(vstackInstance);
        var children = vstackInstance.GetChildInstances();
        
        Assert.Equal(0, children[0].Layout.Y);
        Assert.Equal(50, children[1].Layout.Y);
        Assert.Equal(80, children[2].Layout.Y);
    }
    
    [Fact]
    public void VStack_WithSpacing_ShouldAddGaps()
    {
        // Arrange
        var vstack = new VStack(spacing: 10)
        {
            new Box { Width = 100, Height = 50 },
            new Box { Width = 100, Height = 50 },
            new Box { Width = 100, Height = 50 }
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        root.CalculateLayout(constraints);
        root.Render();
        
        // Assert
        Assert.Equal(100, root.Layout.Width);
        Assert.Equal(170, root.Layout.Height); // 50*3 + 10*2 = 170
        
        // Verify spacing
        var vstackInstance = root as VStackInstance;
        var children = vstackInstance!.GetChildInstances();
        
        Assert.Equal(0, children[0].Layout.Y);
        Assert.Equal(60, children[1].Layout.Y);   // 50 + 10
        Assert.Equal(120, children[2].Layout.Y);  // 50 + 10 + 50 + 10
    }
    
    #endregion
    
    #region HStack Tests
    
    [Fact]
    public void HStack_WithFixedSizeChildren_ShouldStackHorizontally()
    {
        // Arrange
        var hstack = new HStack()
        {
            new Box { Width = 50, Height = 100 },
            new Box { Width = 30, Height = 150 },
            new Box { Width = 40, Height = 120 }
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(hstack, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        root.CalculateLayout(constraints);
        root.Render();
        
        // Assert
        Assert.Equal(120, root.Layout.Width);  // Sum of widths
        Assert.Equal(150, root.Layout.Height); // Max child height
        
        // Verify children positions
        var hstackInstance = root as HStackInstance;
        Assert.NotNull(hstackInstance);
        var children = hstackInstance.GetChildInstances();
        
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(50, children[1].Layout.X);
        Assert.Equal(80, children[2].Layout.X);
    }
    
    [Fact]
    public void HStack_WithSpacing_ShouldAddGaps()
    {
        // Arrange
        var hstack = new HStack(spacing: 10)
        {
            new Box { Width = 50, Height = 100 },
            new Box { Width = 50, Height = 100 },
            new Box { Width = 50, Height = 100 }
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(hstack, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        root.CalculateLayout(constraints);
        root.Render();
        
        // Assert
        Assert.Equal(170, root.Layout.Width);  // 50*3 + 10*2 = 170
        Assert.Equal(100, root.Layout.Height);
        
        // Verify spacing
        var hstackInstance = root as HStackInstance;
        var children = hstackInstance!.GetChildInstances();
        
        Assert.Equal(0, children[0].Layout.X);
        Assert.Equal(60, children[1].Layout.X);   // 50 + 10
        Assert.Equal(120, children[2].Layout.X);  // 50 + 10 + 50 + 10
    }
    
    #endregion
    
    #region Nested Stack Tests
    
    [Fact]
    public void NestedStacks_ShouldLayoutCorrectly()
    {
        // Arrange - HStack containing VStacks
        var root = new HStack(spacing: 10)
        {
            new VStack(spacing: 5)
            {
                new Box { Width = 50, Height = 30 },
                new Box { Width = 50, Height = 30 }
            },
            new VStack(spacing: 5)
            {
                new Box { Width = 60, Height = 40 },
                new Box { Width = 60, Height = 40 }
            }
        };
        
        var instance = _context.ViewInstanceManager.GetOrCreateInstance(root, "root");
        var constraints = LayoutTestHelper.Loose(400, 400);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(instance, constraints);
        _output.WriteLine(result.LayoutTree);
        
        // Assert
        Assert.Equal(120, result.RootLayout.Width);  // 50 + 10 + 60
        Assert.Equal(85, result.RootLayout.Height);  // max(65, 85) = 85
    }
    
    #endregion
    
    #region Constraint Propagation Tests
    
    [Fact]
    public void VStack_WithTightConstraints_ShouldConstrainChildren()
    {
        // Arrange
        var vstack = new VStack()
        {
            new Box { Width = 200, Height = 100 },
            new Box { Width = 150, Height = 100 }
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");
        var constraints = LayoutTestHelper.Tight(100, 150);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(100, result.RootLayout.Width);
        Assert.Equal(150, result.RootLayout.Height);
        
        // Children should be constrained
        var vstackInstance = root as VStackInstance;
        var children = vstackInstance!.GetChildInstances();
        
        Assert.Equal(100, children[0].Layout.Width); // Constrained from 200
        Assert.Equal(100, children[1].Layout.Width); // Constrained from 150
    }
    
    [Fact]
    public void HStack_WithInfiniteConstraints_ShouldNotProduceInfiniteSize()
    {
        // Arrange
        var hstack = new HStack()
        {
            new Text("Hello"),
            new Text("World")
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(hstack, "root");
        var constraints = LayoutTestHelper.Unconstrained();
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Width, "Width should not be infinite");
        LayoutTestHelper.AssertNotInfinite(result.RootLayout.Height, "Height should not be infinite");
        LayoutTestHelper.AssertReasonableSize(result.RootLayout.Width, 0, 1000);
        LayoutTestHelper.AssertReasonableSize(result.RootLayout.Height, 0, 100);
    }
    
    #endregion
    
    #region Edge Cases
    
    [Fact]
    public void EmptyVStack_ShouldHaveZeroSize()
    {
        // Arrange
        var vstack = new VStack();
        var root = _context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    [Fact]
    public void EmptyHStack_ShouldHaveZeroSize()
    {
        // Arrange
        var hstack = new HStack();
        var root = _context.ViewInstanceManager.GetOrCreateInstance(hstack, "root");
        var constraints = LayoutTestHelper.Loose(100, 100);
        
        // Act
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Assert
        Assert.Equal(0, result.RootLayout.Width);
        Assert.Equal(0, result.RootLayout.Height);
    }
    
    [Fact]
    public void VStack_WithNegativeSpacing_ShouldBeHandledGracefully()
    {
        // Arrange
        var vstack = new VStack(spacing: -10) // Negative spacing
        {
            new Box { Width = 50, Height = 50 },
            new Box { Width = 50, Height = 50 }
        };
        
        var root = _context.ViewInstanceManager.GetOrCreateInstance(vstack, "root");
        var constraints = LayoutTestHelper.Loose(200, 200);
        
        // Act & Assert - should not throw
        var result = LayoutTestHelper.PerformLayout(root, constraints);
        
        // Height should handle negative spacing (could overlap or be treated as 0)
        Assert.True(result.RootLayout.Height <= 100); // At most sum of heights
    }
    
    #endregion
}

/// <summary>
/// Extension methods to help with Stack testing.
/// </summary>
internal static class StackTestExtensions
{
    public static IReadOnlyList<ViewInstance> GetChildInstances(this VStackInstance vstack)
    {
        var childrenField = typeof(VStackInstance).GetField("_childInstances", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (childrenField?.GetValue(vstack) as IReadOnlyList<ViewInstance>) ?? new List<ViewInstance>();
    }
    
    public static IReadOnlyList<ViewInstance> GetChildInstances(this HStackInstance hstack)
    {
        var childrenField = typeof(HStackInstance).GetField("_childInstances", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (childrenField?.GetValue(hstack) as IReadOnlyList<ViewInstance>) ?? new List<ViewInstance>();
    }
}