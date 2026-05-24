using System.Threading;
using System.Threading.Tasks;

namespace Publo.Abstraction.Provider;

/// <summary>
/// Sends messages to a concrete transport or storage provider.
/// </summary>
public interface IPubloProvider
{
    /// <summary>
    /// Sends a message through the provider implementation.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="message">The message instance to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task SendAsync<T>(T message, CancellationToken cancellationToken);
}
