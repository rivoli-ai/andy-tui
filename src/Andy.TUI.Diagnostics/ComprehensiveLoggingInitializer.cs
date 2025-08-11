using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Andy.TUI.Diagnostics.Sinks;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Provides comprehensive logging initialization for the entire TUI framework.
/// </summary>
public static class ComprehensiveLoggingInitializer
{
    private static readonly Dictionary<Type, ILogger> _loggerCache = new();
    private static bool _initialized = false;

    /// <summary>
    /// Initializes comprehensive logging for the entire framework.
    /// </summary>
    public static void Initialize(bool isTestMode = false, string? customLogPath = null)
    {
        if (_initialized) return;

        var config = isTestMode ? GetTestConfiguration(customLogPath) : GetDefaultConfiguration(customLogPath);
        LogManager.Initialize(config);

        // Set up global correlation ID for request tracking
        EnhancedLogger.CorrelationId = Guid.NewGuid().ToString("N")[..8];

        _initialized = true;

        var logger = LogManager.GetLogger("LoggingSystem");
        logger.Info("=== Andy.TUI Comprehensive Logging Initialized ===");
        logger.Info($"Mode: {(isTestMode ? "TEST" : "NORMAL")}");
        logger.Info($"Log Level: {config.MinLevel}");
        logger.Info($"Log Directory: {config.FileDirectory ?? "default"}");
        logger.Info($"Correlation ID: {EnhancedLogger.CorrelationId}");
    }

    /// <summary>
    /// Gets or creates a logger for a specific type with automatic caching.
    /// </summary>
    public static ILogger GetLogger<T>()
    {
        var type = typeof(T);
        if (!_loggerCache.TryGetValue(type, out var logger))
        {
            logger = LogManager.GetLogger(type.Name);
            _loggerCache[type] = logger;
        }
        return logger;
    }

    /// <summary>
    /// Gets or creates a logger with automatic category detection.
    /// </summary>
    public static ILogger GetLogger([CallerFilePath] string? filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            return LogManager.GetLogger("Unknown");

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        return LogManager.GetLogger(fileName);
    }

    /// <summary>
    /// Injects logging into a component that needs it.
    /// </summary>
    public static void InjectLogging(object component)
    {
        var type = component.GetType();
        var loggerField = type.GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);

        if (loggerField != null && loggerField.FieldType == typeof(ILogger))
        {
            var logger = GetLogger(type.Name);
            loggerField.SetValue(component, logger);
        }
    }

    private static LogConfiguration GetDefaultConfiguration(string? customPath)
    {
        // Use environment-based config by default, but if no env vars are set, use silent
        var hasLoggingEnvVars = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_LEVEL")) ||
                               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_CONSOLE")) ||
                               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_FILE")) ||
                               !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANDY_TUI_LOG_DEBUG"));

        var config = hasLoggingEnvVars ? LogConfiguration.GetFromEnvironment() : LogConfiguration.Silent;

        if (customPath != null)
        {
            config.FileDirectory = customPath;
            config.EnableFile = true; // If custom path provided, enable file logging
        }

        return config;
    }

    private static LogConfiguration GetTestConfiguration(string? customPath)
    {
        var logDir = customPath ?? Path.Combine(
            Directory.GetCurrentDirectory(),
            "TestLogs",
            DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

        return new LogConfiguration
        {
            MinLevel = LogLevel.Debug,
            EnableConsole = false, // Never output to console during tests
            EnableFile = true,
            EnableDebug = true,
            FileDirectory = logDir,
            MaxFileSize = 100 * 1024 * 1024, // 100MB for test logs
            ConsoleUseColors = false,
            ConsoleUseStderr = true
        };
    }

    /// <summary>
    /// Creates a test session with comprehensive logging.
    /// </summary>
    public static IDisposable BeginTestSession(string testName)
    {
        Initialize(isTestMode: true);
        EnhancedLogger.CorrelationId = $"TEST_{testName}_{Guid.NewGuid():N}"[..20];

        var logger = LogManager.GetLogger("TestFramework");
        logger.Info($"=== BEGIN TEST: {testName} ===");

        return new TestSession(testName);
    }

    private class TestSession : IDisposable
    {
        private readonly string _testName;
        private readonly DateTime _startTime;

        public TestSession(string testName)
        {
            _testName = testName;
            _startTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            var duration = DateTime.UtcNow - _startTime;
            var logger = LogManager.GetLogger("TestFramework");
            logger.Info($"=== END TEST: {_testName} (Duration: {duration.TotalMilliseconds:F2}ms) ===");

            // Export test logs if there were errors
            var stats = LogManager.GetStatistics();
            if (stats.LevelCounts.GetValueOrDefault(LogLevel.Error, 0) > 0)
            {
                var exportPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "TestLogs",
                    "Failures",
                    $"{_testName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

                Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
                LogManager.ExportLogs(exportPath, since: _startTime);
                logger.Warning($"Test had errors. Log exported to: {exportPath}");
            }
        }
    }

    /// <summary>
    /// Adds comprehensive logging to all framework components.
    /// </summary>
    public static void EnableFrameworkLogging()
    {
        // This would be called once at application startup
        Initialize();

        // Log framework initialization
        var logger = LogManager.GetLogger("Framework");
        logger.Info("Andy.TUI Framework logging enabled");
        logger.Info($"Framework Version: {Assembly.GetExecutingAssembly().GetName().Version}");
        logger.Info($"Runtime: {Environment.Version}");
        logger.Info($"OS: {Environment.OSVersion}");
        logger.Info($"Machine: {Environment.MachineName}");
        logger.Info($"Processors: {Environment.ProcessorCount}");
    }
}