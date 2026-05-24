# Publo.Postgres

PostgreSQL provider for Publo broadcast events. It stores published events in PostgreSQL, runs
migrations for the Publo schema, and starts a hosted runner that polls stored events and dispatches
them to `IPubloExecutor<T>` handlers in each running pod or application instance.

## Installation

```bash
dotnet add package Publo.Postgres
```

## Registration

Register Publo with the PostgreSQL provider and supply an `IConnectionFactory` implementation:

```csharp
using System.Data.Common;
using Npgsql;
using Publo.Abstraction.Extensions;
using Publo.Postgres.Extensions;
using Publo.Postgres.Infrastructure.Database;

builder.Services
    .AddPublo(publo => publo.UseNpgsql<ConnectionFactory>())
    .AddPubloExecutor<UserCreated, UserCreatedExecutor>();

public sealed class ConnectionFactory(IConfiguration configuration) : IConnectionFactory
{
    public string GetConnectionString()
        => configuration.GetConnectionString("Postgres")
           ?? throw new InvalidOperationException("Postgres connection string not found.");

    public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        => Task.FromResult<DbConnection>(new NpgsqlConnection(GetConnectionString()));
}
```

`UseNpgsql<T>` registers the provider, repository, migrations, options binding, and polling hosted
service.

Use this provider when you want cluster-wide notifications without introducing Kafka. Each running
instance can poll the PostgreSQL-backed event log and react locally to events such as cache
invalidations, configuration refreshes, and other broadcast signals.

## Configuration

`UseNpgsql<T>` binds options from the `PostgresPubloOptions` configuration section:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=pwd;"
  },
  "PostgresPubloOptions": {
    "schemaName": "publo",
    "pollingInterval": "00:00:05",
    "offsetPolicy": "Latest"
  }
}
```

Options:

| Name | Default | Description |
| --- | --- | --- |
| `SchemaName` | `publo` | PostgreSQL schema used for Publo event tables and migration metadata. |
| `PollingInterval` | `00:00:05` | Delay between polling attempts after the current batch is drained. |
| `OffsetPolicy` | `Latest` | Controls where a new runner starts reading events. |

`OffsetPolicy.Latest` starts from events created after the runner starts. `OffsetPolicy.Earliest`
starts from the earliest uncommitted event available to the runner.

## Publishing And Handling

```csharp
using Publo.Abstraction.Executor;
using Publo.Abstraction.Services;

public sealed record UserCreated(Guid UserId);

public sealed class UserCreatedExecutor : IPubloExecutor<UserCreated>
{
    public Task HandleAsync(UserCreated message, CancellationToken cancellationToken)
    {
        // React to the broadcast event here.
        return Task.CompletedTask;
    }
}

public sealed class UsersService(IPubloService publo)
{
    public Task PublishAsync(Guid userId, CancellationToken cancellationToken)
        => publo.SendAsync(new UserCreated(userId), cancellationToken);
}
```

## Local PostgreSQL

The repository `docker-compose.yaml` starts PostgreSQL on `localhost:5432`:

```bash
docker compose up -d postgres
```

Default local credentials are `postgres` / `pwd`, database `postgres`.
