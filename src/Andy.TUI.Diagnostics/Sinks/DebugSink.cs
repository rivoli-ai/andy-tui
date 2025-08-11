using System.Diagnostics;

namespace Andy.TUI.Diagnostics.Sinks;

/// <summary>
/// Sink that outputs to System.Diagnostics.Debug for IDE debugging.
/// </summary>
public class DebugSink : ILogSink
{
    public void Write(LogEntry entry)
    {
        Debug.WriteLine(entry.FormattedMessage, entry.Category);
    }
}