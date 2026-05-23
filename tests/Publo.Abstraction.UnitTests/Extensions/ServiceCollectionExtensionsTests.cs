using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Publo.Abstraction.Executor;
using Publo.Abstraction.Extensions;
using Publo.Abstraction.Provider;
using Publo.Abstraction.Services;
using Xunit;

namespace Publo.Abstraction.UnitTests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPublo_ShouldInvokeBuilderActionAndRegisterPubloService()
    {
        // Arrange
        var services = new ServiceCollection();
        var wasActionInvoked = false;

        // Act
        var result = services.AddPublo(builder =>
        {
            wasActionInvoked = true;
            builder.UseProvider<FakeProvider>();
        });

        // Assert
        result.Should().BeSameAs(services);
        wasActionInvoked.Should().BeTrue();

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloService) &&
            d.ImplementationType == typeof(PubloService) &&
            d.Lifetime == ServiceLifetime.Scoped);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloProvider) &&
            d.ImplementationType == typeof(FakeProvider) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddPublo_ShouldNotReplaceExistingPubloService()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<IPubloService, ExistingPubloService>();

        // Act
        services.AddPublo(builder => builder.UseProvider<FakeProvider>());

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloService) &&
            d.ImplementationType == typeof(ExistingPubloService) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPubloExecutor_ShouldRegisterExecutorWithRequestedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPubloExecutor<TestMessage, TestExecutor>(ServiceLifetime.Singleton);

        // Assert
        result.Should().BeSameAs(services);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloExecutor<TestMessage>) &&
            d.ImplementationType == typeof(TestExecutor) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddPubloExecutor_ShouldNotReplaceExistingExecutor()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddTransient<IPubloExecutor<TestMessage>, ExistingExecutor>();

        // Act
        services.AddPubloExecutor<TestMessage, TestExecutor>(ServiceLifetime.Singleton);

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloExecutor<TestMessage>) &&
            d.ImplementationType == typeof(ExistingExecutor) &&
            d.Lifetime == ServiceLifetime.Transient);
    }

    private sealed class FakeProvider : IPubloProvider
    {
        public Task SendAsync<T>(T message, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ExistingPubloService : IPubloService
    {
        public Task SendAsync<T>(T message, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed record TestMessage;

    private sealed class TestExecutor : IPubloExecutor<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ExistingExecutor : IPubloExecutor<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
