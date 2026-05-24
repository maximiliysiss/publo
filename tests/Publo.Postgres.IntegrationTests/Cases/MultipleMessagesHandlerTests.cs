using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Publo.Abstraction.Executor;
using Publo.Abstraction.Services;
using Publo.Postgres.IntegrationTests.Shared.Fixture;
using Xunit;

namespace Publo.Postgres.IntegrationTests.Cases;

[Collection(nameof(IntegrationTestCollection))]
public class MultipleMessagesHandlerTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task SendAsync_ShouldHandleEveryMessage()
    {
        // Arrange
        var correlationId = Guid.NewGuid();

        using var serviceScope = fixture.Services.CreateScope();

        var service = serviceScope.ServiceProvider.GetRequiredService<IPubloService>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        // Act
        await service.SendAsync(new Event(correlationId, 41), CancellationToken.None);
        await service.SendAsync(new Event(correlationId, 42), CancellationToken.None);

        // Assert
        var execute = Policy
            .HandleResult<int>(i => i < 2)
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(2))
            .Execute(() => Executor.Count(correlationId));

        execute.Should().Be(2);
    }

    public sealed record Event(Guid CorrelationId, int Value);

    public sealed class Executor : IPubloExecutor<Event>
    {
        private static readonly ConcurrentDictionary<Guid, int> Counters = [];

        public static int Count(Guid correlationId) => Counters.TryGetValue(correlationId, out var value) ? value : 0;

        public Task HandleAsync(Event message, CancellationToken cancellationToken)
        {
            Counters.AddOrUpdate(message.CorrelationId, 1, (_, value) => value + 1);
            return Task.CompletedTask;
        }
    }
}
