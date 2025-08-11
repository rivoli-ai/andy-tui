using System;

namespace Andy.TUI.Diagnostics.Sinks;

/// <summary>
/// Console sink for debugging output.
/// </summary>
public class ConsoleSink : ILogSink
{
    private readonly bool _useColors;
    private readonly bool _useStderr;

    public ConsoleSink(bool useColors = true, bool useStderr = true)
    {
        _useColors = useColors;
        _useStderr = useStderr;
    }

    public void Write(LogEntry entry)
    {
        var output = _useStderr ? Console.Error : Console.Out;

        if (_useColors && !Console.IsOutputRedirected)
        {
            var originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = GetColorForLevel(entry.Level);
                output.WriteLine(entry.FormattedMessage);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        else
        {
            output.WriteLine(entry.FormattedMessage);
        }
    }

    private ConsoleColor GetColorForLevel(LogLevel level) => level switch
    {
        LogLevel.Debug => ConsoleColor.Gray,
        LogLevel.Info => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        _ => ConsoleColor.White
    };
}