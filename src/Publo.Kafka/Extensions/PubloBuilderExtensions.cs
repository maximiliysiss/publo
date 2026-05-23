using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Publo.Abstraction.Builder;
using Publo.Kafka.Consumer;
using Publo.Kafka.Options;
using Publo.Kafka.Producer;
using Publo.Kafka.Provider;

namespace Publo.Kafka.Extensions;

public static class PubloBuilderExtensions
{
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
