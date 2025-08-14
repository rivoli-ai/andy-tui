using System;
using System.Runtime.CompilerServices;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Static logger facade for simplified logging throughout the application.
/// </summary>
public static class Logger
{
    private static ILogger _logger;
    
    static Logger()
    {
        // Initialize with a default logger
        _logger = LogManager.GetLogger("Global");
    }

    /// <summary>
    /// Event raised when a log message is written.
    /// </summary>
    public static event EventHandler<LogMessageEventArgs>? LogMessageWritten;

    public static void Debug(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        _logger.Debug(message, memberName, sourceFilePath, sourceLineNumber);
        LogMessageWritten?.Invoke(null, new LogMessageEventArgs("DEBUG", message));
    }

    public static void Info(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        _logger.Info(message, memberName, sourceFilePath, sourceLineNumber);
        LogMessageWritten?.Invoke(null, new LogMessageEventArgs("INFO", message));
    }

    public static void Warning(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        _logger.Warning(message, memberName, sourceFilePath, sourceLineNumber);
        LogMessageWritten?.Invoke(null, new LogMessageEventArgs("WARNING", message));
    }

    public static void Error(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        _logger.Error(message, memberName, sourceFilePath, sourceLineNumber);
        LogMessageWritten?.Invoke(null, new LogMessageEventArgs("ERROR", message));
    }

    public static void Error(Exception exception, string message = "",
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        var fullMessage = string.IsNullOrEmpty(message) 
            ? exception.ToString() 
            : $"{message}: {exception}";
        _logger.Error(fullMessage, memberName, sourceFilePath, sourceLineNumber);
        LogMessageWritten?.Invoke(null, new LogMessageEventArgs("ERROR", fullMessage));
    }

    public static void Fatal(string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        // Fatal is logged as Error since ILogger doesn't have Fatal
        _logger.Error($"[FATAL] {message}", memberName, sourceFilePath, sourceLineNumber);
        LogMessageWritten?.Invoke(null, new LogMessageEventArgs("FATAL", message));
    }
}

/// <summary>
/// Event args for log messages.
/// </summary>
public class LogMessageEventArgs : EventArgs
{
    public string Level { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }

    public LogMessageEventArgs(string level, string message)
    {
        Level = level;
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}