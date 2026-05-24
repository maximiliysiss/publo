using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Publo.Postgres.Environment;
using Publo.Postgres.Migrations.Options;
using Publo.Postgres.Migrations.Services;
using Publo.Postgres.Options;

namespace Publo.Postgres.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services
            .AddOptions<MigrationOptions>()
            .Configure<IOptions<PostgresPubloOptions>>((opt, repositoryOptions)
                => opt.SchemaName = repositoryOptions.Value.SchemaName);

        services
            .TryAddSingleton<IMigrationRunner, MigrationRunner>();

        services
            .AddHostedService<MigrationService>();

        return services;
    }

    private sealed class MigrationService(IMigrationRunner runner, ISynchronizer synchronizer) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await runner.MigrateUpAsync(stoppingToken);
            await synchronizer.ReadyAsync(stoppingToken);
        }
    }
}
