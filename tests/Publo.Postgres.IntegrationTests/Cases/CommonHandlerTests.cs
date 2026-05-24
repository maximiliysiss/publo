using System;
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
public class CommonHandlerTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task SendAsync_ShouldSendAndHandle()
    {
        // Arrange
        using var serviceScope = fixture.Services.CreateScope();

        var service = serviceScope.ServiceProvider.GetRequiredService<IPubloService>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        // Act
        await service.SendAsync(new Event(42), CancellationToken.None);

        // Assert
        var execute = Policy
            .HandleResult<int>(i => i is 0)
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(2))
            .Execute(() => Executor.Counter);

        execute.Should().BeGreaterThan(0);
    }

    public sealed record Event(int Value);

    public sealed class Executor : IPubloExecutor<Event>
    {
        private static int _counter;
        public static int Counter => _counter;

        public Task HandleAsync(Event message, CancellationToken cancellationToken)
        {
            if (message.Value is 42)
                Interlocked.Increment(ref _counter);

            return Task.CompletedTask;
        }
    }
}
