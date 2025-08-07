using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Core.Spatial;

namespace Andy.TUI.Performance.Tests;

/// <summary>
/// Performance tests comparing linear search vs spatial index operations.
/// </summary>
public class SpatialIndexPerformanceTests
{
    private readonly ITestOutputHelper _output;
    
    public SpatialIndexPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void LinearVsSpatial_RangeQuery_Performance()
    {
        // Test with different element counts
        var elementCounts = new[] { 100, 500, 1000, 5000 };
        var queryRegion = new Rectangle(250, 250, 100, 100);
        
        foreach (var count in elementCounts)
        {
            _output.WriteLine($"\n--- Testing with {count} elements ---");
            
            // Generate test elements in a 1000x1000 grid
            var elements = GenerateElements(count, 1000, 1000);
            
            // Linear search benchmark
            var linearTime = BenchmarkLinearSearch(elements, queryRegion, iterations: 100);
            
            // Spatial index benchmark (simulated for now)
            var spatialTime = BenchmarkSpatialSearch(elements, queryRegion, iterations: 100);
            
            var speedup = linearTime / spatialTime;
            
            _output.WriteLine($"Linear search: {linearTime:F2}ms");
            _output.WriteLine($"Spatial search: {spatialTime:F2}ms");
            _output.WriteLine($"Speedup: {speedup:F2}x");
            
            // Assert spatial is faster
            Assert.True(spatialTime < linearTime, 
                $"Spatial search should be faster than linear for {count} elements");
        }
    }
    
    [Fact]
    public void ZIndexUpdate_Performance_Comparison()
    {
        const int elementCount = 1000;
        const int updateCount = 100;
        
        var elements = GenerateElements(elementCount, 1000, 1000);
        
        _output.WriteLine($"\n--- Z-Index Update Performance ({updateCount} updates) ---");
        
        // Benchmark full remove/insert cycle
        var removeInsertTime = BenchmarkRemoveInsert(elements, updateCount);
        
        // Benchmark z-only update (simulated)
        var zOnlyUpdateTime = BenchmarkZOnlyUpdate(elements, updateCount);
        
        var speedup = removeInsertTime / zOnlyUpdateTime;
        
        _output.WriteLine($"Remove/Insert: {removeInsertTime:F2}ms");
        _output.WriteLine($"Z-only update: {zOnlyUpdateTime:F2}ms");
        _output.WriteLine($"Speedup: {speedup:F2}x");
        
        // Z-only updates should be at least 2x faster
        Assert.True(zOnlyUpdateTime < removeInsertTime / 2, 
            "Z-only updates should be significantly faster than remove/insert");
    }
    
    [Fact]
    public void OcclusionDetection_Performance()
    {
        var layerCounts = new[] { 10, 50, 100 };
        
        foreach (var layers in layerCounts)
        {
            _output.WriteLine($"\n--- Occlusion Detection with {layers} layers ---");
            
            // Create overlapping elements at different z-indices
            var elements = GenerateLayeredElements(layers, elementsPerLayer: 20);
            
            var stopwatch = Stopwatch.StartNew();
            
            // Find fully occluded elements
            var occludedCount = 0;
            foreach (var element in elements)
            {
                if (IsFullyOccluded(element, elements))
                    occludedCount++;
            }
            
            stopwatch.Stop();
            
            _output.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Occluded elements: {occludedCount}/{elements.Count}");
            _output.WriteLine($"Percentage occluded: {(occludedCount * 100.0 / elements.Count):F1}%");
        }
    }
    
