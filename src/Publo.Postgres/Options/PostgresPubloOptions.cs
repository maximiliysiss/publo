using System;

namespace Publo.Postgres.Options;

public sealed class PostgresPubloOptions
{
    public string SchemaName { get; set; } = "publo";
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    public OffsetPolicy OffsetPolicy { get; set; } = OffsetPolicy.Latest;
}
