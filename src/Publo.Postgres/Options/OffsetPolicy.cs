namespace Publo.Postgres.Options;

/// <summary>
/// Defines where a PostgreSQL runner starts reading stored messages.
/// </summary>
public enum OffsetPolicy
{
    /// <summary>
    /// Starts from messages created after the runner starts.
    /// </summary>
    Latest,

    /// <summary>
    /// Starts from the earliest uncommitted message available to the runner.
    /// </summary>
    Earliest,
}
