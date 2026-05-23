using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Publo.Kafka.Extensions;
using Publo.Kafka.Options;

namespace Publo.Kafka.Producer;

internal sealed class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly ConcurrentDictionary<Type, IClient> _clients = [];

    private readonly KafkaPubloOptions _options;

    public KafkaProducer(IOptions<KafkaPubloOptions> options) => _options = options.Value;

    public Task ProduceAsync<T>(T message, CancellationToken cancellationToken)
    {
        var type = typeof(T);

        var client = (IProducer<string, T>)_clients.GetOrAdd(type, _ => CreateClient());

        return client.ProduceAsync(
            topic: _options.Topic,
            message: new Message<string, T> { Key = type.GetMessageKey(), Value = message },
            cancellationToken: cancellationToken);

        IProducer<string, T> CreateClient()
        {
            var serializer = new JsonValueSerializer<T>();

            return new ProducerBuilder<string, T>(_options.ProducerConfig)
                .SetValueSerializer(serializer)
                .Build();
        }
    }

    public void Dispose()
    {
        foreach (var (_, value) in _clients)
            value.Dispose();
    }

    private sealed class JsonValueSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context) => JsonSerializer.SerializeToUtf8Bytes(data);
    }
}
