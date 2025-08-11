using System;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Terminal;

/// <summary>
/// Input handler that polls for input in the main thread.
/// This is the simplest and most reliable approach.
/// </summary>
public class PollingInputHandler : IInputHandler
{
    private bool _isRunning;
    private readonly ILogger _logger;

    public event EventHandler<KeyEventArgs>? KeyPressed;
#pragma warning disable CS0067
    public event EventHandler<MouseEventArgs>? MouseMoved;
    public event EventHandler<MouseEventArgs>? MousePressed;
    public event EventHandler<MouseEventArgs>? MouseReleased;
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;
#pragma warning restore CS0067

    public bool SupportsMouseInput => false;

    public PollingInputHandler()
    {
        _logger = DebugContext.Logger.ForCategory("PollingInputHandler");
    }

    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _logger.Info("Starting polling input handler");
        // Debug logging (uncomment to debug input handler)
        // Console.Error.WriteLine("[PollingInputHandler] Started");

        // Set console to raw mode if possible
        try
        {
            Console.TreatControlCAsInput = false;
        }
        catch { }
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _logger.Info("Stopping polling input handler");
        // Debug logging (uncomment to debug input handler)
        // Console.Error.WriteLine("[PollingInputHandler] Stopped");
    }

    public void Poll()
    {
        if (!_isRunning)
            return;

        try
        {
            // Check if a key is available
            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                _logger.Debug("Read key: {0} (Char: '{1}')", keyInfo.Key, keyInfo.KeyChar);
                // Debug logging (uncomment to debug key input)
                // Console.Error.WriteLine($"[PollingInputHandler] Key: {keyInfo.Key} Char: '{keyInfo.KeyChar}'");
                KeyPressed?.Invoke(this, new KeyEventArgs(keyInfo));
            }
        }
        catch (InvalidOperationException ex)
        {
            // This happens when console input is redirected or not available
            _logger.Error("Cannot read key: {0}", ex.Message);
            // Debug logging (uncomment to debug errors)
            // Console.Error.WriteLine($"[PollingInputHandler] Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.Error("Unexpected error polling input: {0}", ex.Message);
            // Debug logging (uncomment to debug errors)
            // Console.Error.WriteLine($"[PollingInputHandler] Unexpected error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}