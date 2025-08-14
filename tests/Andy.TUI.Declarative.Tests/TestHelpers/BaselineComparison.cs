using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

/// <summary>
/// Provides baseline comparison capabilities for screen recordings.
/// Compares actual recordings against expected baselines to detect rendering issues.
/// </summary>
public class BaselineComparison
{
    private readonly ScreenRecorder _actualRecorder;
    private readonly ScreenRecorder _baselineRecorder;
    private readonly ComparisonOptions _options;

    public BaselineComparison(ScreenRecorder actual, ScreenRecorder baseline, ComparisonOptions? options = null)
    {
        _actualRecorder = actual;
        _baselineRecorder = baseline;
        _options = options ?? new ComparisonOptions();
    }

    /// <summary>
    /// Performs a comprehensive comparison between actual and baseline recordings.
    /// </summary>
    public ComparisonReport Compare()
    {
        var actualReport = _actualRecorder.Analyze();
        var baselineReport = _baselineRecorder.Analyze();
        
        var report = new ComparisonReport
        {
            ActualFrames = actualReport.TotalFrames,
            BaselineFrames = baselineReport.TotalFrames,
            Timestamp = DateTime.UtcNow
        };

        // Align frames by action or sequence
        var alignedFrames = AlignFrames(actualReport.Frames, baselineReport.Frames);
        
        foreach (var (actualFrame, baselineFrame, alignment) in alignedFrames)
        {
            if (actualFrame == null || baselineFrame == null)
            {
                report.Issues.Add(new ComparisonIssue
                {
                    Type = "MissingFrame",
                    FrameNumber = actualFrame?.FrameNumber ?? baselineFrame?.FrameNumber ?? -1,
                    Description = actualFrame == null 
                        ? $"Missing actual frame for baseline action: {baselineFrame?.Action}"
                        : $"Extra actual frame not in baseline: {actualFrame.Action}",
                    Severity = "Medium"
                });
                continue;
            }

            // Compare frames
            var frameComparison = CompareFrames(actualFrame, baselineFrame);
            report.FrameComparisons.Add(frameComparison);
            
            // Add issues from frame comparison
            report.Issues.AddRange(frameComparison.Issues);
        }

        // Sequence analysis
        var sequenceIssues = AnalyzeSequence(report.FrameComparisons);
        report.Issues.AddRange(sequenceIssues);

        return report;
    }

    /// <summary>
    /// Aligns frames between actual and baseline recordings.
    /// </summary>
    private List<(ScreenFrame? actual, ScreenFrame? baseline, string alignment)> AlignFrames(
        List<ScreenFrame> actualFrames, 
        List<ScreenFrame> baselineFrames)
    {
        var aligned = new List<(ScreenFrame?, ScreenFrame?, string)>();
        
        // Simple alignment by action name
        var baselineByAction = baselineFrames.ToDictionary(f => f.Action, f => f);
        var matchedBaselines = new HashSet<string>();
        
        foreach (var actual in actualFrames)
        {
            if (baselineByAction.TryGetValue(actual.Action, out var baseline))
            {
                aligned.Add((actual, baseline, "Matched"));
                matchedBaselines.Add(actual.Action);
            }
            else
            {
                // Try fuzzy matching
                var closest = FindClosestBaseline(actual, baselineFrames);
                if (closest != null && !matchedBaselines.Contains(closest.Action))
                {
                    aligned.Add((actual, closest, "Fuzzy"));
                    matchedBaselines.Add(closest.Action);
                }
                else
                {
                    aligned.Add((actual, null, "NoMatch"));
                }
            }
        }
        
        // Add unmatched baselines
        foreach (var baseline in baselineFrames.Where(b => !matchedBaselines.Contains(b.Action)))
        {
            aligned.Add((null, baseline, "Missing"));
        }
        
        return aligned;
    }

    private ScreenFrame? FindClosestBaseline(ScreenFrame actual, List<ScreenFrame> baselines)
    {
        // Simple string similarity for action names
        return baselines
            .Where(b => StringSimilarity(actual.Action, b.Action) > 0.6)
            .OrderByDescending(b => StringSimilarity(actual.Action, b.Action))
            .FirstOrDefault();
    }

