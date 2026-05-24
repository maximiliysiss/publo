using System;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    private readonly ILogger<PostgresRepository> _logger;

    public PostgresRepository(
        IConnectionFactory connectionFactory,
        IOptions<PostgresPubloOptions> options,
        IDateTimeProvider dateTimeProvider,
        ILogger<PostgresRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task AddAsync<T>(T message, CancellationToken cancellationToken)
    {
        var sql = $@"
INSERT INTO {_options.SchemaName}.messages (type, created_at, payload)
VALUES (:type, :createdAt, :payload)";

        var messageType = typeof(T).GetVersionFreeFullName();
        _logger.AddingPostgresMessage(messageType);

        await using var connection = await _connectionFactory.GetConnectionAsync(cancellationToken);

        await using DbCommand command = new DbCommandInitializer(sql, connection)
        {
            Parameters =
            {
                { "type", messageType },
                { "createdAt", _dateTimeProvider.GetNow() },
                { "payload", JsonSerializer.Serialize(message), NpgsqlDbType.Jsonb },
            }
        };

        await connection.OpenAsync(cancellationToken);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.PostgresMessageAdded(messageType);
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
        {
            _logger.NoPendingPostgresMessage(clientId.Value);
            return null;
        }

        var messageId = reader.GetInt64("id");
        _logger.PostgresMessageSelected(messageId, clientId.Value);

        var typeName = reader.GetString("type");
        var type = Type.GetType(typeName);
        if (type is null)
        {
            _logger.TypeNotFound(typeName);
            throw new InvalidOperationException($"Type '{typeName}' not found");
        }

        var message = JsonSerializer.Deserialize(reader.GetString("payload"), type);
        if (message is null)
        {
            _logger.ValueNotFound(typeName);
            throw new InvalidOperationException($"Message payload not found");
        }

        return new Message(
            Id: new MessageId(messageId),
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

        _logger.CreatingPostgresClient(clientId.Value);

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
        _logger.PostgresClientCreated(clientId.Value);
    }

    public async Task CommitAsync(MessageId messageId, ClientId clientId, CancellationToken cancellationToken)
    {
        var sql = $@"
INSERT INTO {_options.SchemaName}.handled(client_id, message_id, created_at)
VALUES (:clientId, :messageId, :createdAt);
";

        _logger.CommittingPostgresMessage(messageId.Value, clientId.Value);

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
        _logger.PostgresMessageCommitted(messageId.Value, clientId.Value);
    }
}
