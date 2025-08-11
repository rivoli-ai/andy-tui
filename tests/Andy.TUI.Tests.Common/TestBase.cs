using System;
using System.IO;
using System.Runtime.CompilerServices;
using Andy.TUI.Diagnostics;
using Xunit.Abstractions;

namespace Andy.TUI.Tests.Common;

/// <summary>
/// Base class for all tests with comprehensive logging support.
/// </summary>
public abstract class TestBase : IDisposable
{
    private readonly IDisposable? _testSession;
    private readonly ITestOutputHelper? _output;
    protected readonly ILogger Logger;

    protected TestBase(ITestOutputHelper? output = null, [CallerMemberName] string? testName = null)
    {
        _output = output;

        // Initialize comprehensive logging for tests
        ComprehensiveLoggingInitializer.Initialize(isTestMode: true);

        // Create a test session with unique correlation ID
        testName ??= GetType().Name;
        _testSession = ComprehensiveLoggingInitializer.BeginTestSession(testName);

        // Get logger for this test class
        Logger = LogManager.GetLogger(GetType());

        // Log test start
        Logger.Info($"=== Starting Test: {testName} ===");

        // Set up context data
        EnhancedLogger.ContextData["TestClass"] = GetType().Name;
        EnhancedLogger.ContextData["TestMethod"] = testName;
    }

    /// <summary>
    /// Logs a test step for clarity in logs.
    /// </summary>
    protected void LogStep(string step, params object[] args)
    {
        var message = args.Length > 0 ? string.Format(step, args) : step;
        Logger.Info($"STEP: {message}");
        _output?.WriteLine($"STEP: {message}");
    }

    /// <summary>
    /// Logs an assertion for clarity in logs.
    /// </summary>
    protected void LogAssertion(string assertion, params object[] args)
    {
        var message = args.Length > 0 ? string.Format(assertion, args) : assertion;
        Logger.Info($"ASSERT: {message}");
        _output?.WriteLine($"ASSERT: {message}");
    }

    /// <summary>
    /// Logs test data for debugging.
    /// </summary>
    protected void LogData(string name, object? value)
    {
        Logger.Debug($"DATA: {name} = {value}");
        _output?.WriteLine($"DATA: {name} = {value}");
    }

    /// <summary>
    /// Creates a scope for a specific test scenario.
    /// </summary>
    protected IDisposable BeginScenario(string scenario)
    {
        Logger.Info($">>> BEGIN SCENARIO: {scenario}");
        _output?.WriteLine($">>> BEGIN SCENARIO: {scenario}");

        return new ScenarioScope(Logger, _output, scenario);
    }

    /// <summary>
    /// Exports the test logs if there were failures.
    /// </summary>
    protected void ExportLogsOnFailure(Exception? exception = null)
    {
        if (exception != null)
        {
            Logger.Error(exception, "Test failed with exception");

            var testName = EnhancedLogger.ContextData["TestMethod"]?.ToString() ?? "Unknown";
            var exportPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TestLogs",
                "Failures",
                $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
            LogManager.ExportLogs(exportPath);

            _output?.WriteLine($"Test logs exported to: {exportPath}");
        }
    }

    /// <summary>
    /// Gets a summary of the test execution.
    /// </summary>
    protected string GetTestSummary()
    {
        var inspector = new LogInspector();
        return inspector.GenerateReport(includeRecentLogs: true);
    }

    public virtual void Dispose()
    {
        Logger.Info("=== Test Completed ===");

        // Get test statistics
        var stats = LogManager.GetStatistics();
        if (stats.LevelCounts.GetValueOrDefault(LogLevel.Error, 0) > 0)
        {
            _output?.WriteLine($"Test had {stats.LevelCounts[LogLevel.Error]} errors");
            _output?.WriteLine(LogManager.GetErrorSummary());
        }

        _testSession?.Dispose();
    }

    private class ScenarioScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ITestOutputHelper? _output;
        private readonly string _scenario;
        private readonly DateTime _startTime;

        public ScenarioScope(ILogger logger, ITestOutputHelper? output, string scenario)
        {
            _logger = logger;
            _output = output;
            _scenario = scenario;
            _startTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            var duration = DateTime.UtcNow - _startTime;
            _logger.Info($"<<< END SCENARIO: {_scenario} (Duration: {duration.TotalMilliseconds:F2}ms)");
            _output?.WriteLine($"<<< END SCENARIO: {_scenario} (Duration: {duration.TotalMilliseconds:F2}ms)");
        }
    }
}