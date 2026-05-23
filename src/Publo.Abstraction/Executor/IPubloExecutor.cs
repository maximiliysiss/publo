using System.Threading;
using System.Threading.Tasks;

namespace Publo.Abstraction.Executor;

public interface IPubloExecutor<T>
{
    Task HandleAsync(T message, CancellationToken cancellationToken);
}
