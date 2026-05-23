using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Publo.Kafka.Producer;
using Publo.Kafka.Provider;
using Xunit;

namespace Publo.Kafka.UnitTests.Provider;

public class KafkaPubloProviderTests
{
    [Fact]
    public async Task SendAsync_ShouldForwardMessageAndCancellationTokenToProducer()
    {
        // Arrange
        var message = new TestMessage(42);
        using var cancellationTokenSource = new CancellationTokenSource();

        var producer = new CapturingProducer();
        var provider = new KafkaPubloProvider(producer);

        // Act
        await provider.SendAsync(message, cancellationTokenSource.Token);

        // Assert
        producer.Message.Should().Be(message);
        producer.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    private sealed record TestMessage(int Value);

    private sealed class CapturingProducer : IKafkaProducer
    {
        public object? Message { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task ProduceAsync<T>(T message, CancellationToken cancellationToken)
        {
            Message = message;
            CancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}
