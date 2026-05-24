using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Publo.Postgres.Environment;
using Xunit;

namespace Publo.Postgres.UnitTests.Environment;

public class InfrastructureReadinessWaiterTests
{
    [Fact]
    public async Task WaitAsync_ShouldCompleteAfterReadyAsync()
    {
        // Arrange
        var waiter = new InfrastructureReadinessWaiter();
        var waitTask = waiter.WaitAsync(CancellationToken.None);

        // Act
        await waiter.ReadyAsync(CancellationToken.None);

        // Assert
        await waitTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task WaitAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var waiter = new InfrastructureReadinessWaiter();
        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await cancellationTokenSource.CancelAsync();
        var act = () => waiter.WaitAsync(cancellationTokenSource.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
