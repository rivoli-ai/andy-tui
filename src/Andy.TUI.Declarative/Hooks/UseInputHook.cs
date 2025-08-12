using System;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Hooks;

/// <summary>
/// Hook for handling keyboard input in a component.
/// </summary>
public class UseInputHook : IHook
{
    private Action<ConsoleKeyInfo>? _handler;
    private IInputHandler? _inputHandler;
    private EventHandler<KeyEventArgs>? _subscription;

    /// <summary>
    /// Sets up keyboard input handling.
    /// </summary>
    /// <param name="inputHandler">The input handler to subscribe to.</param>
    /// <param name="handler">The handler for keyboard events.</param>
    public void SetHandler(IInputHandler? inputHandler, Action<ConsoleKeyInfo> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        // Unsubscribe from previous handler
        if (_subscription != null && _inputHandler != null)
        {
            _inputHandler.KeyPressed -= _subscription;
            _subscription = null;
        }

        _handler = handler;
        _inputHandler = inputHandler;

        // Subscribe to new handler
        if (_inputHandler != null)
        {
            _subscription = (sender, e) =>
            {
                var keyInfo = new ConsoleKeyInfo(
                    e.KeyChar,
                    e.Key,
                    e.Modifiers.HasFlag(ConsoleModifiers.Shift),
                    e.Modifiers.HasFlag(ConsoleModifiers.Alt),
                    e.Modifiers.HasFlag(ConsoleModifiers.Control)
                );
                _handler(keyInfo);
            };
            _inputHandler.KeyPressed += _subscription;
        }
    }

    public void Dispose()
    {
        // Unsubscribe from input handler
        if (_subscription != null && _inputHandler != null)
        {
            _inputHandler.KeyPressed -= _subscription;
        }
        _handler = null;
        _inputHandler = null;
        _subscription = null;
    }
}