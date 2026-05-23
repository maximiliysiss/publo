using System.Threading;
using System.Threading.Tasks;

namespace Publo.Abstraction.Provider;

public interface IPubloProvider
{
    Task SendAsync<T>(T message, CancellationToken cancellationToken);
}
