using System;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using Publo.Postgres.Entities;
using Publo.Postgres.Extensions;
using Publo.Postgres.Infrastructure.Database;
using Publo.Postgres.Infrastructure.DateTime;
using Publo.Postgres.Options;

namespace Publo.Postgres.Repositories;

internal sealed class PostgresRepository : IPostgresRepository
{
    private readonly IConnectionFactory _connectionFactory;

    private readonly PostgresPubloOptions _options;

    private readonly IDateTimeProvider _dateTimeProvider;

    public PostgresRepository(
        IConnectionFactory connectionFactory,
        IOptions<PostgresPubloOptions> options,
        IDateTimeProvider dateTimeProvider)
    {
        _connectionFactory = connectionFactory;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
    }

    public async Task AddAsync<T>(T message, CancellationToken cancellationToken)
    {
        var sql = $@"
INSERT INTO {_options.SchemaName}.messages (type, created_at, payload)
VALUES (:type, :createdAt, :payload)";

        await using var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);

        await using DbCommand command = new DbCommandInitializer(sql, connection)
        {
            Parameters =
            {
                { "type", typeof(T).GetVersionFreeFullName() },
                { "createdAt", _dateTimeProvider.GetNow() },
                { "payload", JsonSerializer.Serialize(message), NpgsqlDbType.Jsonb },
            }
        };

        await connection.OpenAsync(cancellationToken);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Message?> GetAsync(ClientId clientId, DateTimeOffset? from, CancellationToken cancellationToken)
    {
        var sql = $@"
SELECT m.id, m.type, m.payload, m.created_at
FROM {_options.SchemaName}.messages m
LEFT JOIN {_options.SchemaName}.handled h ON h.message_id = m.id AND h.client_id = :clientId
WHERE h.message_id IS NULL AND (:from IS NULL OR m.created_at >= :from)
ORDER BY m.id ASC
LIMIT 1;
";

        await using var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);

        await using DbCommand command = new DbCommandInitializer(sql, connection)
        {
            Parameters =
            {
                { "clientId", clientId.Value },
                { "from", from },
            }
        };

        await connection.OpenAsync(cancellationToken);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var isRead = await reader.ReadAsync(cancellationToken);

        if (!isRead)
            return null;

        var type = Type.GetType(reader.GetString("type"));
        if (type is null)
            throw new InvalidOperationException($"Type '{reader.GetString("type")}' not found");

        var message = JsonSerializer.Deserialize(reader.GetString("payload"), type);
        if (message is null)
            throw new InvalidOperationException($"Message payload not found");

        return new Message(
            Id: new MessageId(reader.GetInt64("id")),
            Payload: message,
            Type: type,
            CreatedAt: reader.GetFieldValue<DateTimeOffset>("created_at"));
    }

    public async Task CreateAsync(ClientId clientId, CancellationToken cancellationToken)
    {
        var sql = $@"
INSERT INTO {_options.SchemaName}.clients (id, created_at)
VALUES (:clientId, :createdAt);
";

        await using var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);

        await using DbCommand command = new DbCommandInitializer(sql, connection)
        {
            Parameters =
            {
                { "clientId", clientId.Value },
                { "createdAt", _dateTimeProvider.GetNow() },
            }
        };

        await connection.OpenAsync(cancellationToken);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task CommitAsync(MessageId messageId, ClientId clientId, CancellationToken cancellationToken)
    {
        var sql = $@"
INSERT INTO {_options.SchemaName}.handled(client_id, message_id, created_at)
VALUES (:clientId, :messageId, :createdAt);
";

        await using var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);

        await using DbCommand command = new DbCommandInitializer(sql, connection)
        {
            Parameters =
            {
                { "clientId", clientId.Value },
                { "messageId", messageId.Value },
                { "createdAt", _dateTimeProvider.GetNow() },
            }
        };

        await connection.OpenAsync(cancellationToken);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
