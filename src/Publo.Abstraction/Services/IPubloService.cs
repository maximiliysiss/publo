using System.Threading;
using System.Threading.Tasks;

namespace Publo.Abstraction.Services;

/// <summary>
/// Publishes messages through the Publo provider configured in dependency injection.
/// </summary>
public interface IPubloService
{
    /// <summary>
    /// Sends a message to the configured provider.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The message instance to publish.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task SendAsync<T>(T message, CancellationToken cancellationToken);
}
