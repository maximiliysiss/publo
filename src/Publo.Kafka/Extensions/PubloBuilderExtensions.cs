using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Publo.Abstraction.Builder;
using Publo.Kafka.Consumer;
using Publo.Kafka.Options;
using Publo.Kafka.Producer;
using Publo.Kafka.Provider;

namespace Publo.Kafka.Extensions;

/// <summary>
/// Provides Kafka provider registration methods for Publo.
/// </summary>
public static class PubloBuilderExtensions
{
    /// <summary>
    /// Registers the Kafka Publo provider, producer, consumer hosted service, and Kafka options binding.
    /// </summary>
    /// <param name="builder">The Publo builder to configure.</param>
    public static IPubloBuilder UseKafka(this IPubloBuilder builder)
    {
        builder.UseProvider<KafkaPubloProvider>();

        var services = builder.Services;

        services
            .AddOptions<KafkaPubloOptions>()
            .BindConfiguration(nameof(KafkaPubloOptions));

        services
            .TryAddSingleton<IKafkaProducer, KafkaProducer>();

        services
            .AddHostedService<KafkaConsumer>();

        return builder;
    }
}
