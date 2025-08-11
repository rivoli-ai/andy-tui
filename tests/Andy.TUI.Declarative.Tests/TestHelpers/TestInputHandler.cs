using System;
using System.Threading;
using Andy.TUI.Terminal;

namespace Andy.TUI.Declarative.Tests.TestHelpers;

public class TestInputHandler : IInputHandler
{
    public event EventHandler<KeyEventArgs>? KeyPressed;
#pragma warning disable CS0067
public event EventHandler<MouseEventArgs>? MouseMoved;
public event EventHandler<MouseEventArgs>? MousePressed;
public event EventHandler<MouseEventArgs>? MouseReleased;
public event EventHandler<MouseWheelEventArgs>? MouseWheel;
#pragma warning restore CS0067

    public bool SupportsMouseInput => false;

    public void Start() { /* no-op */ }
    public void Stop() { /* no-op */ }
    public void Poll() { /* no-op */ }
    public void Dispose() { }

    public void EmitKey(char ch, ConsoleKey key, ConsoleModifiers mods = 0)
    {
        var info = new ConsoleKeyInfo(ch, key,
            (mods & ConsoleModifiers.Shift) != 0,
            (mods & ConsoleModifiers.Alt) != 0,
            (mods & ConsoleModifiers.Control) != 0);
        KeyPressed?.Invoke(this, new KeyEventArgs(info));
        // Small delay to simulate processing
        Thread.Yield();
    }
}
