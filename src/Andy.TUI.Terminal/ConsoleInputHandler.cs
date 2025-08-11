using Andy.TUI.Diagnostics;

namespace Andy.TUI.Terminal;

/// <summary>
/// Basic console input handler that supports keyboard input.
/// </summary>
public class ConsoleInputHandler : IInputHandler
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

    public ConsoleInputHandler()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _logger = DebugContext.Logger.ForCategory("ConsoleInputHandler");
    }

    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _logger.Info("Starting console input handler");

        // Start a background task to read console input
        _inputTask = Task.Run(async () =>
        {
            _logger.Debug("Input task started");
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    // Check if key is available without blocking
                    if (Console.KeyAvailable)
                    {
                        _logger.Debug("Key available");
                        var keyInfo = Console.ReadKey(intercept: true);
                        _logger.Debug("Read key: {0} (Char: '{1}')", keyInfo.Key, keyInfo.KeyChar);
                        KeyPressed?.Invoke(this, new KeyEventArgs(keyInfo));
                        _logger.Debug("Key event fired");
                    }
                    else
                    {
                        // Small delay to prevent busy waiting
                        await Task.Delay(10, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error("Error in input handling: {0}", ex.Message);
                }
            }
        }, _cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
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