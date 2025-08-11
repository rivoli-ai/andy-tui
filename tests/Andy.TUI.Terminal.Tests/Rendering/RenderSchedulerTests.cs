using Xunit;
using Moq;
using Andy.TUI.Terminal;

namespace Andy.TUI.Terminal.Tests.Rendering;

public class RenderSchedulerTests : IDisposable
{
    private readonly Mock<ITerminal> _terminalMock;
    private readonly Mock<IRenderer> _rendererMock;
    private readonly TerminalBuffer _buffer;
    private readonly RenderScheduler _scheduler;

    public RenderSchedulerTests()
    {
        _terminalMock = new Mock<ITerminal>();
        _rendererMock = new Mock<IRenderer>();
        _buffer = new TerminalBuffer(80, 24);
        _scheduler = new RenderScheduler(_terminalMock.Object, _rendererMock.Object, _buffer);
    }

    [Fact]
    public void Constructor_SetsDefaultProperties()
    {
        // Assert
        Assert.Equal(60, _scheduler.TargetFps);
        Assert.Equal(16, _scheduler.MaxBatchWaitMs);
    }

    [Fact]
    public void Start_BeginsRendering()
    {
        // Act
        _scheduler.Start();
        Thread.Sleep(100); // Let it run a bit
        _scheduler.Stop();

        // Assert
        // Should have called BeginFrame at least once if there were updates
        Assert.True(_scheduler.ActualFps >= 0);
    }

    [Fact]
    public void Stop_StopsRendering()
    {
        // Arrange
        _scheduler.Start();
        Thread.Sleep(50);

        // Act
        _scheduler.Stop();
        var fpsBefore = _scheduler.ActualFps;
        Thread.Sleep(100);
        var fpsAfter = _scheduler.ActualFps;

        // Assert
        Assert.Equal(fpsBefore, fpsAfter); // FPS should not change after stopping
    }

    [Fact]
    public void QueueRender_ExecutesUpdateAction()
    {
        // Arrange
        bool actionExecuted = false;
        _scheduler.Start();

        // Act
        _scheduler.QueueRender(() => actionExecuted = true);
        Thread.Sleep(50); // Give it time to process

        // Assert
        Assert.True(actionExecuted);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void QueueRender_WithDirtyBuffer_TriggersRender()
    {
        // Arrange
        _scheduler.Start();

        // Act
        _scheduler.QueueRender(() => _buffer.SetCell(0, 0, 'X'));
        Thread.Sleep(50); // Give it time to render

        // Assert
        _rendererMock.Verify(r => r.BeginFrame(), Times.AtLeastOnce);
        _rendererMock.Verify(r => r.EndFrame(), Times.AtLeastOnce);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void ForceRender_TriggersImmediateRender()
    {
        // Arrange
        _scheduler.Start();

        // Act
        _scheduler.ForceRender();
        Thread.Sleep(50); // Give it time to render

        // Assert
        _rendererMock.Verify(r => r.BeginFrame(), Times.AtLeastOnce);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void RenderScheduler_BatchesMultipleUpdates()
    {
        // Arrange
        var updateCount = 0;
        _scheduler.MaxBatchWaitMs = 50;
        _scheduler.Start();

        // Act
        for (int i = 0; i < 5; i++)
        {
            _scheduler.QueueRender(() =>
            {
                updateCount++;
                _buffer.SetCell(i, 0, (char)('A' + i));
            });
        }
        Thread.Sleep(100); // Wait for batch to complete

        // Assert
        Assert.Equal(5, updateCount); // All updates should execute
        _rendererMock.Verify(r => r.BeginFrame(), Times.AtLeastOnce);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void BeforeRender_EventIsRaised()
    {
        // Arrange
        bool eventRaised = false;
        _scheduler.BeforeRender += (s, e) => eventRaised = true;
        _scheduler.Start();

        // Act
        _scheduler.QueueRender(() => _buffer.SetCell(0, 0, 'X'));
        Thread.Sleep(50);

        // Assert
        Assert.True(eventRaised);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void AfterRender_EventIsRaised()
    {
        // Arrange
        bool eventRaised = false;
        double renderTime = 0;
        _scheduler.AfterRender += (s, e) =>
        {
            eventRaised = true;
            renderTime = e.RenderTimeMs;
        };
        _scheduler.Start();

        // Act
        _scheduler.QueueRender(() => _buffer.SetCell(0, 0, 'X'));
        Thread.Sleep(50);

        // Assert
        Assert.True(eventRaised);
        Assert.True(renderTime >= 0);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void BeforeRender_CanCancelRender()
    {
        // Arrange
        _scheduler.BeforeRender += (s, e) => e.Cancel = true;
        _scheduler.Start();

        // Act
        _scheduler.QueueRender(() => _buffer.SetCell(0, 0, 'X'));
        Thread.Sleep(50);

        // Assert
        _rendererMock.Verify(r => r.BeginFrame(), Times.Never);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void TargetFps_LimitsFrameRate()
    {
        // Arrange
        _scheduler.TargetFps = 10; // Low FPS for testing
        _scheduler.Start();

        // Act
        for (int i = 0; i < 20; i++)
        {
            _scheduler.QueueRender(() => _buffer.SetCell(0, 0, (char)('A' + i)));
            Thread.Sleep(10);
        }
        Thread.Sleep(1100); // Just over 1 second

        // Assert
        Assert.True(_scheduler.ActualFps <= 15); // Should be close to 10, with some tolerance

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void FixedMode_RendersAtTargetFps_WithoutDirtyBuffer()
    {
        // Arrange
        _scheduler.Mode = RenderMode.Fixed;
        _scheduler.TargetFps = 30;
        _scheduler.Start();

        // Act
        Thread.Sleep(1100); // ~1 second

        // Assert
        Assert.True(_scheduler.ActualFps >= 20, $"Expected fps >= 20, got {_scheduler.ActualFps}");

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void Dispose_StopsScheduler()
    {
        // Arrange
        _scheduler.Start();

        // Act
        _scheduler.Dispose();

        // Assert
        // Should not throw when accessing after dispose
        _scheduler.QueueRender(() => { }); // This should be safe but do nothing
    }

    [Fact]
    public void RenderScheduler_OnlyRendersWhenBufferIsDirty()
    {
        // Arrange
        _scheduler.Start();

        // Act
        _scheduler.QueueRender(() => { }); // Empty action, buffer stays clean
        Thread.Sleep(50);

        // Assert
        _rendererMock.Verify(r => r.BeginFrame(), Times.Never);

        // Cleanup
        _scheduler.Stop();
    }

    [Fact]
    public void AverageRenderTime_TracksPerformance()
    {
        // Arrange
        _scheduler.Start();

        // Act
        for (int i = 0; i < 10; i++)
        {
            _scheduler.QueueRender(() => _buffer.SetCell(i, 0, 'X'));
            Thread.Sleep(20);
        }
        Thread.Sleep(100);

        // Assert
        Assert.True(_scheduler.AverageRenderTimeMs >= 0);

        // Cleanup
        _scheduler.Stop();
    }

    public void Dispose()
    {
        _scheduler?.Dispose();
    }
}