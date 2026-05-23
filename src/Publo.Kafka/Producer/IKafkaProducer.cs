using System.Threading;
using System.Threading.Tasks;

namespace Publo.Kafka.Producer;

internal interface IKafkaProducer
{
    Task ProduceAsync<T>(T message, CancellationToken cancellationToken);
}
