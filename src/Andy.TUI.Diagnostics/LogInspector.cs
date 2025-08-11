using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Tool for inspecting and analyzing log data.
/// </summary>
public class LogInspector
{
    private readonly LogBuffer _buffer;

    public LogInspector(LogBuffer? buffer = null)
    {
        _buffer = buffer ?? LogManager.GlobalBuffer;
    }

    /// <summary>
    /// Generates a detailed report of the current log state.
    /// </summary>
    public string GenerateReport(bool includeRecentLogs = true)
    {
        var sb = new StringBuilder();
        var stats = _buffer.GetStatistics();

        sb.AppendLine("=== Andy.TUI Log Report ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        sb.AppendLine("=== Statistics ===");
        sb.AppendLine($"Total Entries: {stats.TotalEntries:N0}");
        sb.AppendLine($"Current Buffer: {stats.CurrentEntries:N0}");

        if (stats.OldestEntry.HasValue && stats.NewestEntry.HasValue)
        {
            sb.AppendLine($"Time Range: {stats.OldestEntry:HH:mm:ss} - {stats.NewestEntry:HH:mm:ss}");
            sb.AppendLine($"Duration: {(stats.NewestEntry.Value - stats.OldestEntry.Value).TotalSeconds:F1}s");
        }

        sb.AppendLine();
        sb.AppendLine("=== Level Distribution ===");
        foreach (var kvp in stats.LevelCounts.OrderBy(x => x.Key))
        {
            var percentage = stats.TotalEntries > 0
                ? (kvp.Value * 100.0 / stats.TotalEntries)
                : 0;
            sb.AppendLine($"{kvp.Key,-8}: {kvp.Value,6:N0} ({percentage:F1}%)");
        }

        sb.AppendLine();
        sb.AppendLine("=== Top Categories ===");
        foreach (var kvp in stats.CategoryCounts.OrderByDescending(x => x.Value).Take(10))
        {
            sb.AppendLine($"{kvp.Key,-30}: {kvp.Value,6:N0}");
        }

        if (includeRecentLogs)
        {
            sb.AppendLine();
            sb.AppendLine("=== Recent Errors ===");
            var errors = _buffer.GetEntries(minLevel: LogLevel.Error, limit: 5);
            foreach (var error in errors)
            {
                sb.AppendLine($"{error.Timestamp:HH:mm:ss} [{error.Category}] {error.Message}");
                if (error.Exception != null)
                {
                    sb.AppendLine($"  -> {error.Exception.GetType().Name}: {error.Exception.Message}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("=== Recent Warnings ===");
            var warnings = _buffer.GetEntries(minLevel: LogLevel.Warning, limit: 5)
                .Where(e => e.Level == LogLevel.Warning);
            foreach (var warning in warnings)
            {
                sb.AppendLine($"{warning.Timestamp:HH:mm:ss} [{warning.Category}] {warning.Message}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Analyzes patterns in log entries.
    /// </summary>
    public LogAnalysis AnalyzeLogs(DateTime? since = null)
    {
        var entries = _buffer.GetEntries(since: since);
        var analysis = new LogAnalysis();

        if (!entries.Any())
        {
            return analysis;
        }

        // Error patterns
        var errorGroups = entries
            .Where(e => e.Level == LogLevel.Error)
            .GroupBy(e => e.Exception?.GetType().Name ?? "General")
            .OrderByDescending(g => g.Count());

        foreach (var group in errorGroups)
        {
            analysis.ErrorPatterns[group.Key] = group.Count();
        }

        // Performance issues (find operations taking > 100ms)
        var performanceLogs = entries
            .Where(e => e.Message.Contains("ms") && TryExtractMilliseconds(e.Message, out var ms) && ms > 100)
            .OrderByDescending(e => ExtractMilliseconds(e.Message));

        foreach (var log in performanceLogs.Take(10))
        {
            analysis.SlowOperations.Add(new SlowOperation
            {
                Timestamp = log.Timestamp,
                Category = log.Category,
                Operation = log.Message,
                Duration = ExtractMilliseconds(log.Message)
            });
        }

        // Frequency analysis
        var timeGroups = entries
            .GroupBy(e => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day,
                                      e.Timestamp.Hour, e.Timestamp.Minute, 0))
            .OrderBy(g => g.Key);

        foreach (var group in timeGroups)
        {
            analysis.LogFrequency[group.Key] = group.Count();
        }

        // Category hotspots
        var categoryActivity = entries
            .GroupBy(e => e.Category)
            .Select(g => new { Category = g.Key, Count = g.Count(), Errors = g.Count(e => e.Level == LogLevel.Error) })
            .OrderByDescending(x => x.Errors)
            .ThenByDescending(x => x.Count);

        foreach (var cat in categoryActivity.Take(10))
        {
            analysis.HotCategories.Add(new CategoryHotspot
            {
                Category = cat.Category,
                TotalLogs = cat.Count,
                ErrorCount = cat.Errors
            });
        }

        return analysis;
    }

    /// <summary>
    /// Exports specific log entries matching criteria.
    /// </summary>
    public void ExportFiltered(
        string filePath,
        LogLevel? minLevel = null,
        string? category = null,
        string? searchText = null,
        DateTime? since = null,
        DateTime? until = null)
    {
        var entries = _buffer.GetEntries(
            since: since,
            minLevel: minLevel,
            category: category,
            searchText: searchText);

        if (until.HasValue)
        {
            entries = entries.Where(e => e.Timestamp <= until.Value).ToList();
        }

        using var writer = new StreamWriter(filePath);
        writer.WriteLine($"=== Filtered Log Export ===");
        writer.WriteLine($"Criteria:");
        if (minLevel.HasValue) writer.WriteLine($"  Min Level: {minLevel}");
        if (!string.IsNullOrEmpty(category)) writer.WriteLine($"  Category: {category}");
        if (!string.IsNullOrEmpty(searchText)) writer.WriteLine($"  Search: {searchText}");
        if (since.HasValue) writer.WriteLine($"  Since: {since:yyyy-MM-dd HH:mm:ss}");
        if (until.HasValue) writer.WriteLine($"  Until: {until:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine($"Matches: {entries.Count}");
        writer.WriteLine(new string('=', 80));

        foreach (var entry in entries)
        {
            writer.WriteLine(entry.FormattedMessage);
        }
    }

    private bool TryExtractMilliseconds(string message, out int milliseconds)
    {
        milliseconds = 0;
        var parts = message.Split(' ');
        foreach (var part in parts)
        {
            if (part.EndsWith("ms") && int.TryParse(part[..^2], out milliseconds))
            {
                return true;
            }
        }
        return false;
    }

    private int ExtractMilliseconds(string message)
    {
        TryExtractMilliseconds(message, out var ms);
        return ms;
    }
}

public class LogAnalysis
{
    public Dictionary<string, int> ErrorPatterns { get; } = new();
    public List<SlowOperation> SlowOperations { get; } = new();
    public Dictionary<DateTime, int> LogFrequency { get; } = new();
    public List<CategoryHotspot> HotCategories { get; } = new();
}

public class SlowOperation
{
    public DateTime Timestamp { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public int Duration { get; init; }
}

public class CategoryHotspot
{
    public string Category { get; init; } = string.Empty;
    public int TotalLogs { get; init; }
    public int ErrorCount { get; init; }
}