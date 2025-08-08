using System;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// A no-op logger for when logging is disabled.
/// </summary>
public class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new NullLogger();
    
    private NullLogger() { }
    
    public void Debug(string message, params object[] args) { }
    public void Info(string message, params object[] args) { }
    public void Warning(string message, params object[] args) { }
    public void Error(string message, params object[] args) { }
    public void Error(Exception exception, string message, params object[] args) { }
    
    public ILogger ForCategory(string category) => this;
}