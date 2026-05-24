using System;
using System.Threading;
using System.Threading.Tasks;
using Publo.Postgres.Entities;

namespace Publo.Postgres.Repositories;

internal interface IPostgresRepository
{
    Task AddAsync<T>(T message, CancellationToken cancellationToken);
    Task<Message?> GetAsync(ClientId clientId, DateTimeOffset? from, CancellationToken cancellationToken);
    Task CreateAsync(ClientId clientId, CancellationToken cancellationToken);
    Task CommitAsync(MessageId messageId, ClientId clientId, CancellationToken cancellationToken);
}