    private double StringSimilarity(string a, string b)
    {
        if (a == b) return 1.0;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;
        
        var longer = a.Length > b.Length ? a : b;
        var shorter = a.Length > b.Length ? b : a;
        
        if (longer.Length == 0) return 1.0;
        
        var editDistance = LevenshteinDistance(longer, shorter);
        return (longer.Length - editDistance) / (double)longer.Length;
    }

    private int LevenshteinDistance(string s, string t)
    {
        var n = s.Length;
        var m = t.Length;
        var d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (var i = 0; i <= n; d[i, 0] = i++) { }
        for (var j = 0; j <= m; d[0, j] = j++) { }

        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    /// <summary>
    /// Compares two frames cell by cell.
    /// </summary>
    private FrameComparison CompareFrames(ScreenFrame actual, ScreenFrame baseline)
    {
        var comparison = new FrameComparison
        {
            ActualFrameNumber = actual.FrameNumber,
            BaselineFrameNumber = baseline.FrameNumber,
            Action = actual.Action
        };

        // Check dimensions
        if (actual.Width != baseline.Width || actual.Height != baseline.Height)
        {
            comparison.Issues.Add(new ComparisonIssue
            {
                Type = "DimensionMismatch",
                FrameNumber = actual.FrameNumber,
                Description = $"Dimensions differ: actual {actual.Width}x{actual.Height} vs baseline {baseline.Width}x{baseline.Height}",
                Severity = "High"
            });
            return comparison;
        }

        // Cell-by-cell comparison
        var cellDiffs = new List<CellDiff>();
        var colorMismatches = 0;
        var charMismatches = 0;
        var regions = new Dictionary<(int, int), RegionInfo>();

        for (int y = 0; y < actual.Height; y++)
        {
            var actualLine = actual.Content.Lines.FirstOrDefault(l => l.Y == y);
            var baselineLine = baseline.Content.Lines.FirstOrDefault(l => l.Y == y);

            if (actualLine == null && baselineLine == null) continue;

            for (int x = 0; x < actual.Width; x++)
            {
                var actualChar = actualLine?.Characters.FirstOrDefault(c => c.X == x);
                var baselineChar = baselineLine?.Characters.FirstOrDefault(c => c.X == x);

                if (actualChar == null && baselineChar == null) continue;

                var diff = CompareCell(actualChar, baselineChar, x, y);
                if (diff != null)
                {
                    cellDiffs.Add(diff);
                    
                    if (diff.ColorMismatch) colorMismatches++;
                    if (diff.CharMismatch) charMismatches++;
                    
                    // Track regions
                    AddToRegion(regions, x, y, diff);
                }
            }
        }

        comparison.CellDiffs = cellDiffs;
        comparison.TotalCells = actual.Width * actual.Height;
        comparison.MismatchedCells = cellDiffs.Count;
        comparison.ColorMismatches = colorMismatches;
        comparison.CharMismatches = charMismatches;

        // Analyze regions
        foreach (var region in regions.Values.Where(r => r.Cells.Count > _options.MinRegionSize))
        {
            var issue = AnalyzeRegion(region, actual.FrameNumber);
            if (issue != null)
            {
                comparison.Issues.Add(issue);
            }
        }

        // Calculate diff score
        comparison.DiffScore = CalculateDiffScore(comparison);
        
        // Check threshold
        if (comparison.DiffScore > _options.DiffThreshold)
        {
            comparison.Issues.Add(new ComparisonIssue
            {
                Type = "HighDiffScore",
                FrameNumber = actual.FrameNumber,
                Description = $"Frame differs significantly from baseline (score: {comparison.DiffScore:F2})",
                Severity = "High",
                Details = new List<string>
                {
                    $"Color mismatches: {colorMismatches}",
                    $"Character mismatches: {charMismatches}",
                    $"Total affected cells: {cellDiffs.Count}/{comparison.TotalCells}"
                }
            });
        }

        return comparison;
    }

    private CellDiff? CompareCell(CharInfo? actual, CharInfo? baseline, int x, int y)
    {
        if (actual == null || baseline == null)
        {
            return new CellDiff
            {
                X = x,
                Y = y,
                CharMismatch = true,
                ActualChar = actual?.Char ?? '\0',
                BaselineChar = baseline?.Char ?? '\0'
            };
        }

        var charMatch = actual.Char == baseline.Char;
        var fgMatch = actual.ForegroundColor == baseline.ForegroundColor;
        var bgMatch = actual.BackgroundColor == baseline.BackgroundColor;

        if (charMatch && fgMatch && bgMatch)
            return null;

        var diff = new CellDiff
        {
            X = x,
            Y = y,
            CharMismatch = !charMatch,
            ColorMismatch = !fgMatch || !bgMatch,
            ActualChar = actual.Char,
            BaselineChar = baseline.Char,
            ActualFg = actual.ForegroundColor,
            BaselineFg = baseline.ForegroundColor,
            ActualBg = actual.BackgroundColor,
            BaselineBg = baseline.BackgroundColor
        };

        // Calculate color distance if colors differ
        if (diff.ColorMismatch)
        {
            diff.ColorDistance = CalculateColorDistance(
                actual.ForegroundColor, baseline.ForegroundColor,
                actual.BackgroundColor, baseline.BackgroundColor);
        }

        return diff;
    }

    private double CalculateColorDistance(string actualFg, string baselineFg, string actualBg, string baselineBg)
    {
        // Simple distance based on whether colors match
        // Could be enhanced with actual RGB distance calculation
        double distance = 0;
        if (actualFg != baselineFg) distance += 1.0;
        if (actualBg != baselineBg) distance += 1.0;
        return distance;
    }

    private void AddToRegion(Dictionary<(int, int), RegionInfo> regions, int x, int y, CellDiff diff)
    {
        var key = (x, y);
        
        // Check adjacent cells for existing regions
        var adjacentKeys = new[]
        {
            (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)
        };
        
        RegionInfo? existingRegion = null;
        foreach (var adj in adjacentKeys)
        {
            if (regions.TryGetValue(adj, out var region))
            {
                existingRegion = region;
                break;
            }
        }
        
        if (existingRegion != null)
        {
            existingRegion.Cells.Add(diff);
            regions[key] = existingRegion;
        }
        else
        {
            var newRegion = new RegionInfo
            {
                Id = Guid.NewGuid().ToString(),
                Cells = new List<CellDiff> { diff }
            };
            regions[key] = newRegion;
        }
    }

    private ComparisonIssue? AnalyzeRegion(RegionInfo region, int frameNumber)
    {
        // Analyze characteristics of the region
        var bounds = GetRegionBounds(region);
        var colorOnly = region.Cells.All(c => c.ColorMismatch && !c.CharMismatch);
        var charOnly = region.Cells.All(c => c.CharMismatch && !c.ColorMismatch);
        
        if (colorOnly && region.Cells.Count > 5)
        {
            // Analyze color patterns
            var bgColors = region.Cells.Select(c => c.ActualBg).Distinct().ToList();
            var fgColors = region.Cells.Select(c => c.ActualFg).Distinct().ToList();
            
            return new ComparisonIssue
            {
                Type = "ColorRegionMismatch",
                FrameNumber = frameNumber,
                Description = $"Color mismatch in region ({bounds.minX},{bounds.minY})-({bounds.maxX},{bounds.maxY})",
                Severity = "High",
                Details = new List<string>
                {
                    $"Affected cells: {region.Cells.Count}",
                    $"Background colors: {string.Join(", ", bgColors)}",
                    $"Foreground colors: {string.Join(", ", fgColors)}"
                }
            };
        }
        
        return null;
    }

    private (int minX, int minY, int maxX, int maxY) GetRegionBounds(RegionInfo region)
    {
        var minX = region.Cells.Min(c => c.X);
        var minY = region.Cells.Min(c => c.Y);
        var maxX = region.Cells.Max(c => c.X);
        var maxY = region.Cells.Max(c => c.Y);
        return (minX, minY, maxX, maxY);
    }

    private double CalculateDiffScore(FrameComparison comparison)
    {
        if (comparison.TotalCells == 0) return 0;
        
        // Weighted score: colors are more important than characters
        var colorWeight = _options.ColorMismatchWeight;
        var charWeight = _options.CharMismatchWeight;
        
        var score = (comparison.ColorMismatches * colorWeight + comparison.CharMismatches * charWeight) 
                    / (double)comparison.TotalCells;
        
        return score * 100; // Convert to percentage
    }

    /// <summary>
    /// Analyzes the sequence of frame comparisons for dynamic issues.
    /// </summary>
    private List<ComparisonIssue> AnalyzeSequence(List<FrameComparison> comparisons)
    {
        var issues = new List<ComparisonIssue>();
        
        // Check for oscillations
        var oscillations = DetectOscillations(comparisons);
        issues.AddRange(oscillations);
        
        // Check for color persistence
        var persistenceIssues = DetectColorPersistence(comparisons);
        issues.AddRange(persistenceIssues);
        
        // Check for entropy spikes
        var entropyIssues = DetectEntropySpikes(comparisons);
        issues.AddRange(entropyIssues);
        
        return issues;
    }

    private List<ComparisonIssue> DetectOscillations(List<FrameComparison> comparisons)
    {
        var issues = new List<ComparisonIssue>();
        var windowSize = _options.OscillationWindowSize;
        
        for (int i = 0; i <= comparisons.Count - windowSize; i++)
        {
            var window = comparisons.Skip(i).Take(windowSize).ToList();
            
            // Track cells that flip colors
            var cellFlips = new Dictionary<(int, int), int>();
            
            for (int j = 1; j < window.Count; j++)
            {
                var prev = window[j - 1];
                var curr = window[j];
                
                foreach (var diff in curr.CellDiffs.Where(d => d.ColorMismatch))
                {
                    var key = (diff.X, diff.Y);
                    cellFlips[key] = cellFlips.GetValueOrDefault(key) + 1;
                }
            }
            
            var oscillatingCells = cellFlips.Where(kv => kv.Value >= _options.OscillationThreshold).ToList();
            
            if (oscillatingCells.Any())
            {
                issues.Add(new ComparisonIssue
                {
                    Type = "ColorOscillation",
                    FrameNumber = window.First().ActualFrameNumber,
                    Description = $"Color oscillation detected in {oscillatingCells.Count} cells over {windowSize} frames",
                    Severity = "High",
                    Details = oscillatingCells.Take(5).Select(kv => 
                        $"Cell ({kv.Key.Item1},{kv.Key.Item2}): {kv.Value} flips").ToList()
                });
            }
        }
        
        return issues;
    }

    private List<ComparisonIssue> DetectColorPersistence(List<FrameComparison> comparisons)
    {
        var issues = new List<ComparisonIssue>();
        
        // Track how long colors persist in unexpected places
        var colorLineage = new Dictionary<(int, int), List<string>>();
        
        foreach (var comparison in comparisons)
        {
            foreach (var diff in comparison.CellDiffs.Where(d => d.ColorMismatch))
            {
                var key = (diff.X, diff.Y);
                if (!colorLineage.ContainsKey(key))
                    colorLineage[key] = new List<string>();
                    
                colorLineage[key].Add($"{diff.ActualBg}:{diff.ActualFg}");
            }
        }
        
        // Find cells where unexpected colors persist
        foreach (var kv in colorLineage.Where(kv => kv.Value.Count > _options.PersistenceThreshold))
        {
            var uniqueColors = kv.Value.Distinct().ToList();
            if (uniqueColors.Count == 1) // Same unexpected color persists
            {
                issues.Add(new ComparisonIssue
                {
                    Type = "ColorPersistence",
                    FrameNumber = comparisons.First().ActualFrameNumber,
                    Description = $"Unexpected color persists at ({kv.Key.Item1},{kv.Key.Item2}) for {kv.Value.Count} frames",
                    Severity = "Medium",
                    Details = new List<string> { $"Persistent color: {uniqueColors[0]}" }
                });
            }
        }
        
        return issues;
    }

    private List<ComparisonIssue> DetectEntropySpikes(List<FrameComparison> comparisons)
    {
        var issues = new List<ComparisonIssue>();
        
        // Calculate entropy for each frame
        var entropies = comparisons.Select(c => CalculateColorEntropy(c)).ToList();
        
        if (entropies.Count < 2) return issues;
        
        var mean = entropies.Average();
        var stdDev = Math.Sqrt(entropies.Select(e => Math.Pow(e - mean, 2)).Average());
        
        for (int i = 1; i < entropies.Count; i++)
        {
            var spike = entropies[i] - entropies[i - 1];
            
            if (Math.Abs(spike) > stdDev * _options.EntropySpikeFactor)
            {
                issues.Add(new ComparisonIssue
                {
                    Type = "EntropySpike",
                    FrameNumber = comparisons[i].ActualFrameNumber,
                    Description = $"Sudden color entropy change: {spike:F2} (threshold: {stdDev * _options.EntropySpikeFactor:F2})",
                    Severity = "Medium",
                    Details = new List<string>
                    {
                        $"Previous entropy: {entropies[i-1]:F2}",
                        $"Current entropy: {entropies[i]:F2}"
                    }
                });
            }
        }
        
        return issues;
    }

    private double CalculateColorEntropy(FrameComparison comparison)
    {
        if (comparison.CellDiffs.Count == 0) return 0;
        
        // Count unique color combinations
        var colorCombos = comparison.CellDiffs
            .Select(d => $"{d.ActualBg}:{d.ActualFg}")
            .GroupBy(c => c)
            .Select(g => g.Count() / (double)comparison.CellDiffs.Count)
            .ToList();
        
        // Calculate Shannon entropy
        var entropy = -colorCombos.Sum(p => p * Math.Log(p, 2));
        return entropy;
    }
}

// Supporting classes
public class ComparisonOptions
{
    public double DiffThreshold { get; set; } = 5.0; // 5% of cells
    public double ColorMismatchWeight { get; set; } = 2.0;
    public double CharMismatchWeight { get; set; } = 1.0;
    public int MinRegionSize { get; set; } = 3;
    public int OscillationWindowSize { get; set; } = 3;
    public int OscillationThreshold { get; set; } = 2;
    public int PersistenceThreshold { get; set; } = 5;
    public double EntropySpikeFactor { get; set; } = 2.0;
}

public class ComparisonReport
{
    public int ActualFrames { get; set; }
    public int BaselineFrames { get; set; }
    public DateTime Timestamp { get; set; }
    public List<FrameComparison> FrameComparisons { get; set; } = new();
    public List<ComparisonIssue> Issues { get; set; } = new();
    
