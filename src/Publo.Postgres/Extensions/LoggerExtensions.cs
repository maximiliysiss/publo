using System;
using Microsoft.Extensions.Logging;

namespace Publo.Postgres.Extensions;

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

    [LoggerMessage(5, LogLevel.Debug, "No pending Postgres message found for client {clientId}.")]
    public static partial void NoPendingPostgresMessage(this ILogger logger, Guid clientId);

    [LoggerMessage(6, LogLevel.Error, "Type {typeName} not found.")]
    public static partial void TypeNotFound(this ILogger logger, string typeName);

    [LoggerMessage(7, LogLevel.Error, "Value for key {key} not found.")]
    public static partial void ValueNotFound(this ILogger logger, string key);

    [LoggerMessage(8, LogLevel.Debug, "Adding Postgres message of type {messageType}.")]
    public static partial void AddingPostgresMessage(this ILogger logger, string messageType);

    [LoggerMessage(9, LogLevel.Debug, "Postgres message of type {messageType} added.")]
    public static partial void PostgresMessageAdded(this ILogger logger, string messageType);

    [LoggerMessage(10, LogLevel.Debug, "Postgres message {messageId} selected for client {clientId}.")]
    public static partial void PostgresMessageSelected(this ILogger logger, long messageId, Guid clientId);

    [LoggerMessage(11, LogLevel.Debug, "Creating Postgres client {clientId}.")]
    public static partial void CreatingPostgresClient(this ILogger logger, Guid clientId);

    [LoggerMessage(12, LogLevel.Debug, "Postgres client {clientId} created.")]
    public static partial void PostgresClientCreated(this ILogger logger, Guid clientId);

    [LoggerMessage(13, LogLevel.Debug, "Committing Postgres message {messageId} for client {clientId}.")]
    public static partial void CommittingPostgresMessage(this ILogger logger, long messageId, Guid clientId);

    [LoggerMessage(14, LogLevel.Debug, "Postgres message {messageId} committed for client {clientId}.")]
    public static partial void PostgresMessageCommitted(this ILogger logger, long messageId, Guid clientId);

    [LoggerMessage(15, LogLevel.Information, "Waiting for Postgres infrastructure readiness.")]
    public static partial void WaitingForInfrastructureReadiness(this ILogger logger);

    [LoggerMessage(16, LogLevel.Information, "Postgres infrastructure is ready.")]
    public static partial void InfrastructureReady(this ILogger logger);

    [LoggerMessage(17, LogLevel.Information, "Starting Postgres migrations for schema {schemaName}.")]
    public static partial void StartingPostgresMigrations(this ILogger logger, string schemaName);

    [LoggerMessage(18, LogLevel.Information, "Postgres migrations completed for schema {schemaName}.")]
    public static partial void PostgresMigrationsCompleted(this ILogger logger, string schemaName);

    [LoggerMessage(19, LogLevel.Debug, "Reloading Npgsql types after migrations.")]
    public static partial void ReloadingNpgsqlTypes(this ILogger logger);

    [LoggerMessage(20, LogLevel.Debug, "Npgsql types reloaded after migrations.")]
    public static partial void NpgsqlTypesReloaded(this ILogger logger);

    [LoggerMessage(21, LogLevel.Debug, "Postgres runner client {clientId} registered.")]
    public static partial void PostgresRunnerClientRegistered(this ILogger logger, Guid clientId);

    [LoggerMessage(22, LogLevel.Debug, "Postgres message {messageId} handled for client {clientId}.")]
    public static partial void PostgresMessageHandled(this ILogger logger, long messageId, Guid clientId);
}
