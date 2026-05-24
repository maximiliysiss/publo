using System.Threading;
using System.Threading.Tasks;
using Publo.Abstraction.Provider;
using Publo.Postgres.Environment;
using Publo.Postgres.Repositories;

namespace Publo.Postgres.Provider;

internal sealed class PostgresPubloProvider(IPostgresRepository repository, IInfrastructureReadinessWaiter waiter) : IPubloProvider
{
    public async Task SendAsync<T>(T message, CancellationToken cancellationToken)
    {
        await waiter.WaitAsync(cancellationToken);
        await repository.AddAsync(message, cancellationToken);
    }
}
