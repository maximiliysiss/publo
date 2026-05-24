using System.Collections.Generic;

namespace Publo.Kafka.Options;

/// <summary>
/// Configures the Kafka Publo provider.
/// </summary>
public sealed class KafkaPubloOptions
{
    /// <summary>
    /// Gets or sets the Kafka topic used for publishing and consuming messages.
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets how the consumer handles malformed or unsupported messages.
    /// </summary>
    public SkipPolicy SkipPolicy { get; set; } = SkipPolicy.Soft;

    /// <summary>
    /// Gets or sets the Confluent.Kafka consumer configuration values.
    /// </summary>
    public Dictionary<string, string> ConsumerConfig { get; set; } = [];

    /// <summary>
    /// Gets or sets the Confluent.Kafka producer configuration values.
    /// </summary>
    public Dictionary<string, string> ProducerConfig { get; set; } = [];
}
