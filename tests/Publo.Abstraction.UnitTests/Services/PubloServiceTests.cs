using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Publo.Abstraction.Provider;
using Publo.Abstraction.Services;
using Xunit;

namespace Publo.Abstraction.UnitTests.Services;

public class PubloServiceTests
{
    [Fact]
    public async Task SendAsync_ShouldForwardMessageAndCancellationTokenToProvider()
    {
        // Arrange
        var message = new TestMessage(42);
        using var cancellationTokenSource = new CancellationTokenSource();

        var provider = new CapturingProvider();
        var service = new PubloService(provider, NullLogger<PubloService>.Instance);

        // Act
        await service.SendAsync(message, cancellationTokenSource.Token);

        // Assert
        provider.Message.Should().Be(message);
        provider.CancellationToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task SendAsync_ShouldPropagateProviderException()
    {
        // Arrange
        var exception = new InvalidOperationException("send failed");
        var provider = new ThrowingProvider(exception);
        var service = new PubloService(provider, NullLogger<PubloService>.Instance);

        // Act
        var act = () => service.SendAsync(new TestMessage(42), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("send failed");
    }

    private sealed record TestMessage(int Value);

    private sealed class CapturingProvider : IPubloProvider
    {
        public object? Message { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task SendAsync<T>(T message, CancellationToken cancellationToken)
        {
            Message = message;
            CancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingProvider(Exception exception) : IPubloProvider
    {
        public Task SendAsync<T>(T message, CancellationToken cancellationToken) => Task.FromException(exception);
    }
}
