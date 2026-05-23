using System.Collections.Generic;
using Confluent.Kafka;

namespace Publo.Kafka.Options;

public sealed class KafkaPubloOptions
{
    public string Topic { get; set; } = string.Empty;

    public SkipPolicy SkipPolicy { get; set; } = SkipPolicy.Soft;

    public Dictionary<string, string> ConsumerConfig { get; set; } = [];
    public Dictionary<string, string> ProducerConfig { get; set; } = [];
}
