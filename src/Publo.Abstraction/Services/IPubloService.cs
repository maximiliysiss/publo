using System.Threading;
using System.Threading.Tasks;

namespace Publo.Abstraction.Services;

public interface IPubloService
{
    Task SendAsync<T>(T message, CancellationToken cancellationToken);
}
