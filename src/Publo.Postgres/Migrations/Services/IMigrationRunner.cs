using System.Threading;
using System.Threading.Tasks;

namespace Publo.Postgres.Migrations.Services;

internal interface IMigrationRunner
{
    Task MigrateUpAsync(CancellationToken cancellationToken);
}
