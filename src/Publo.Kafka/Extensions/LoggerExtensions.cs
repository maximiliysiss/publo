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

    [LoggerMessage(8, LogLevel.Information, "Kafka consumer subscribed to topic {topic}.")]
    public static partial void KafkaConsumerSubscribed(this ILogger logger, string topic);

    [LoggerMessage(9, LogLevel.Debug, "Kafka message received from topic {topic}, partition {partition}, offset {offset}.")]
    public static partial void KafkaMessageReceived(this ILogger logger, string topic, int partition, long offset);

    [LoggerMessage(10, LogLevel.Debug, "Kafka message with key {key} handled.")]
    public static partial void KafkaMessageHandled(this ILogger logger, string key);

    [LoggerMessage(11, LogLevel.Debug, "Kafka message committed for topic {topic}, partition {partition}, offset {offset}.")]
    public static partial void KafkaMessageCommitted(this ILogger logger, string topic, int partition, long offset);

    [LoggerMessage(12, LogLevel.Debug, "Creating Kafka producer for message type {messageType}.")]
    public static partial void CreatingKafkaProducer(this ILogger logger, string messageType);

    [LoggerMessage(13, LogLevel.Debug, "Producing Kafka message of type {messageType} to topic {topic}.")]
    public static partial void ProducingKafkaMessage(this ILogger logger, string messageType, string topic);

    [LoggerMessage(14, LogLevel.Debug, "Kafka message of type {messageType} produced to topic {topic}, partition {partition}, offset {offset}.")]
    public static partial void KafkaMessageProduced(this ILogger logger, string messageType, string topic, int partition, long offset);

    [LoggerMessage(15, LogLevel.Debug, "Disposing {producerCount} Kafka producer clients.")]
    public static partial void DisposingKafkaProducers(this ILogger logger, int producerCount);
}
