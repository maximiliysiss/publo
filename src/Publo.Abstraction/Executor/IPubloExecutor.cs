using System.Threading;
using System.Threading.Tasks;

namespace Publo.Abstraction.Executor;

/// <summary>
/// Handles messages of the specified type.
/// </summary>
/// <typeparam name="T">The message type handled by the executor.</typeparam>
public interface IPubloExecutor<T>
{
    /// <summary>
    /// Handles a received message.
    /// </summary>
    /// <param name="message">The received message.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task HandleAsync(T message, CancellationToken cancellationToken);
}