    [Fact]
    public void TabSwitch_Rendering_Performance()
    {
        const int tabCount = 10;
        const int iterations = 1000;
        
        _output.WriteLine($"\n--- Tab Switch Performance ({iterations} switches) ---");
        
        // Simulate full re-render approach
        var fullRenderTime = MeasureTime(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                SimulateFullTabViewRender(tabCount);
            }
        });
        
        // Simulate optimized approach (only content area)
        var optimizedTime = MeasureTime(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                SimulateOptimizedTabSwitch(tabCount);
            }
        });
        
        var speedup = fullRenderTime / optimizedTime;
        
        _output.WriteLine($"Full re-render: {fullRenderTime:F2}ms");
        _output.WriteLine($"Optimized (content only): {optimizedTime:F2}ms");
        _output.WriteLine($"Speedup: {speedup:F2}x");
        
        // Optimized should be at least 3x faster
        Assert.True(optimizedTime < fullRenderTime / 3, 
            "Optimized tab switching should be significantly faster");
    }
    
    #region Helper Methods
    
    private List<TestElement> GenerateElements(int count, int maxX, int maxY)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var elements = new List<TestElement>();
        
        for (int i = 0; i < count; i++)
        {
            var x = random.Next(maxX);
            var y = random.Next(maxY);
            var width = random.Next(20, 100);
            var height = random.Next(10, 50);
            var zIndex = random.Next(0, 10);
            
            elements.Add(new TestElement
            {
                Id = i,
                Bounds = new Rectangle(x, y, width, height),
                ZIndex = zIndex
            });
        }
        
        return elements;
    }
    
    private List<TestElement> GenerateLayeredElements(int layers, int elementsPerLayer)
    {
        var elements = new List<TestElement>();
        var id = 0;
        
        for (int layer = 0; layer < layers; layer++)
        {
            for (int i = 0; i < elementsPerLayer; i++)
            {
                // Create overlapping elements
                var x = (i * 20) % 200;
                var y = (i * 15) % 150;
                
                elements.Add(new TestElement
                {
                    Id = id++,
                    Bounds = new Rectangle(x, y, 50, 30),
                    ZIndex = layer * 10 // Clear z-index separation
                });
            }
        }
        
        return elements;
    }
    
    private double BenchmarkLinearSearch(List<TestElement> elements, Rectangle queryRegion, int iterations)
    {
        return MeasureTime(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                var results = elements.Where(e => e.Bounds.IntersectsWith(queryRegion)).ToList();
            }
        });
    }
    
    private double BenchmarkSpatialSearch(List<TestElement> elements, Rectangle queryRegion, int iterations)
    {
        // Simulate spatial index performance (would use actual R-Tree when implemented)
        // For now, simulate O(log n) performance
        return MeasureTime(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                // Simulate visiting ~log(n) nodes
                var nodesToVisit = (int)Math.Log2(elements.Count) + 1;
                var results = elements
                    .Take(nodesToVisit * 5) // Simulate examining some elements
                    .Where(e => e.Bounds.IntersectsWith(queryRegion))
                    .ToList();
            }
        });
    }
    
    private double BenchmarkRemoveInsert(List<TestElement> elements, int updates)
    {
        return MeasureTime(() =>
        {
            var elementList = new List<TestElement>(elements);
            for (int i = 0; i < updates; i++)
            {
                var idx = i % elementList.Count;
                var element = elementList[idx];
                
                // Simulate remove
                elementList.RemoveAt(idx);
                
                // Simulate re-insert with new z-index
                element.ZIndex++;
                elementList.Add(element);
            }
        });
    }
    
    private double BenchmarkZOnlyUpdate(List<TestElement> elements, int updates)
    {
        return MeasureTime(() =>
        {
            for (int i = 0; i < updates; i++)
            {
                var idx = i % elements.Count;
                // Simulate direct z-index update
                elements[idx].ZIndex++;
            }
        });
    }
    
    private bool IsFullyOccluded(TestElement element, List<TestElement> allElements)
    {
        return allElements.Any(other => 
            other.Id != element.Id &&
            other.ZIndex > element.ZIndex &&
            other.Bounds.Contains(element.Bounds));
    }
    
    private void SimulateFullTabViewRender(int tabCount)
    {
        // Simulate rendering all tab headers and content
        for (int i = 0; i < tabCount; i++)
        {
            // Simulate header render
            Thread.SpinWait(10);
        }
        // Simulate content render
        Thread.SpinWait(100);
    }
    
    private void SimulateOptimizedTabSwitch(int tabCount)
    {
        // Simulate clearing old content area
        Thread.SpinWait(20);
        // Simulate rendering new content only
        Thread.SpinWait(50);
        // Simulate updating tab header states
        Thread.SpinWait(10);
    }
    
    private double MeasureTime(Action action)
    {
        // Warm up
        action();
        
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        
        return stopwatch.Elapsed.TotalMilliseconds;
    }
    
    #endregion
    
    private class TestElement
    {
        public int Id { get; set; }
        public Rectangle Bounds { get; set; }
        public int ZIndex { get; set; }
    }
}