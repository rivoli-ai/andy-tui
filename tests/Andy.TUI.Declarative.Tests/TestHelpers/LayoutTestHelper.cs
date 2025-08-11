using System;
using System.Collections.Generic;
using System.Text;
using Andy.TUI.Layout;
using Andy.TUI.Declarative.ViewInstances;
using Xunit;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

/// <summary>
/// Helper class for testing layout calculations and constraints.
/// </summary>
public static class LayoutTestHelper
{
    #region Constraint Creation Helpers

    /// <summary>
    /// Creates tight constraints that force a specific size.
    /// </summary>
    public static LayoutConstraints Tight(float width, float height)
    {
        return LayoutConstraints.Tight(width, height);
    }

    /// <summary>
    /// Creates loose constraints with maximum bounds.
    /// </summary>
    public static LayoutConstraints Loose(float width, float height)
    {
        return LayoutConstraints.Loose(width, height);
    }

    /// <summary>
    /// Creates unconstrained layout constraints.
    /// </summary>
    public static LayoutConstraints Unconstrained()
    {
        return LayoutConstraints.Unconstrained;
    }

    /// <summary>
    /// Creates constraints with specific min/max values.
    /// </summary>
    public static LayoutConstraints Custom(float minWidth, float maxWidth, float minHeight, float maxHeight)
    {
        return new LayoutConstraints(minWidth, maxWidth, minHeight, maxHeight);
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Asserts that two layout boxes are equal within a tolerance.
    /// </summary>
    public static void AssertLayoutBox(LayoutBox actual, LayoutBox expected, float tolerance = 0.01f)
    {
        Assert.InRange(actual.X, expected.X - tolerance, expected.X + tolerance);
        Assert.InRange(actual.Y, expected.Y - tolerance, expected.Y + tolerance);
        Assert.InRange(actual.Width, expected.Width - tolerance, expected.Width + tolerance);
        Assert.InRange(actual.Height, expected.Height - tolerance, expected.Height + tolerance);
        Assert.InRange(actual.AbsoluteX, expected.AbsoluteX - tolerance, expected.AbsoluteX + tolerance);
        Assert.InRange(actual.AbsoluteY, expected.AbsoluteY - tolerance, expected.AbsoluteY + tolerance);
    }

    /// <summary>
    /// Asserts that a value is not infinite.
    /// </summary>
    public static void AssertNotInfinite(float value, string message)
    {
        Assert.False(float.IsInfinity(value), message);
    }

    /// <summary>
    /// Asserts that a value is within a reasonable range.
    /// </summary>
    public static void AssertReasonableSize(float value, float min, float max, string valueName = "value")
    {
        Assert.False(float.IsInfinity(value), $"{valueName} should not be infinite");
        Assert.False(float.IsNaN(value), $"{valueName} should not be NaN");
        Assert.InRange(value, min, max);
    }

    /// <summary>
    /// Asserts that layout constraints are valid.
    /// </summary>
    public static void AssertValidConstraints(LayoutConstraints constraints)
    {
        Assert.True(constraints.MinWidth >= 0, "MinWidth should be non-negative");
        Assert.True(constraints.MinHeight >= 0, "MinHeight should be non-negative");
        Assert.True(constraints.MinWidth <= constraints.MaxWidth, "MinWidth should not exceed MaxWidth");
        Assert.True(constraints.MinHeight <= constraints.MaxHeight, "MinHeight should not exceed MaxHeight");

        // Check for reasonable values (not too extreme unless intended)
        if (!float.IsPositiveInfinity(constraints.MaxWidth))
        {
            AssertReasonableSize(constraints.MaxWidth, 0, 10000, "MaxWidth");
        }
        if (!float.IsPositiveInfinity(constraints.MaxHeight))
        {
            AssertReasonableSize(constraints.MaxHeight, 0, 10000, "MaxHeight");
        }
    }

    #endregion

    #region Debug Helpers

    /// <summary>
    /// Creates a visual representation of the layout tree.
    /// </summary>
    public static string VisualizeLayout(ViewInstance root)
    {
        var sb = new StringBuilder();
        VisualizeLayoutRecursive(root, sb, "", true);
        return sb.ToString();
    }

    private static void VisualizeLayoutRecursive(ViewInstance instance, StringBuilder sb, string indent, bool isLast)
    {
        // Draw tree structure
        sb.Append(indent);
        if (indent.Length > 0)
        {
            sb.Append(isLast ? "└── " : "├── ");
        }

        // Add instance info
        var layout = instance.Layout;
        sb.AppendLine($"{instance.GetType().Name} [{layout.X:F1},{layout.Y:F1} {layout.Width:F1}x{layout.Height:F1}] " +
                     $"Abs:[{layout.AbsoluteX},{layout.AbsoluteY}]");

        // Process children
        if (instance is IContainerInstance container)
        {
            var children = container.GetChildInstances();
            for (int i = 0; i < children.Count; i++)
            {
                var childIndent = indent + (isLast ? "    " : "│   ");
                VisualizeLayoutRecursive(children[i], sb, childIndent, i == children.Count - 1);
            }
        }
    }

    /// <summary>
    /// Dumps the constraint tree for debugging.
    /// </summary>
    public static string DumpConstraintTree(ViewInstance root, LayoutConstraints rootConstraints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Constraint Tree ===");
        DumpConstraintTreeRecursive(root, rootConstraints, sb, "", true);
        return sb.ToString();
    }

    private static void DumpConstraintTreeRecursive(ViewInstance instance, LayoutConstraints constraints,
        StringBuilder sb, string indent, bool isLast)
    {
        // Draw tree structure
        sb.Append(indent);
        if (indent.Length > 0)
        {
            sb.Append(isLast ? "└── " : "├── ");
        }

        // Add constraint info
        sb.AppendLine($"{instance.GetType().Name} " +
                     $"W:[{constraints.MinWidth:F1}-{FormatFloat(constraints.MaxWidth)}] " +
                     $"H:[{constraints.MinHeight:F1}-{FormatFloat(constraints.MaxHeight)}]");

        // Note: To show child constraints, we'd need to capture them during layout
        // This is a simplified version for demonstration
    }

    private static string FormatFloat(float value)
    {
        if (float.IsPositiveInfinity(value)) return "∞";
        if (float.IsNegativeInfinity(value)) return "-∞";
        if (float.IsNaN(value)) return "NaN";
        return value.ToString("F1");
    }

    #endregion

    #region Layout Testing Utilities

    /// <summary>
    /// Performs a layout calculation and returns the result.
    /// </summary>
    public static LayoutResult PerformLayout(ViewInstance root, LayoutConstraints constraints)
    {
        // Calculate layout
        root.CalculateLayout(constraints);

        // Set absolute position for root
        root.Layout.AbsoluteX = 0;
        root.Layout.AbsoluteY = 0;

        // Trigger render to propagate absolute positions
        root.Render();

        return new LayoutResult
        {
            RootLayout = root.Layout,
            LayoutTree = VisualizeLayout(root)
        };
    }

    /// <summary>
    /// Creates a layout snapshot for comparison.
    /// </summary>
    public static LayoutSnapshot CreateSnapshot(ViewInstance root)
    {
        var snapshot = new LayoutSnapshot();
        CollectLayoutSnapshot(root, snapshot);
        return snapshot;
    }

    private static void CollectLayoutSnapshot(ViewInstance instance, LayoutSnapshot snapshot)
    {
        snapshot.Add(instance.Id, instance.Layout);

        if (instance is IContainerInstance container)
        {
            foreach (var child in container.GetChildInstances())
            {
                CollectLayoutSnapshot(child, snapshot);
            }
        }
    }

    #endregion
}

/// <summary>
/// Represents the result of a layout calculation.
/// </summary>
public class LayoutResult
{
    public LayoutBox RootLayout { get; set; } = new();
    public string LayoutTree { get; set; } = string.Empty;
}

/// <summary>
/// Represents a snapshot of layout state for comparison.
/// </summary>
public class LayoutSnapshot
{
    private readonly Dictionary<string, LayoutBox> _layouts = new();

    public void Add(string id, LayoutBox layout)
    {
        _layouts[id] = layout;
    }

    public LayoutBox Get(string id)
    {
        return _layouts.TryGetValue(id, out var layout) ? layout : new LayoutBox();
    }

    public void AssertEqual(LayoutSnapshot other, float tolerance = 0.01f)
    {
        foreach (var kvp in _layouts)
        {
            var otherLayout = other.Get(kvp.Key);
            LayoutTestHelper.AssertLayoutBox(otherLayout, kvp.Value, tolerance);
        }
    }
}

/// <summary>
/// Interface for container instances that have children.
/// </summary>
public interface IContainerInstance
{
    IReadOnlyList<ViewInstance> GetChildInstances();
}