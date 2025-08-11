using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Andy.TUI.Diagnostics;

/// <summary>
/// Represents a single log entry with complete context information.
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public LogLevel Level { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? ThreadId { get; init; }
    public string? SourceFile { get; init; }
    public string? SourceMethod { get; init; }
    public int? SourceLine { get; init; }
    public Exception? Exception { get; init; }
    public Dictionary<string, object?> Context { get; init; } = new();
    public string? CorrelationId { get; init; }

    public string FormattedMessage => FormatMessage();

    private string FormatMessage()
    {
        var parts = new List<string>
        {
            $"[{Timestamp:HH:mm:ss.fff}]",
            $"[{Level,-7}]",
            $"[{Category}]"
        };

        if (!string.IsNullOrEmpty(ThreadId))
            parts.Add($"[T:{ThreadId}]");

        if (!string.IsNullOrEmpty(CorrelationId))
            parts.Add($"[C:{CorrelationId}]");

        parts.Add(Message);

        if (SourceFile != null && SourceLine != null)
            parts.Add($"@ {SourceFile}:{SourceLine}");

        if (Exception != null)
        {
            parts.Add($"\n  Exception: {Exception.GetType().Name}: {Exception.Message}");
            if (Exception.StackTrace != null)
                parts.Add($"\n  {Exception.StackTrace.Replace("\n", "\n  ")}");
        }

        if (Context.Count > 0)
        {
            parts.Add("\n  Context:");
            foreach (var kvp in Context)
            {
                parts.Add($"\n    {kvp.Key}: {kvp.Value}");
            }
        }

        return string.Join(" ", parts);
    }
}