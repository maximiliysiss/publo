using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Publo.Abstraction.Extensions;
using Publo.Abstraction.Provider;
using Publo.Kafka.Consumer;
using Publo.Kafka.Extensions;
using Publo.Kafka.Producer;
using Publo.Kafka.Provider;
using Xunit;

namespace Publo.Kafka.UnitTests.Extensions;

public class PubloBuilderExtensionsTests
{
    [Fact]
    public void UseKafka_ShouldRegisterKafkaServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPublo(builder => builder.UseKafka());

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloProvider) &&
            d.ImplementationType == typeof(KafkaPubloProvider) &&
            d.Lifetime == ServiceLifetime.Scoped);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IKafkaProducer) &&
            d.ImplementationType == typeof(KafkaProducer) &&
            d.Lifetime == ServiceLifetime.Singleton);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IHostedService) &&
            d.ImplementationType == typeof(KafkaConsumer) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseKafka_ShouldNotReplaceExistingProducer()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<IKafkaProducer, ExistingProducer>();

        // Act
        services.AddPublo(builder => builder.UseKafka());

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IKafkaProducer) &&
            d.ImplementationType == typeof(ExistingProducer) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    private sealed class ExistingProducer : IKafkaProducer
    {
        public Task ProduceAsync<T>(T message, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
