using System.Threading;
using System.Threading.Tasks;
using Publo.Abstraction.Provider;
using Publo.Kafka.Producer;

namespace Publo.Kafka.Provider;

internal sealed class KafkaPubloProvider(IKafkaProducer producer) : IPubloProvider
{
    public Task SendAsync<T>(T message, CancellationToken cancellationToken) => producer.ProduceAsync(message, cancellationToken);
}
