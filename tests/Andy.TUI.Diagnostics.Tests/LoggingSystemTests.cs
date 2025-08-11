using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Andy.TUI.Tests.Common;

namespace Andy.TUI.Diagnostics.Tests;

public class LoggingSystemTests : TestBase
{
    public LoggingSystemTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void LogManager_ShouldCaptureAllLogLevels()
    {
        LogStep("Testing all log levels");

        var logger = LogManager.GetLogger("TestLogger");

        logger.Debug("Debug message");
        logger.Info("Info message");
        logger.Warning("Warning message");
        logger.Error("Error message");

        var entries = LogManager.GlobalBuffer.GetEntries();

        LogAssertion("All log levels should be captured");
        Assert.Contains(entries, e => e.Level == LogLevel.Debug);
        Assert.Contains(entries, e => e.Level == LogLevel.Info);
        Assert.Contains(entries, e => e.Level == LogLevel.Warning);
        Assert.Contains(entries, e => e.Level == LogLevel.Error);
    }

    [Fact]
    public void LogBuffer_ShouldMaintainHistory()
    {
        LogStep("Testing log buffer history");

        var logger = LogManager.GetLogger("HistoryTest");

        for (int i = 0; i < 100; i++)
        {
            logger.Info($"Message {i}");
        }

        var entries = LogManager.GlobalBuffer.GetEntries();

        LogAssertion("Buffer should contain all 100 messages");
        Assert.True(entries.Count >= 100);
    }

    [Fact]
    public void LogInspector_ShouldGenerateReport()
    {
        LogStep("Testing log inspector report generation");

        var logger = LogManager.GetLogger("InspectorTest");
        logger.Info("Test message 1");
        logger.Warning("Test warning");
        logger.Error("Test error");

        var inspector = new LogInspector();
        var report = inspector.GenerateReport();

        LogData("Report", report);

        LogAssertion("Report should contain statistics");
        Assert.Contains("Statistics", report);
        Assert.Contains("Level Distribution", report);
        Assert.Contains("Recent Errors", report);
    }

    [Fact]
    public void EnhancedLogger_ShouldTrackCorrelationId()
    {
        using (BeginScenario("Correlation ID tracking"))
        {
            var correlationId = Guid.NewGuid().ToString();
            EnhancedLogger.CorrelationId = correlationId;

            var logger = LogManager.GetLogger("CorrelationTest") as EnhancedLogger;
            logger?.Info("Test with correlation");

            var entries = LogManager.GlobalBuffer.GetEntries();
            var lastEntry = entries.LastOrDefault();

            LogAssertion("Entry should have correlation ID");
            Assert.NotNull(lastEntry);
            Assert.Equal(correlationId, lastEntry.CorrelationId);
        }
    }

    [Fact]
    public void LoggingExtensions_ShouldFormatKeyInput()
    {
        LogStep("Testing key input logging");

        var logger = LogManager.GetLogger("KeyInputTest");
        var keyInfo = new ConsoleKeyInfo('A', ConsoleKey.A, false, false, true);

        logger.LogKeyInput(keyInfo, "TestContext");

        var entries = LogManager.GlobalBuffer.GetEntries();
        var lastEntry = entries.LastOrDefault();

        LogAssertion("Key input should be logged with context");
        Assert.NotNull(lastEntry);
        Assert.Contains("Ctrl+A", lastEntry.Message);
        Assert.Contains("TestContext", lastEntry.Message);
    }

    [Fact]
    public void LoggingExtensions_ShouldTrackFocusChanges()
    {
        LogStep("Testing focus change logging");

        var logger = LogManager.GetLogger("FocusTest");
        logger.LogFocusChange("Button1", "TextField2", "Tab key");

        var entries = LogManager.GlobalBuffer.GetEntries();
        var lastEntry = entries.LastOrDefault();

        LogAssertion("Focus change should be logged with reason");
        Assert.NotNull(lastEntry);
        Assert.Contains("Button1", lastEntry.Message);
        Assert.Contains("TextField2", lastEntry.Message);
        Assert.Contains("Tab key", lastEntry.Message);
    }

    [Fact]
    public void FileSink_ShouldWriteToFile()
    {
        LogStep("Testing file sink");

        var tempDir = Path.Combine(Path.GetTempPath(), $"LogTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var logger = new EnhancedLogger("FileSinkTest");
            logger.AddSink(new Sinks.FileSink(tempDir));

            logger.Info("Test message to file");
            logger.Error("Test error to file");

            // Give the async writer time to flush
            System.Threading.Thread.Sleep(100);

            var files = Directory.GetFiles(tempDir, "*.log");

            LogAssertion("Log file should be created");
            Assert.NotEmpty(files);

            var content = File.ReadAllText(files[0]);
            LogData("File content", content);

            Assert.Contains("Test message to file", content);
            Assert.Contains("Test error to file", content);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public void TestBase_ShouldAutomaticallyEnableLogging()
    {
        LogStep("Verifying TestBase automatic logging");

        // This test inherits from TestBase, so logging should already be initialized
        var stats = LogManager.GetStatistics();

        LogAssertion("Logging should be initialized with entries");
        Assert.True(stats.TotalEntries > 0);

        // Verify test context is set
        Assert.Contains("TestClass", EnhancedLogger.ContextData.Keys);
        Assert.Contains("TestMethod", EnhancedLogger.ContextData.Keys);
    }
}