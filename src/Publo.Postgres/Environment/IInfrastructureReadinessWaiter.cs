using System.Threading;
using System.Threading.Tasks;

namespace Publo.Postgres.Environment;

internal interface IInfrastructureReadinessWaiter
{
    Task WaitAsync(CancellationToken cancellationToken);
}
