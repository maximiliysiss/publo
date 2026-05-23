using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Publo.Kafka.Infrastructure.Hosted;
using Xunit;

namespace Publo.Kafka.UnitTests.Infrastructure.Hosted;

public class RestartableServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRestartService_WhenExecutionFails()
    {
        // Arrange
        using var service = new FailingThenWaitingService();

        // Act
        await service.StartAsync(CancellationToken.None);
        await service.Restarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRestartService_WhenReloadTokenIsCanceled()
    {
        // Arrange
        using var service = new ReloadingService();

        // Act
        await service.StartAsync(CancellationToken.None);
        await service.Restarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await service.StopAsync(CancellationToken.None);

        // Assert
        service.CallCount.Should().Be(2);
    }

    private sealed class FailingThenWaitingService()
        : RestartableService(NullLogger<FailingThenWaitingService>.Instance, new ZeroSleepDurationProvider())
    {
        public TaskCompletionSource Restarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        protected override async Task ExecuteAsync(CancellationTokenSource reloadTokenSource)
        {
            CallCount++;

            if (CallCount is 1)
                throw new InvalidOperationException("failed");

            Restarted.TrySetResult();

            await Task.Delay(Timeout.InfiniteTimeSpan, reloadTokenSource.Token);
        }
    }

    private sealed class ReloadingService()
        : RestartableService(NullLogger<ReloadingService>.Instance, new ZeroSleepDurationProvider())
    {
        public TaskCompletionSource Restarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        protected override async Task ExecuteAsync(CancellationTokenSource reloadTokenSource)
        {
            CallCount++;

            if (CallCount is 1)
            {
                await reloadTokenSource.CancelAsync();
                throw new OperationCanceledException(reloadTokenSource.Token);
            }

            Restarted.TrySetResult();

            await Task.Delay(Timeout.InfiniteTimeSpan, reloadTokenSource.Token);
        }
    }

    private sealed class ZeroSleepDurationProvider : ISleepDurationProvider
    {
        public TimeSpan GetSleepDelay(int attempt) => TimeSpan.Zero;
    }
}
