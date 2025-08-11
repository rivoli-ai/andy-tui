using System;
using System.Threading;
using System.Threading.Tasks;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Terminal;

/// <summary>
/// Enhanced console input handler that works around Console.KeyAvailable issues
/// in alternate screen mode on macOS/Unix platforms.
/// </summary>
public class EnhancedConsoleInputHandler : IInputHandler
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _inputTask;
    private bool _isRunning;
    private bool _disposed;
    private readonly ILogger _logger;

    public event EventHandler<KeyEventArgs>? KeyPressed;
#pragma warning disable CS0067 // The event is never used
    public event EventHandler<MouseEventArgs>? MouseMoved;
    public event EventHandler<MouseEventArgs>? MousePressed;
    public event EventHandler<MouseEventArgs>? MouseReleased;
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;
#pragma warning restore CS0067 // The event is never used

    public bool SupportsMouseInput => false;

    public EnhancedConsoleInputHandler()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger = DebugContext.Logger.ForCategory("EnhancedConsoleInputHandler");
    }

    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _logger.Info("Starting enhanced console input handler");

        // Use a different approach for reading input that doesn't rely on Console.KeyAvailable
        _inputTask = Task.Run(async () =>
        {
            _logger.Debug("Input task started");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Create a task that reads a key
                    var readKeyTask = Task.Run(() => Console.ReadKey(intercept: true));

                    // Wait for either the key or cancellation
                    var completedTask = await Task.WhenAny(
                        readKeyTask,
                        Task.Delay(50, _cancellationTokenSource.Token)
                    );

                    if (completedTask == readKeyTask && readKeyTask.IsCompletedSuccessfully)
                    {
                        var keyInfo = readKeyTask.Result;
                        _logger.Debug("Read key: {0} (Char: '{1}')", keyInfo.Key, keyInfo.KeyChar);
                        KeyPressed?.Invoke(this, new KeyEventArgs(keyInfo));
                        _logger.Debug("Key event fired");
                    }
                    // If it was the delay task, just continue the loop
                }
                catch (OperationCanceledException)
                {
                    _logger.Debug("Input task cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in input handling: {0}", ex.Message);
                    // Small delay to prevent tight error loop
                    await Task.Delay(10, _cancellationTokenSource.Token);
                }
            }

            _logger.Debug("Input task ended");
        }, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _logger.Info("Stopping enhanced console input handler");
        _cancellationTokenSource.Cancel();

        try
        {
            _inputTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch
        {
            // Ignore exceptions during shutdown
        }
    }

    public void Poll()
    {
        // For console input, polling is handled by the background task
        // This method is here for compatibility with the interface
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _cancellationTokenSource.Dispose();
        _disposed = true;
    }
}