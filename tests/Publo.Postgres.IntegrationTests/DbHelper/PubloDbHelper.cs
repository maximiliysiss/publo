using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using Publo.Postgres.Entities;
using Publo.Postgres.Extensions;
using Publo.Postgres.Infrastructure.Database;
using Publo.Postgres.Options;

namespace Publo.Postgres.IntegrationTests.DbHelper;

internal sealed class PubloDbHelper : IAsyncDisposable
{
    private readonly IConnectionFactory _connectionFactory;

    private readonly HashSet<Guid> _clientIds = [];

    private readonly HashSet<long> _messageIds = [];

    private readonly string _schemaName;

    public PubloDbHelper(IConnectionFactory connectionFactory, IOptions<PostgresPubloOptions> options)
    {
        _connectionFactory = connectionFactory;
        _schemaName = options.Value.SchemaName;
    }

    public async Task<long> AddMessageAsync<T>(T payload, DateTimeOffset? createdAt = null)
    {
        var query = $@"
INSERT INTO {_schemaName}.messages(type, payload, created_at)
VALUES (:type, :payload, :createdAt)
RETURNING id;
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "type", typeof(T).GetVersionFreeFullName() },
                { "payload", JsonSerializer.Serialize(payload), NpgsqlDbType.Jsonb },
                { "createdAt", createdAt ?? DateTimeOffset.UtcNow },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        var id = (long?)await command.ExecuteScalarAsync(CancellationToken.None)
                 ?? throw new InvalidOperationException("No message id returned.");

        _messageIds.Add(id);

        return id;
    }

    public async Task AddHandledAsync(ClientId clientId, MessageId messageId, DateTimeOffset? createdAt = null)
    {
        var query = $@"
INSERT INTO {_schemaName}.handled(client_id, message_id, created_at)
VALUES (:clientId, :messageId, :createdAt);
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "clientId", clientId.Value },
                { "messageId", messageId.Value },
                { "createdAt", createdAt ?? DateTimeOffset.UtcNow },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await command.ExecuteNonQueryAsync(CancellationToken.None);

        Track(clientId);
        Track(messageId);
    }

    public async Task<IReadOnlyCollection<MessageRow>> GetMessagesAsync<T>(params long[] ids)
    {
        var query = $@"
SELECT id, type, payload, created_at AS createdAt
FROM {_schemaName}.messages
WHERE (cardinality(:ids) = 0 OR id = ANY(:ids)) AND type = :type
ORDER BY id;
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "ids", ids },
                { "type", typeof(T).GetVersionFreeFullName() },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken.None);

        var rows = new List<MessageRow>();

        while (await reader.ReadAsync(CancellationToken.None))
        {
            rows.Add(new MessageRow(
                Id: reader.GetInt64(reader.GetOrdinal("id")),
                Type: reader.GetString(reader.GetOrdinal("type")),
                Payload: reader.GetString(reader.GetOrdinal("payload")),
                CreatedAt: reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("createdAt"))));
        }

        return rows;
    }

    public async Task<ClientRow?> GetClientAsync(ClientId clientId)
    {
        var query = $@"
SELECT id, created_at AS createdAt
FROM {_schemaName}.clients
WHERE id = :clientId;
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "clientId", clientId.Value },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken.None);

        if (!await reader.ReadAsync(CancellationToken.None))
            return null;

        return new ClientRow(
            Id: reader.GetGuid(reader.GetOrdinal("id")),
            CreatedAt: reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("createdAt")));
    }

    public async Task<HandledRow?> GetHandledAsync(ClientId clientId, MessageId messageId)
    {
        var query = $@"
SELECT client_id AS clientId, message_id AS messageId, created_at AS createdAt
FROM {_schemaName}.handled
WHERE client_id = :clientId AND message_id = :messageId;
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "clientId", clientId.Value },
                { "messageId", messageId.Value },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await using var reader = await command.ExecuteReaderAsync(CancellationToken.None);

        if (!await reader.ReadAsync(CancellationToken.None))
            return null;

        return new HandledRow(
            ClientId: reader.GetGuid(reader.GetOrdinal("clientId")),
            MessageId: reader.GetInt64(reader.GetOrdinal("messageId")),
            CreatedAt: reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("createdAt")));
    }

    public ClientId Track(ClientId clientId)
    {
        _clientIds.Add(clientId.Value);
        return clientId;
    }

    public MessageId Track(MessageId messageId)
    {
        _messageIds.Add(messageId.Value);
        return messageId;
    }

    public async ValueTask DisposeAsync()
    {
        await DeleteHandledAsync();
        await DeleteClientsAsync();
        await DeleteMessagesAsync();
    }

    private async Task DeleteHandledAsync()
    {
        var query = $@"
DELETE FROM {_schemaName}.handled
WHERE client_id = ANY(:clientIds) OR message_id = ANY(:messageIds);
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "clientIds", _clientIds.ToArray() },
                { "messageIds", _messageIds.ToArray() },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await command.ExecuteNonQueryAsync(CancellationToken.None);
    }

    private async Task DeleteClientsAsync()
    {
        var query = $@"
DELETE FROM {_schemaName}.clients
WHERE id = ANY(:clientIds);
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "clientIds", _clientIds.ToArray() },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await command.ExecuteNonQueryAsync(CancellationToken.None);
    }

    private async Task DeleteMessagesAsync()
    {
        var query = $@"
DELETE FROM {_schemaName}.messages
WHERE id = ANY(:messageIds);
";

        await using var connection = await _connectionFactory.GetConnectionAsync(CancellationToken.None);

        await using DbCommand command = new DbCommandInitializer(query, connection)
        {
            Parameters =
            {
                { "messageIds", _messageIds.ToArray() },
            }
        };

        await connection.OpenAsync(CancellationToken.None);

        await command.ExecuteNonQueryAsync(CancellationToken.None);
    }

    public sealed record MessageRow(long Id, string Type, string Payload, DateTimeOffset CreatedAt);

    public sealed record ClientRow(Guid Id, DateTimeOffset CreatedAt);

    public sealed record HandledRow(Guid ClientId, long MessageId, DateTimeOffset CreatedAt);
}
