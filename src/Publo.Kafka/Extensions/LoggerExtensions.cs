using System;
using Microsoft.Extensions.Logging;

namespace Publo.Kafka.Extensions;

internal static partial class LoggerExtensions
{
    [LoggerMessage(1, LogLevel.Error, "Restarting service {hostedService} due to an exception. Retry #{retryNumber}.")]
    public static partial void RestartingService(this ILogger logger, string hostedService, int retryNumber, Exception exception);

    [LoggerMessage(2, LogLevel.Information, "Starting service {hostedService}.")]
    public static partial void StartingService(this ILogger logger, string hostedService);

    [LoggerMessage(3, LogLevel.Information, "Restarting service {hostedService} due to a configuration change.")]
    public static partial void ConfigurationChanged(this ILogger logger, string hostedService);

    [LoggerMessage(4, LogLevel.Information, "Stopping service {serviceName}.")]
    public static partial void StoppingService(this ILogger logger, string serviceName);

    [LoggerMessage(5, LogLevel.Error, "No message received from Kafka.")]
    public static partial void NoMessageReceived(this ILogger logger);

    [LoggerMessage(6, LogLevel.Error, "Type {typeName} not found.")]
    public static partial void TypeNotFound(this ILogger logger, string typeName);

    [LoggerMessage(7, LogLevel.Error, "Value for key {key} not found.")]
    public static partial void ValueNotFound(this ILogger logger, string key);
}
