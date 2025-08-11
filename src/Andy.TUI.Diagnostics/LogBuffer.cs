using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Thread-safe circular buffer for storing log entries in memory.
/// </summary>
public class LogBuffer
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private readonly int _maxEntries;
    private int _totalEntries;
    private readonly object _statsLock = new();
    private readonly Dictionary<LogLevel, int> _levelCounts = new();
    private readonly Dictionary<string, int> _categoryCounts = new();
    
    public LogBuffer(int maxEntries = 100000)
    {
        _maxEntries = maxEntries;
        foreach (LogLevel level in Enum.GetValues<LogLevel>())
        {
            _levelCounts[level] = 0;
        }
    }
    
    public void Add(LogEntry entry)
    {
        _entries.Enqueue(entry);
        Interlocked.Increment(ref _totalEntries);
        
        lock (_statsLock)
        {
            _levelCounts[entry.Level]++;
            _categoryCounts[entry.Category] = _categoryCounts.GetValueOrDefault(entry.Category, 0) + 1;
        }
        
        // Trim if we exceed max entries
        while (_entries.Count > _maxEntries)
        {
            _entries.TryDequeue(out _);
        }
    }
    
    public IReadOnlyList<LogEntry> GetEntries(
        DateTime? since = null,
        LogLevel? minLevel = null,
        string? category = null,
        string? searchText = null,
        int? limit = null)
    {
        var query = _entries.AsEnumerable();
        
        if (since.HasValue)
            query = query.Where(e => e.Timestamp >= since.Value);
            
        if (minLevel.HasValue)
            query = query.Where(e => e.Level >= minLevel.Value);
            
        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
            
        if (!string.IsNullOrEmpty(searchText))
            query = query.Where(e => 
                e.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                e.Exception?.Message.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true);
                
        if (limit.HasValue)
            query = query.TakeLast(limit.Value);
            
        return query.ToList();
    }
    
    public void Clear()
    {
        while (_entries.TryDequeue(out _)) { }
        lock (_statsLock)
        {
            foreach (var key in _levelCounts.Keys.ToList())
            {
                _levelCounts[key] = 0;
            }
            _categoryCounts.Clear();
        }
        _totalEntries = 0;
    }
    
    public LogStatistics GetStatistics()
    {
        lock (_statsLock)
        {
            return new LogStatistics
            {
                TotalEntries = _totalEntries,
                CurrentEntries = _entries.Count,
                LevelCounts = new Dictionary<LogLevel, int>(_levelCounts),
                CategoryCounts = new Dictionary<string, int>(_categoryCounts),
                OldestEntry = _entries.FirstOrDefault()?.Timestamp,
                NewestEntry = _entries.LastOrDefault()?.Timestamp
            };
        }
    }
}

public class LogStatistics
{
    public int TotalEntries { get; init; }
    public int CurrentEntries { get; init; }
    public Dictionary<LogLevel, int> LevelCounts { get; init; } = new();
    public Dictionary<string, int> CategoryCounts { get; init; } = new();
    public DateTime? OldestEntry { get; init; }
    public DateTime? NewestEntry { get; init; }
}