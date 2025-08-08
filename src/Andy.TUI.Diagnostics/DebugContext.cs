using System;
using System.IO;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Provides global debug context and logging for the Andy.TUI framework.
/// </summary>
public static class DebugContext
{
    private static ILogger _logger = NullLogger.Instance;
    private static bool _initialized = false;

    /// <summary>
    /// Gets the global logger instance.
    /// </summary>
    public static ILogger Logger => _logger;

    /// <summary>
    /// Gets whether debug logging is enabled.
    /// </summary>
    public static bool IsEnabled => _logger != NullLogger.Instance;

    /// <summary>
    /// Initializes the debug context. Call this at application startup.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        // Check for debug environment variable
        var debugEnv = Environment.GetEnvironmentVariable("ANDY_TUI_DEBUG");
        // Console.Error.WriteLine($"[Andy.TUI] ANDY_TUI_DEBUG = '{debugEnv}'");
        if (string.IsNullOrEmpty(debugEnv)) return;

        // Parse log level
        var logLevel = LogLevel.Debug;
        if (Enum.TryParse<LogLevel>(debugEnv, true, out var parsedLevel))
        {
            logLevel = parsedLevel;
        }

        // Create log directory
        var logDir = Environment.GetEnvironmentVariable("ANDY_TUI_DEBUG_DIR")
            ?? Path.Combine(Path.GetTempPath(), "andy-tui-debug", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        Directory.CreateDirectory(logDir);

        // Initialize file logger
        _logger = new FileLogger(logDir, logLevel);

        // Log initialization
        _logger.Info("Andy.TUI Debug Logging Initialized");
        _logger.Info("Log Directory: {0}", logDir);
        _logger.Info("Log Level: {0}", logLevel);

        // Log environment info
        _logger.Info("Environment:");
        _logger.Info("  Platform: {0}", Environment.OSVersion.Platform);
        _logger.Info("  .NET Version: {0}", Environment.Version);
        _logger.Info("  Process ID: {0}", Environment.ProcessId);

        // Console.Error.WriteLine($"[Andy.TUI] Debug logging enabled. Logs: {logDir}");
    }

    /// <summary>
    /// Shuts down the debug context and flushes all logs.
    /// </summary>
    public static void Shutdown()
    {
        if (_logger is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _logger = NullLogger.Instance;
    }
}