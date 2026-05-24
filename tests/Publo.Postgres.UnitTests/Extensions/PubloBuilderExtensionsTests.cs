using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Publo.Abstraction.Extensions;
using Publo.Abstraction.Provider;
using Publo.Postgres.Environment;
using Publo.Postgres.Extensions;
using Publo.Postgres.Infrastructure.Database;
using Publo.Postgres.Infrastructure.DateTime;
using Publo.Postgres.Provider;
using Publo.Postgres.Repositories;
using Publo.Postgres.Runners;
using Xunit;

namespace Publo.Postgres.UnitTests.Extensions;

public class PubloBuilderExtensionsTests
{
    [Fact]
    public void UseNpgsql_ShouldRegisterPostgresServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPublo(builder => builder.UseNpgsql<TestConnectionFactory>());

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPubloProvider) &&
            d.ImplementationType == typeof(PostgresPubloProvider) &&
            d.Lifetime == ServiceLifetime.Scoped);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IConnectionFactory) &&
            d.ImplementationType == typeof(TestConnectionFactory) &&
            d.Lifetime == ServiceLifetime.Singleton);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPostgresRepository) &&
            d.ImplementationType == typeof(PostgresRepository) &&
            d.Lifetime == ServiceLifetime.Scoped);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IDateTimeProvider) &&
            d.ImplementationType == typeof(DateTimeProvider) &&
            d.Lifetime == ServiceLifetime.Singleton);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IInfrastructureReadinessWaiter) &&
            d.ImplementationType == typeof(InfrastructureReadinessWaiter) &&
            d.Lifetime == ServiceLifetime.Singleton);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(ISynchronizer) &&
            d.Lifetime == ServiceLifetime.Singleton);

        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IHostedService) &&
            d.ImplementationType == typeof(RunnerHostedService) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void UseNpgsql_ShouldNotReplaceExistingConnectionFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<IConnectionFactory, ExistingConnectionFactory>();

        // Act
        services.AddPublo(builder => builder.UseNpgsql<TestConnectionFactory>());

        // Assert
        services.Should().ContainSingle(d =>
            d.ServiceType == typeof(IConnectionFactory) &&
            d.ImplementationType == typeof(ExistingConnectionFactory) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    private sealed class TestConnectionFactory : IConnectionFactory
    {
        public string GetConnectionString() => string.Empty;

        public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
            => throw new System.NotSupportedException();
    }

    private sealed class ExistingConnectionFactory : IConnectionFactory
    {
        public string GetConnectionString() => string.Empty;

        public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
            => throw new System.NotSupportedException();
    }
}
