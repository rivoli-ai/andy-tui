using System;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Defines the interface for logging in the Andy.TUI framework.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    void Debug(string message, params object[] args);
    
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    void Info(string message, params object[] args);
    
    /// <summary>
    /// Logs a warning message.
    /// </summary>
    void Warning(string message, params object[] args);
    
    /// <summary>
    /// Logs an error message.
    /// </summary>
    void Error(string message, params object[] args);
    
    /// <summary>
    /// Logs an error with exception details.
    /// </summary>
    void Error(Exception exception, string message, params object[] args);
    
    /// <summary>
    /// Creates a child logger with a specific category.
    /// </summary>
    ILogger ForCategory(string category);
}

/// <summary>
/// Log levels for the framework.
/// </summary>
public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    None = 4
}