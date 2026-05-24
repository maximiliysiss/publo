using System.Threading;
using System.Threading.Tasks;

namespace Publo.Postgres.Environment;

internal interface ISynchronizer
{
    Task ReadyAsync(CancellationToken cancellationToken);
}
