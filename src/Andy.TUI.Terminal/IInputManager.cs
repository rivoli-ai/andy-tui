namespace Andy.TUI.Terminal;

/// <summary>
/// Enhanced input manager interface that supports unified input events.
/// </summary>
public interface IInputManager : IDisposable
{
    /// <summary>
    /// Event raised when any input event occurs.
    /// </summary>
    event EventHandler<InputEvent>? InputReceived;
    
    /// <summary>
    /// Gets whether mouse input is supported.
    /// </summary>
    bool SupportsMouseInput { get; }
    
    /// <summary>
    /// Gets whether the input manager is currently running.
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// Gets or sets whether input buffering is enabled.
    /// </summary>
    bool BufferingEnabled { get; set; }
    
    /// <summary>
    /// Gets the maximum size of the input buffer.
    /// </summary>
    int BufferSize { get; set; }
    
    /// <summary>
    /// Starts listening for input events.
    /// </summary>
    void Start();
    
    /// <summary>
    /// Stops listening for input events.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Polls for input events. Should be called regularly in the main loop.
    /// </summary>
    void Poll();
    
    /// <summary>
    /// Enables mouse input if supported.
    /// </summary>
    void EnableMouseInput();
    
    /// <summary>
    /// Disables mouse input.
    /// </summary>
    void DisableMouseInput();
    
    /// <summary>
    /// Gets all buffered input events and clears the buffer.
    /// </summary>
    /// <returns>Array of input events in chronological order.</returns>
    InputEvent[] FlushBuffer();
    
    /// <summary>
    /// Clears the input buffer without returning events.
    /// </summary>
    void ClearBuffer();
    
    /// <summary>
    /// Gets the current mouse position if mouse input is enabled.
    /// </summary>
    /// <returns>Current mouse position or null if not available.</returns>
    (int X, int Y)? GetMousePosition();
}