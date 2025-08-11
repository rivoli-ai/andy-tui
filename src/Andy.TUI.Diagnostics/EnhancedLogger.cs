using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Enhanced logger with comprehensive debugging features.
/// </summary>
public class EnhancedLogger : ILogger, IDisposable
{
    private readonly string _category;
    private readonly LogLevel _minLevel;
    private readonly List<ILogSink> _sinks = new();
    private readonly ConcurrentDictionary<string, EnhancedLogger> _childLoggers = new();
    private readonly LogBuffer _buffer;
    private static readonly AsyncLocal<string> _correlationId = new();
    private static readonly AsyncLocal<Dictionary<string, object?>> _contextData = new();
    
    public static string? CorrelationId
    {
        get => _correlationId.Value;
        set => _correlationId.Value = value!;
    }
    
    public static Dictionary<string, object?> ContextData =>
        _contextData.Value ??= new Dictionary<string, object?>();
    
    public LogBuffer Buffer => _buffer;
    
    public EnhancedLogger(
        string category = "Root",
        LogLevel minLevel = LogLevel.Debug,
        LogBuffer? sharedBuffer = null)
    {
        _category = category;
        _minLevel = minLevel;
        _buffer = sharedBuffer ?? new LogBuffer();
    }
    
    public void AddSink(ILogSink sink)
    {
        _sinks.Add(sink);
    }
    
    public void Debug(string message, params object[] args) =>
        Log(LogLevel.Debug, message, args);
    
    public void Info(string message, params object[] args) =>
        Log(LogLevel.Info, message, args);
    
    public void Warning(string message, params object[] args) =>
        Log(LogLevel.Warning, message, args);
    
    public void Error(string message, params object[] args) =>
        Log(LogLevel.Error, message, args);
    
    public void Error(Exception exception, string message, params object[] args) =>
        LogWithException(LogLevel.Error, exception, message, args);
    
    public ILogger ForCategory(string category)
    {
        return _childLoggers.GetOrAdd(category, cat =>
        {
            var childLogger = new EnhancedLogger($"{_category}.{cat}", _minLevel, _buffer);
            foreach (var sink in _sinks)
            {
                childLogger.AddSink(sink);
            }
            return childLogger;
        });
    }
    
    public void Trace(
        string message,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0,
        params object[] args)
    {
        LogInternal(LogLevel.Debug, message, args, null, file, member, line);
    }
    
    public IDisposable BeginScope(string name, params (string Key, object? Value)[] properties)
    {
        var scope = new LogScope(name);
        foreach (var (key, value) in properties)
        {
            ContextData[key] = value;
        }
        return scope;
    }
    
    public IDisposable MeasureTime(string operation)
    {
        return new TimeMeasurement(this, operation);
    }
    
    private void Log(LogLevel level, string message, params object[] args)
    {
        LogInternal(level, message, args);
    }
    
    private void LogWithException(LogLevel level, Exception exception, string message, params object[] args)
    {
        LogInternal(level, message, args, exception);
    }
    
    private void LogInternal(
        LogLevel level,
        string message,
        object[]? args = null,
        Exception? exception = null,
        [CallerFilePath] string? file = null,
        [CallerMemberName] string? member = null,
        [CallerLineNumber] int line = 0)
    {
        if (level < _minLevel) return;
        
        try
        {
            var formattedMessage = args?.Length > 0 ? string.Format(message, args) : message;
            
            var entry = new LogEntry
            {
                Level = level,
                Category = _category,
                Message = formattedMessage,
                ThreadId = Thread.CurrentThread.ManagedThreadId.ToString(),
                SourceFile = Path.GetFileName(file),
                SourceMethod = member,
                SourceLine = line,
                Exception = exception,
                CorrelationId = CorrelationId,
                Context = new Dictionary<string, object?>(ContextData)
            };
            
            _buffer.Add(entry);
            
            foreach (var sink in _sinks)
            {
                try
                {
                    sink.Write(entry);
                }
                catch
                {
                    // Ignore sink failures
                }
            }
        }
        catch
        {
            // Ignore logging failures
        }
    }
    
    public void Dispose()
    {
        foreach (var sink in _sinks)
        {
            if (sink is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        
        foreach (var child in _childLoggers.Values)
        {
            child.Dispose();
        }
    }
    
    private class LogScope : IDisposable
    {
        private readonly Dictionary<string, object?> _originalContext;
        
        public LogScope(string name)
        {
            _originalContext = new Dictionary<string, object?>(ContextData);
            ContextData["Scope"] = name;
        }
        
        public void Dispose()
        {
            ContextData.Clear();
            foreach (var kvp in _originalContext)
            {
                ContextData[kvp.Key] = kvp.Value;
            }
        }
    }
    
    private class TimeMeasurement : IDisposable
    {
        private readonly EnhancedLogger _logger;
        private readonly string _operation;
        private readonly Stopwatch _stopwatch;
        
        public TimeMeasurement(EnhancedLogger logger, string operation)
        {
            _logger = logger;
            _operation = operation;
            _stopwatch = Stopwatch.StartNew();
            _logger.Debug($"Starting {operation}");
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Debug($"Completed {_operation} in {_stopwatch.ElapsedMilliseconds}ms");
        }
    }
}

/// <summary>
/// Interface for log output destinations.
/// </summary>
public interface ILogSink
{
    void Write(LogEntry entry);
}