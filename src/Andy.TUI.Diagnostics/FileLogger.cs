using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace Andy.TUI.Core.Diagnostics;

/// <summary>
/// A logger that writes to files with rotation and categorization.
/// </summary>
public class FileLogger : ILogger, IDisposable
{
    private readonly string _baseDirectory;
    private readonly string _category;
    private readonly LogLevel _minLevel;
    private readonly object _lock = new object();
    private StreamWriter? _writer;
    private readonly Timer _flushTimer;
    private readonly ConcurrentDictionary<string, FileLogger> _childLoggers = new();
    
    public FileLogger(string baseDirectory, LogLevel minLevel = LogLevel.Debug, string category = "Root")
    {
        _baseDirectory = baseDirectory;
        _category = category;
        _minLevel = minLevel;
        
        Directory.CreateDirectory(baseDirectory);
        
        // Flush logs every second
        _flushTimer = new Timer(_ => Flush(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        
        InitializeWriter();
    }
    
    private void InitializeWriter()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{_category}_{timestamp}.log";
        var filePath = Path.Combine(_baseDirectory, fileName);
        
        _writer = new StreamWriter(filePath, append: true) { AutoFlush = false };
        _writer.WriteLine($"=== Andy.TUI Debug Log - {_category} ===");
        _writer.WriteLine($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        _writer.WriteLine($"Min Level: {_minLevel}");
        _writer.WriteLine(new string('-', 80));
    }
    
    public void Debug(string message, params object[] args)
    {
        Log(LogLevel.Debug, message, args);
    }
    
    public void Info(string message, params object[] args)
    {
        Log(LogLevel.Info, message, args);
    }
    
    public void Warning(string message, params object[] args)
    {
        Log(LogLevel.Warning, message, args);
    }
    
    public void Error(string message, params object[] args)
    {
        Log(LogLevel.Error, message, args);
    }
    
    public void Error(Exception exception, string message, params object[] args)
    {
        Log(LogLevel.Error, $"{string.Format(message, args)}\nException: {exception}");
    }
    
    public ILogger ForCategory(string category)
    {
        return _childLoggers.GetOrAdd(category, cat => 
            new FileLogger(_baseDirectory, _minLevel, $"{_category}.{cat}"));
    }
    
    private void Log(LogLevel level, string message, params object[] args)
    {
        if (level < _minLevel) return;
        
        try
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            var logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] [{level,-7}] {formattedMessage}";
            
            lock (_lock)
            {
                _writer?.WriteLine(logEntry);
            }
        }
        catch
        {
            // Ignore logging errors to prevent cascading failures
        }
    }
    
    private void Flush()
    {
        lock (_lock)
        {
            _writer?.Flush();
        }
    }
    
    public void Dispose()
    {
        _flushTimer?.Dispose();
        
        lock (_lock)
        {
            _writer?.WriteLine(new string('-', 80));
            _writer?.WriteLine($"Stopped: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            _writer?.Flush();
            _writer?.Dispose();
        }
        
        foreach (var child in _childLoggers.Values)
        {
            child.Dispose();
        }
    }
}