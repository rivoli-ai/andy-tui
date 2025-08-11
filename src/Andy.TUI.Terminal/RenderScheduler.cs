using System.Diagnostics;
using Andy.TUI.Diagnostics;

namespace Andy.TUI.Terminal;

/// <summary>
/// Manages rendering scheduling, frame rate limiting, and update batching.
/// </summary>
public class RenderScheduler : IDisposable
{
    private readonly ITerminal _terminal;
    private readonly IRenderer _renderer;
    private readonly TerminalBuffer _buffer;
    private readonly object _renderLock = new();
    private readonly Queue<Action> _renderQueue = new();
    private readonly ManualResetEventSlim _renderEvent = new(false);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger _logger;

    private Thread? _renderThread;
    private volatile bool _isRunning;
    private volatile bool _forceRender;

    /// <summary>
    /// Gets or sets the target frames per second.
    /// </summary>
    public int TargetFps { get; set; } = 60;

    /// <summary>
    /// Gets or sets the render scheduling mode.
    /// OnDemand renders only when there are dirty buffer regions or a force render is requested.
    /// Fixed renders a frame at TargetFps regardless of dirty state.
    /// </summary>
    public RenderMode Mode { get; set; } = RenderMode.OnDemand;

    /// <summary>
    /// Gets or sets the maximum time to wait for batching updates (in milliseconds).
    /// </summary>
    public int MaxBatchWaitMs { get; set; } = 16;

    /// <summary>
    /// Gets the actual frames per second achieved.
    /// </summary>
    public double ActualFps { get; private set; }

    /// <summary>
    /// Gets the average render time in milliseconds.
    /// </summary>
    public double AverageRenderTimeMs { get; private set; }

    /// <summary>
    /// Event raised before each frame is rendered.
    /// </summary>
    public event EventHandler<RenderFrameEventArgs>? BeforeRender;

    /// <summary>
    /// Event raised after each frame is rendered.
    /// </summary>
    public event EventHandler<RenderFrameEventArgs>? AfterRender;

