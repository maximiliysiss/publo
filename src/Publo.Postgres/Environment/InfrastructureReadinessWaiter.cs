using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Publo.Postgres.Extensions;

namespace Publo.Postgres.Environment;

internal class InfrastructureReadinessWaiter(ILogger<InfrastructureReadinessWaiter> logger) : IInfrastructureReadinessWaiter, ISynchronizer
{
    private readonly TaskCompletionSource _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task ReadyAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _completionSource.TrySetResult();
        logger.InfrastructureReady();
        return Task.CompletedTask;
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        logger.WaitingForInfrastructureReadiness();
        return _completionSource.Task.WaitAsync(cancellationToken);
    }
}
