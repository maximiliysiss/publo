using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Publo.Abstraction.Builder;
using Publo.Postgres.Environment;
using Publo.Postgres.Infrastructure.Database;
using Publo.Postgres.Infrastructure.DateTime;
using Publo.Postgres.Options;
using Publo.Postgres.Provider;
using Publo.Postgres.Repositories;
using Publo.Postgres.Runners;

namespace Publo.Postgres.Extensions;

public static class PubloBuilderExtensions
{
    public static IPubloBuilder UseNpgsql<T>(this IPubloBuilder builder) where T : class, IConnectionFactory
    {
        builder.UseProvider<PostgresPubloProvider>();

        var services = builder.Services;

        services
            .AddOptions<PostgresPubloOptions>()
            .BindConfiguration(nameof(PostgresPubloOptions));

        services
            .TryAddSingleton<IConnectionFactory, T>();

        services
            .TryAddScoped<IPostgresRepository, PostgresRepository>();

        services
            .TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services
            .AddMigrations();

        services
            .TryAddSingleton<IInfrastructureReadinessWaiter, InfrastructureReadinessWaiter>();

        services
            .TryAddSingleton<ISynchronizer>(sp => (ISynchronizer)sp.GetRequiredService<IInfrastructureReadinessWaiter>());

        services
            .AddHostedService<RunnerHostedService>();

        return builder;
    }
}
