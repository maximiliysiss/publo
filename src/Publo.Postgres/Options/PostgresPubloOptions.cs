using System;

namespace Publo.Postgres.Options;

/// <summary>
/// Configures the PostgreSQL Publo provider.
/// </summary>
public sealed class PostgresPubloOptions
{
    /// <summary>
    /// Gets or sets the PostgreSQL schema that stores Publo tables and migration metadata.
    /// </summary>
    public string SchemaName { get; set; } = "publo";

    /// <summary>
    /// Gets or sets the delay between polling attempts when no more messages are immediately available.
    /// </summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets where a new runner starts reading messages.
    /// </summary>
    public OffsetPolicy OffsetPolicy { get; set; } = OffsetPolicy.Latest;
}
