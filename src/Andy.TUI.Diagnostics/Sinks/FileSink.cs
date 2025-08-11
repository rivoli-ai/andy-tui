using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Andy.TUI.Diagnostics.Sinks;

/// <summary>
/// Asynchronous file sink with rotation support.
/// </summary>
public class FileSink : ILogSink, IDisposable
{
    private readonly string _directory;
    private readonly long _maxFileSize;
    private readonly Channel<LogEntry> _channel;
    private readonly CancellationTokenSource _cts;
    private readonly Task _writerTask;
    private StreamWriter? _currentWriter;
    private string? _currentFile;
    private long _currentSize;

    public FileSink(string directory, long maxFileSize = 10 * 1024 * 1024) // 10MB default
    {
        _directory = directory;
        _maxFileSize = maxFileSize;
        Directory.CreateDirectory(directory);

        _channel = Channel.CreateUnbounded<LogEntry>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _cts = new CancellationTokenSource();
        _writerTask = Task.Run(() => ProcessLogsAsync(_cts.Token));
    }

    public void Write(LogEntry entry)
    {
        if (!_channel.Writer.TryWrite(entry))
        {
            // Channel is closed, ignore
        }
    }

    private async Task ProcessLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var entry in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                await WriteToFileAsync(entry);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
        finally
        {
            _currentWriter?.Dispose();
        }
    }

    private async Task WriteToFileAsync(LogEntry entry)
    {
        try
        {
            if (_currentWriter == null || _currentSize > _maxFileSize)
            {
                await RotateFileAsync();
            }

            var line = entry.FormattedMessage;
            await _currentWriter!.WriteLineAsync(line);
            _currentSize += System.Text.Encoding.UTF8.GetByteCount(line + Environment.NewLine);

            // Flush important messages immediately
            if (entry.Level >= LogLevel.Warning)
            {
                await _currentWriter.FlushAsync();
            }
        }
        catch
        {
            // Ignore write failures
        }
    }

    private async Task RotateFileAsync()
    {
        if (_currentWriter != null)
        {
            await _currentWriter.FlushAsync();
            _currentWriter.Dispose();
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
        _currentFile = Path.Combine(_directory, $"andy_tui_{timestamp}.log");
        _currentWriter = new StreamWriter(_currentFile, append: false) { AutoFlush = false };
        _currentSize = 0;

        await _currentWriter.WriteLineAsync($"=== Andy.TUI Log File ===");
        await _currentWriter.WriteLineAsync($"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        await _currentWriter.WriteLineAsync(new string('-', 80));
    }

    public void Dispose()
    {
        _channel.Writer.TryComplete();
        _cts.Cancel();

        try
        {
            _writerTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch
        {
            // Ignore
        }

        _currentWriter?.Dispose();
        _cts.Dispose();
    }
}