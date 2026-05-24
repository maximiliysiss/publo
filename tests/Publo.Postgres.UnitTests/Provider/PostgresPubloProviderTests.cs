using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Publo.Postgres.Entities;
using Publo.Postgres.Environment;
using Publo.Postgres.Provider;
using Publo.Postgres.Repositories;
using Xunit;

namespace Publo.Postgres.UnitTests.Provider;

public class PostgresPubloProviderTests
{
    [Fact]
    public async Task SendAsync_ShouldWaitForReadinessAndForwardMessageToRepository()
    {
        // Arrange
        var message = new TestMessage(42);
        using var cancellationTokenSource = new CancellationTokenSource();

        var waiter = new CapturingWaiter();
        var repository = new CapturingRepository(waiter);
        var provider = new PostgresPubloProvider(repository, waiter);

        // Act
        await provider.SendAsync(message, cancellationTokenSource.Token);

        // Assert
        waiter.WasCalled.Should().BeTrue();
        waiter.CancellationToken.Should().Be(cancellationTokenSource.Token);
        repository.Message.Should().Be(message);
        repository.CancellationToken.Should().Be(cancellationTokenSource.Token);
        repository.WasReadinessReached.Should().BeTrue();
    }

    private sealed record TestMessage(int Value);

    private sealed class CapturingWaiter : IInfrastructureReadinessWaiter
    {
        public bool WasCalled { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            WasCalled = true;
            CancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }

    private sealed class CapturingRepository(CapturingWaiter waiter) : IPostgresRepository
    {
        public object? Message { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public bool WasReadinessReached { get; private set; }

        public Task AddAsync<T>(T message, CancellationToken cancellationToken)
        {
            Message = message;
            CancellationToken = cancellationToken;
            WasReadinessReached = waiter.WasCalled;

            return Task.CompletedTask;
        }

        public Task<Message?> GetAsync(ClientId clientId, DateTimeOffset? from, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task CreateAsync(ClientId clientId, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task CommitAsync(MessageId messageId, ClientId clientId, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
