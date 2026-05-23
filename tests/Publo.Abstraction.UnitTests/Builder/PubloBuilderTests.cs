using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Publo.Abstraction.Builder;
using Publo.Abstraction.Provider;
using Xunit;

namespace Publo.Abstraction.UnitTests.Builder;

public class PubloBuilderTests
{
    [Fact]
    public void Services_ShouldReturnSourceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new PubloBuilder(services);

        // Act
        var result = builder.Services;

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void UseProvider_ShouldRegisterProviderWithRequestedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new PubloBuilder(services);

        // Act
        var result = builder.UseProvider<FakeProvider>(ServiceLifetime.Singleton);

        // Assert
        result.Should().BeSameAs(builder);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloProvider) &&
            d.ImplementationType == typeof(FakeProvider) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseProvider_ShouldNotReplaceExistingProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new PubloBuilder(services);

        services.AddTransient<IPubloProvider, ExistingProvider>();

        // Act
        builder.UseProvider<FakeProvider>(ServiceLifetime.Singleton);

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloProvider) &&
            d.ImplementationType == typeof(ExistingProvider) &&
            d.Lifetime == ServiceLifetime.Transient);
    }

    private sealed class FakeProvider : IPubloProvider
    {
        public Task SendAsync<T>(T message, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class ExistingProvider : IPubloProvider
    {
        public Task SendAsync<T>(T message, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
