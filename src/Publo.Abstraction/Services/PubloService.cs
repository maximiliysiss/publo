using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Publo.Abstraction.Extensions;
using Publo.Abstraction.Provider;

namespace Publo.Abstraction.Services;

internal sealed class PubloService(IPubloProvider provider, ILogger<PubloService> logger) : IPubloService
{
    public async Task SendAsync<T>(T message, CancellationToken cancellationToken)
    {
        await provider.SendAsync(message, cancellationToken);

        logger.MessageSent(typeof(T).FullName ?? typeof(T).Name);
    }
}
