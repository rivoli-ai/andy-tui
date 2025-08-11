using System;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Configuration for the logging system.
/// </summary>
public class LogConfiguration
{
    public LogLevel MinLevel { get; set; } = LogLevel.Warning;
    public bool EnableConsole { get; set; } = false;
    public bool EnableFile { get; set; } = false;
    public bool EnableDebug { get; set; } = false;
    public bool ConsoleUseColors { get; set; } = true;
    public bool ConsoleUseStderr { get; set; } = false;
    public string? FileDirectory { get; set; }
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Gets the default configuration based on environment variables.
    /// </summary>
    public static LogConfiguration Default => GetFromEnvironment();

    /// <summary>
    /// Creates configuration from environment variables.
    /// </summary>
    public static LogConfiguration GetFromEnvironment()
    {
        var config = new LogConfiguration();
        
        // Check environment variables
        var logLevel = Environment.GetEnvironmentVariable("ANDY_TUI_LOG_LEVEL");
        if (!string.IsNullOrEmpty(logLevel) && Enum.TryParse<LogLevel>(logLevel, true, out var level))
        {
            config.MinLevel = level;
        }

        // Enable console logging if LOG_LEVEL is set or ANDY_TUI_LOG_CONSOLE=true
        config.EnableConsole = !string.IsNullOrEmpty(logLevel) ||
                              string.Equals(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_CONSOLE"), "true", StringComparison.OrdinalIgnoreCase);

        // Enable file logging if ANDY_TUI_LOG_FILE=true
        config.EnableFile = string.Equals(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_FILE"), "true", StringComparison.OrdinalIgnoreCase);

        // Enable debug logging if ANDY_TUI_LOG_DEBUG=true
        config.EnableDebug = string.Equals(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_DEBUG"), "true", StringComparison.OrdinalIgnoreCase);

        // Console stderr usage
        config.ConsoleUseStderr = string.Equals(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_STDERR"), "true", StringComparison.OrdinalIgnoreCase);

        // Custom file directory
        var fileDir = Environment.GetEnvironmentVariable("ANDY_TUI_LOG_DIR");
        if (!string.IsNullOrEmpty(fileDir))
        {
            config.FileDirectory = fileDir;
        }

        return config;
    }

    /// <summary>
    /// Configuration for tests - enables all logging to debug level.
    /// </summary>
    public static LogConfiguration ForTesting(string? customLogPath = null)
    {
        return new LogConfiguration
        {
            MinLevel = LogLevel.Debug,
            EnableConsole = false, // Don't spam test output
            EnableFile = true,
            EnableDebug = true,
            FileDirectory = customLogPath
        };
    }

    /// <summary>
    /// Silent configuration - no logging.
    /// </summary>
    public static LogConfiguration Silent => new LogConfiguration
    {
        MinLevel = LogLevel.None,
        EnableConsole = false,
        EnableFile = false,
        EnableDebug = false
    };
}