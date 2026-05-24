namespace Publo.Kafka.Options;

/// <summary>
/// Defines how the Kafka consumer handles messages it cannot process.
/// </summary>
public enum SkipPolicy
{
    /// <summary>
    /// Stops processing by throwing when a message cannot be consumed, resolved, or deserialized.
    /// </summary>
    Strict,

    /// <summary>
    /// Skips unsupported messages when possible and continues processing.
    /// </summary>
    Soft,
}
