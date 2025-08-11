using System;
using System.Linq;
using System.Threading;

namespace Andy.TUI.Terminal;

/// <summary>
/// Adapter that exposes CrossPlatformInputManager as an IInputHandler.
/// </summary>
public class CrossPlatformInputHandler : IInputHandler
{
    private readonly CrossPlatformInputManager _manager;
    private readonly CancellationTokenSource _cts = new();
    private Thread? _pumpThread;

    public event EventHandler<KeyEventArgs>? KeyPressed;
    public event EventHandler<MouseEventArgs>? MouseMoved;
    public event EventHandler<MouseEventArgs>? MousePressed;
    public event EventHandler<MouseEventArgs>? MouseReleased;
    public event EventHandler<MouseWheelEventArgs>? MouseWheel;

    public bool SupportsMouseInput => _manager.SupportsMouseInput;

    public CrossPlatformInputHandler()
    {
        _manager = new CrossPlatformInputManager();
        _manager.InputReceived += OnManagerInput;
    }

    public void Start()
    {
        _manager.Start();
        _pumpThread = new Thread(Pump) { IsBackground = true, Name = "CP Input Pump" };
        _pumpThread.Start();
    }

    public void Stop()
    {
        _cts.Cancel();
        _manager.Stop();
        _pumpThread?.Join(200);
        _pumpThread = null;
    }

    public void Poll()
    {
        // No-op; events are raised on background thread
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }

    private void Pump()
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                var events = _manager.FlushBuffer();
                foreach (var e in events)
                {
                    Dispatch(e);
                }
                Thread.Sleep(5);
            }
        }
        catch (ThreadAbortException) { }
    }

    private void OnManagerInput(object? sender, InputEvent e)
    {
        Dispatch(e);
    }

    private void Dispatch(InputEvent e)
    {
        if (e.Type == InputEventType.KeyPress && e.Key != null)
        {
            KeyPressed?.Invoke(this, new KeyEventArgs(new ConsoleKeyInfo(
                e.Key.KeyChar,
                e.Key.Key,
                (e.Key.Modifiers & ConsoleModifiers.Shift) != 0,
                (e.Key.Modifiers & ConsoleModifiers.Alt) != 0,
                (e.Key.Modifiers & ConsoleModifiers.Control) != 0
            )));
        }
        else if (e.Type == InputEventType.MouseMove && e.Mouse != null)
        {
            MouseMoved?.Invoke(this, new MouseEventArgs(e.Mouse.X, e.Mouse.Y, MouseButton.None, e.Mouse.Modifiers));
        }
        else if (e.Type == InputEventType.MousePress && e.Mouse != null)
        {
            MousePressed?.Invoke(this, new MouseEventArgs(e.Mouse.X, e.Mouse.Y, e.Mouse.Button, e.Mouse.Modifiers));
        }
        else if (e.Type == InputEventType.MouseRelease && e.Mouse != null)
        {
            MouseReleased?.Invoke(this, new MouseEventArgs(e.Mouse.X, e.Mouse.Y, e.Mouse.Button, e.Mouse.Modifiers));
        }
        else if (e.Type == InputEventType.MouseWheel && e.Mouse != null)
        {
            MouseWheel?.Invoke(this, new MouseWheelEventArgs(e.Mouse.X, e.Mouse.Y, e.Mouse.WheelDelta, e.Mouse.Modifiers));
        }
    }
}
