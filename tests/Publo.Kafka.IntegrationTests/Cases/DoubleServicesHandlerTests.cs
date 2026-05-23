using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Publo.Abstraction.Executor;
using Publo.Abstraction.Services;
using Publo.Kafka.IntegrationTests.Shared.Fixture;
using Xunit;

namespace Publo.Kafka.IntegrationTests.Cases;

[Collection(nameof(IntegrationTestCollection))]
public class DoubleServicesHandlerTests : IAsyncLifetime
{
    private IntegrationTestFixture[] _fixtures = [];

    [Fact]
    public async Task Handle_ShouldBeHandled_WhenThereAreSeveralPods()
    {
        // Arrange
        var initFixture = _fixtures.First();

        await Task.Delay(TimeSpan.FromSeconds(2));

        // Act
        using var serviceScope = initFixture.Services.CreateScope();

        var service = serviceScope.ServiceProvider.GetRequiredService<IPubloService>();

        await service.SendAsync(new Event(42), CancellationToken.None);

        // Assert
        var execute = Policy
            .HandleResult<int>(i => i < 2)
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(2))
            .Execute(() => Executor.Counter);

        execute.Should().BeGreaterThan(1);
    }

    public sealed record Event(int Value);

    public sealed class Executor : IPubloExecutor<Event>
    {
        private static int _counter;
        public static int Counter => _counter;

        public Task HandleAsync(Event message, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _counter);
            return Task.CompletedTask;
        }
    }

    public Task InitializeAsync()
    {
        _fixtures = Enumerable
            .Range(0, 2)
            .Select(_ => Run())
            .ToArray();

        return Task.CompletedTask;

        static IntegrationTestFixture Run()
        {
            var integrationTestFixture = new IntegrationTestFixture();

            using var _ = integrationTestFixture.CreateClient();

            return integrationTestFixture;
        }
    }

    public async Task DisposeAsync()
    {
        foreach (var integrationTestFixture in _fixtures)
            await integrationTestFixture.DisposeAsync();
    }
}
