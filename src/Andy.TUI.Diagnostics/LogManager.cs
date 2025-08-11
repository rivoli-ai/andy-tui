using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Andy.TUI.Diagnostics.Sinks;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Central logging manager for the entire application.
/// </summary>
public static class LogManager
{
    private static readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private static readonly LogBuffer _globalBuffer = new(maxEntries: 100000);
    private static LogLevel _defaultLevel = LogLevel.Debug;
    private static readonly List<ILogSink> _globalSinks = new();
    private static bool _initialized = false;
    private static readonly object _initLock = new();
    
    /// <summary>
    /// Gets the global log buffer containing all log entries.
    /// </summary>
    public static LogBuffer GlobalBuffer => _globalBuffer;
    
    /// <summary>
    /// Gets or sets the default minimum log level.
    /// </summary>
    public static LogLevel DefaultLevel
    {
        get => _defaultLevel;
        set
        {
            _defaultLevel = value;
            // Update existing loggers if needed
        }
    }
    
    /// <summary>
    /// Initializes the logging system with default or custom configuration.
    /// </summary>
    public static void Initialize(LogConfiguration? config = null)
    {
        lock (_initLock)
        {
            if (_initialized) return;
            
            config ??= LogConfiguration.Default;
            _defaultLevel = config.MinLevel;
            
            // Add configured sinks
            if (config.EnableConsole)
            {
                _globalSinks.Add(new ConsoleSink(
                    useColors: config.ConsoleUseColors,
                    useStderr: config.ConsoleUseStderr));
            }
            
            if (config.EnableFile)
            {
                var logDir = config.FileDirectory ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Andy.TUI",
                    "Logs");
                    
                _globalSinks.Add(new FileSink(logDir, config.MaxFileSize));
            }
            
            if (config.EnableDebug && System.Diagnostics.Debugger.IsAttached)
            {
                _globalSinks.Add(new DebugSink());
            }
            
            _initialized = true;
        }
    }
    
    /// <summary>
    /// Gets a logger for the specified category.
    /// </summary>
    public static ILogger GetLogger(string category)
    {
        if (!_initialized)
        {
            Initialize();
        }
        
        return _loggers.GetOrAdd(category, cat =>
        {
            var logger = new EnhancedLogger(cat, _defaultLevel, _globalBuffer);
            foreach (var sink in _globalSinks)
            {
                logger.AddSink(sink);
            }
            return logger;
        });
    }
    
    /// <summary>
    /// Gets a logger for the specified type.
    /// </summary>
    public static ILogger GetLogger<T>() => GetLogger(typeof(T).Name);
    
    /// <summary>
    /// Gets a logger for the specified type.
    /// </summary>
    public static ILogger GetLogger(Type type) => GetLogger(type.Name);
    
    /// <summary>
    /// Exports log entries to a file.
    /// </summary>
    public static void ExportLogs(string filePath, DateTime? since = null, LogLevel? minLevel = null)
    {
        var entries = _globalBuffer.GetEntries(since: since, minLevel: minLevel);
        
        using var writer = new StreamWriter(filePath, append: false);
        writer.WriteLine($"=== Andy.TUI Log Export ===");
        writer.WriteLine($"Exported: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        writer.WriteLine($"Total Entries: {entries.Count}");
        writer.WriteLine(new string('=', 80));
        
        foreach (var entry in entries)
        {
            writer.WriteLine(entry.FormattedMessage);
        }
    }
    
    /// <summary>
    /// Exports log entries as JSON.
    /// </summary>
    public static void ExportLogsAsJson(string filePath, DateTime? since = null, LogLevel? minLevel = null)
    {
        var entries = _globalBuffer.GetEntries(since: since, minLevel: minLevel);
        var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        File.WriteAllText(filePath, json);
    }
    
    /// <summary>
    /// Gets log statistics.
    /// </summary>
    public static LogStatistics GetStatistics() => _globalBuffer.GetStatistics();
    
    /// <summary>
    /// Searches logs for specific text.
    /// </summary>
    public static IReadOnlyList<LogEntry> SearchLogs(
        string searchText,
        DateTime? since = null,
        LogLevel? minLevel = null,
        string? category = null,
        int? limit = null)
    {
        return _globalBuffer.GetEntries(
            since: since,
            minLevel: minLevel,
            category: category,
            searchText: searchText,
            limit: limit);
    }
    
    /// <summary>
    /// Clears all log entries from the buffer.
    /// </summary>
    public static void ClearLogs()
    {
        _globalBuffer.Clear();
    }
    
    /// <summary>
    /// Gets a summary of recent errors.
    /// </summary>
    public static string GetErrorSummary(int maxErrors = 10)
    {
        var errors = _globalBuffer.GetEntries(minLevel: LogLevel.Error, limit: maxErrors);
        var sb = new StringBuilder();
        sb.AppendLine($"=== Recent Errors ({errors.Count}) ===");
        
        foreach (var error in errors)
        {
            sb.AppendLine($"{error.Timestamp:HH:mm:ss} [{error.Category}] {error.Message}");
            if (error.Exception != null)
            {
                sb.AppendLine($"  Exception: {error.Exception.GetType().Name}: {error.Exception.Message}");
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Shutdown the logging system and flush all sinks.
    /// </summary>
    public static void Shutdown()
    {
        lock (_initLock)
        {
            foreach (var sink in _globalSinks)
            {
                if (sink is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _globalSinks.Clear();
            _loggers.Clear();
            _initialized = false;
        }
    }
}

/// <summary>
/// Configuration for the logging system.
/// </summary>
public class LogConfiguration
{
    public LogLevel MinLevel { get; set; } = LogLevel.Debug;
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = true;
    public bool EnableDebug { get; set; } = true;
    public bool ConsoleUseColors { get; set; } = true;
    public bool ConsoleUseStderr { get; set; } = true;
    public string? FileDirectory { get; set; }
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    
    public static LogConfiguration Default => new();
    
    public static LogConfiguration Testing => new()
    {
        MinLevel = LogLevel.Debug,
        EnableConsole = false, // No console output during tests
        EnableFile = true,
        EnableDebug = true,
        FileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestLogs")
    };
    
    public static LogConfiguration Production => new()
    {
        MinLevel = LogLevel.Info,
        EnableConsole = false,
        EnableFile = true,
        EnableDebug = false
    };
}