    /// <summary>
    /// Creates a new render scheduler.
    /// </summary>
    public RenderScheduler(ITerminal terminal, IRenderer renderer, TerminalBuffer buffer)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _logger = LogManager.GetLogger<RenderScheduler>();
    }

    /// <summary>
    /// Starts the render scheduler.
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _renderThread = new Thread(RenderLoop)
        {
            Name = "Andy.TUI Render Thread",
            IsBackground = true
        };
        _renderThread.Start();
    }

    /// <summary>
    /// Stops the render scheduler.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cancellationTokenSource.Cancel();
        _renderEvent.Set();

        _renderThread?.Join(1000);
        _renderThread = null;
    }

    /// <summary>
    /// Queues a render update.
    /// </summary>
    public void QueueRender(Action? updateAction = null)
    {
        lock (_renderLock)
        {
            if (updateAction != null)
            {
                _renderQueue.Enqueue(updateAction);
            }
            _renderEvent.Set();
        }
    }

    /// <summary>
    /// Forces an immediate render on the next frame.
    /// </summary>
    public void ForceRender()
    {
        _forceRender = true;
        _renderEvent.Set();
    }

    /// <summary>
    /// Processes all queued render operations immediately.
    /// This is synchronous and ensures the buffer is updated.
    /// </summary>
    public void ProcessQueuedOperations()
    {
        var updates = new List<Action>();
        lock (_renderLock)
        {
            while (_renderQueue.Count > 0)
            {
                updates.Add(_renderQueue.Dequeue());
            }
        }

        // Execute updates synchronously
        foreach (var update in updates)
        {
            update();
        }
    }

    /// <summary>
    /// The main render loop.
    /// </summary>
    private void RenderLoop()
    {
        var frameStopwatch = new Stopwatch();
        var fpsStopwatch = new Stopwatch();
        var renderTimes = new Queue<double>();
        var targetFrameTime = TimeSpan.FromSeconds(1.0 / TargetFps);

        int frameCount = 0;
        fpsStopwatch.Start();

        while (_isRunning)
        {
            frameStopwatch.Restart();

            try
            {
                // Wait for either an update signal or the next frame boundary (for fixed mode)
                var remaining = targetFrameTime - frameStopwatch.Elapsed;
                var waitTime = Math.Max(0, (int)Math.Round(remaining.TotalMilliseconds));
                if (Mode == RenderMode.Fixed)
                {
                    // In fixed mode, don't depend on signals; just sleep to the next frame
                    if (waitTime > 0)
                        Thread.Sleep(waitTime);
                }
                else
                {
                    _renderEvent.Wait(Math.Max(1, waitTime), _cancellationTokenSource.Token);
                    _renderEvent.Reset();
                }

                // Process all queued updates
                var updates = new List<Action>();
                lock (_renderLock)
                {
                    while (_renderQueue.Count > 0)
                    {
                        updates.Add(_renderQueue.Dequeue());
                    }
                }

                // Execute updates
                foreach (var update in updates)
                {
                    update();
                }

                // Check if we need to render
                if (Mode == RenderMode.Fixed || _buffer.IsDirty || _forceRender)
                {
                    RenderFrame();
                    _forceRender = false;
                }

                // Update FPS counter
                frameCount++;
                if (fpsStopwatch.ElapsedMilliseconds >= 1000)
                {
                    ActualFps = frameCount / (fpsStopwatch.ElapsedMilliseconds / 1000.0);
                    frameCount = 0;
                    fpsStopwatch.Restart();

                    // Update average render time
                    if (renderTimes.Count > 0)
                    {
                        AverageRenderTimeMs = renderTimes.Average();
                    }
                }

                // Track render time
                var renderTime = frameStopwatch.ElapsedMilliseconds;
                renderTimes.Enqueue(renderTime);
                if (renderTimes.Count > 60) // Keep last 60 frame times
                {
                    renderTimes.Dequeue();
                }

                // Frame rate limiting for on-demand mode only (fixed mode handled above)
                if (Mode == RenderMode.OnDemand)
                {
                    var elapsed = frameStopwatch.Elapsed;
                    if (elapsed < targetFrameTime)
                    {
                        Thread.Sleep(targetFrameTime - elapsed);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log error but continue rendering
                _logger.Error(ex, "Render loop error");
            }
        }
    }

    /// <summary>
    /// Renders a single frame.
    /// </summary>
    private void RenderFrame()
    {
        var frameArgs = new RenderFrameEventArgs();

        // Raise before render event
        BeforeRender?.Invoke(this, frameArgs);

        if (frameArgs.Cancel)
            return;

        var renderStopwatch = Stopwatch.StartNew();

        // Get dirty regions and swap buffers
        var dirtyRegions = _buffer.SwapBuffers().ToList();

        // Always begin/end a frame when force-rendering, even if there are no dirty cells
        var shouldBeginFrame = dirtyRegions.Count > 0 || _forceRender || Mode == RenderMode.Fixed;
        if (shouldBeginFrame)
        {
            _renderer.BeginFrame();
            try
            {
                if (dirtyRegions.Count > 0)
                {
                    // Render dirty cells
                    if (_renderer is AnsiRenderer ansiRenderer)
                    {
                        ansiRenderer.RenderCells(dirtyRegions);
                    }
                    else
                    {
                        // Fallback to individual cell rendering
                        foreach (var region in dirtyRegions)
                        {
                            _renderer.DrawChar(region.X, region.Y, region.NewCell.Character, region.NewCell.Style);
                        }
                    }
                }
            }
            finally
            {
                _renderer.EndFrame();
            }
        }

        renderStopwatch.Stop();
        frameArgs.RenderTimeMs = renderStopwatch.ElapsedMilliseconds;
        frameArgs.DirtyCellCount = dirtyRegions.Count;

        // Raise after render event
        AfterRender?.Invoke(this, frameArgs);
    }

    /// <summary>
    /// Disposes the render scheduler.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _cancellationTokenSource.Dispose();
        _renderEvent.Dispose();
    }
}

/// <summary>
/// Controls how the render scheduler decides when to draw frames.
/// </summary>
public enum RenderMode
{
    /// <summary>
    /// Render only when there are changes or a forced render is requested.
    /// </summary>
    OnDemand,

    /// <summary>
    /// Render at a fixed, configurable frame rate regardless of changes.
    /// </summary>
    Fixed
}

/// <summary>
/// Event arguments for render frame events.
/// </summary>
public class RenderFrameEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets whether to cancel the render.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the render time in milliseconds.
    /// </summary>
    public double RenderTimeMs { get; internal set; }

    /// <summary>
    /// Gets the number of dirty cells rendered.
    /// </summary>
    public int DirtyCellCount { get; internal set; }
}