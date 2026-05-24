using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Publo.Postgres.Infrastructure.Database;

/// <summary>
/// Provides database connections for the PostgreSQL outbox repository.
/// </summary>
public interface IConnectionFactory
{
    /// <summary>
    /// Gets the connection string used by the outbox.
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Creates a database connection for outbox operations.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>An unopened or opened database connection usable by Publo.</returns>
    Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken);
}
