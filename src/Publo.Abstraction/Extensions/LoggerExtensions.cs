using Microsoft.Extensions.Logging;

namespace Publo.Abstraction.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(1, LogLevel.Debug, "Message of type {messageType} sent.")]
    public static partial void MessageSent(this ILogger logger, string messageType);
}