    public string GenerateSummary()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Baseline Comparison Report ===");
        sb.AppendLine($"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Frames compared: {FrameComparisons.Count}");
        sb.AppendLine($"Total issues: {Issues.Count}");
        sb.AppendLine();
        
        // Group issues by type
        var byType = Issues.GroupBy(i => i.Type);
        sb.AppendLine("Issues by Type:");
        foreach (var group in byType.OrderByDescending(g => g.Count()))
        {
            sb.AppendLine($"  {group.Key}: {group.Count()}");
        }
        sb.AppendLine();
        
        // High severity issues
        var highSeverity = Issues.Where(i => i.Severity == "High").ToList();
        if (highSeverity.Any())
        {
            sb.AppendLine($"High Severity Issues ({highSeverity.Count}):");
            foreach (var issue in highSeverity.Take(10))
            {
                sb.AppendLine($"  Frame {issue.FrameNumber}: {issue.Description}");
            }
        }
        
        return sb.ToString();
    }
}

public class FrameComparison
{
    public int ActualFrameNumber { get; set; }
    public int BaselineFrameNumber { get; set; }
    public string Action { get; set; } = "";
    public List<CellDiff> CellDiffs { get; set; } = new();
    public int TotalCells { get; set; }
    public int MismatchedCells { get; set; }
    public int ColorMismatches { get; set; }
    public int CharMismatches { get; set; }
    public double DiffScore { get; set; }
    public List<ComparisonIssue> Issues { get; set; } = new();
}

public class CellDiff
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool CharMismatch { get; set; }
    public bool ColorMismatch { get; set; }
    public char ActualChar { get; set; }
    public char BaselineChar { get; set; }
    public string ActualFg { get; set; } = "";
    public string BaselineFg { get; set; } = "";
    public string ActualBg { get; set; } = "";
    public string BaselineBg { get; set; } = "";
    public double ColorDistance { get; set; }
}

public class RegionInfo
{
    public string Id { get; set; } = "";
    public List<CellDiff> Cells { get; set; } = new();
}

public class ComparisonIssue
{
    public string Type { get; set; } = "";
    public int FrameNumber { get; set; }
    public string Description { get; set; } = "";
    public string Severity { get; set; } = "";
    public List<string>? Details { get; set; }
}