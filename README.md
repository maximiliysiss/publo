<div align="center">
  <img src="assets/publo-icon.png" alt="Publo" width="120" height="120">
  <h1>Publo</h1>
  <p>A small .NET messaging abstraction with provider packages for Kafka and PostgreSQL.</p>

  <p>
    <a href="https://github.com/maximiliysiss/publo/actions/workflows/dotnet.yml"><img alt="Build" src="https://github.com/maximiliysiss/publo/actions/workflows/dotnet.yml/badge.svg"></a>
    <a href="LICENSE"><img alt="License" src="https://img.shields.io/github/license/maximiliysiss/publo"></a>
    <img alt=".NET target frameworks" src="https://img.shields.io/badge/.NET-netstandard2.0%20%7C%20net8.0-512BD4">
  </p>
</div>

The core package exposes one sending API, `IPubloService`, and one handler contract,
`IPubloExecutor<T>`. Provider packages decide where messages are stored or delivered and run hosted
services that dispatch received messages to registered executors.

## Packages

| Package | NuGet | Purpose | Target framework |
| --- | --- | --- | --- |
| `Publo.Abstraction` | [![NuGet](https://img.shields.io/nuget/v/Publo.Abstraction?label=version)](https://www.nuget.org/packages/Publo.Abstraction) [![Downloads](https://img.shields.io/nuget/dt/Publo.Abstraction?label=downloads)](https://www.nuget.org/packages/Publo.Abstraction) | Core service, handler, provider, and DI registration contracts. | `netstandard2.0` |
| `Publo.Kafka` | [![NuGet](https://img.shields.io/nuget/v/Publo.Kafka?label=version)](https://www.nuget.org/packages/Publo.Kafka) [![Downloads](https://img.shields.io/nuget/dt/Publo.Kafka?label=downloads)](https://www.nuget.org/packages/Publo.Kafka) | Kafka producer and consumer provider built on `Confluent.Kafka`. | `net8.0` |
| `Publo.Postgres` | [![NuGet](https://img.shields.io/nuget/v/Publo.Postgres?label=version)](https://www.nuget.org/packages/Publo.Postgres) [![Downloads](https://img.shields.io/nuget/dt/Publo.Postgres?label=downloads)](https://www.nuget.org/packages/Publo.Postgres) | PostgreSQL-backed message store and polling runner. | `net8.0` |

## Basic Usage

Register Publo with a provider and register one executor per message type:

```csharp
using Publo.Abstraction.Executor;
using Publo.Abstraction.Extensions;
using Publo.Kafka.Extensions;

builder.Services
    .AddPublo(publo => publo.UseKafka())
    .AddPubloExecutor<UserCreated, UserCreatedExecutor>();

public sealed record UserCreated(Guid UserId);

public sealed class UserCreatedExecutor : IPubloExecutor<UserCreated>
{
    public Task HandleAsync(UserCreated message, CancellationToken cancellationToken)
    {
        // Handle the message here.
        return Task.CompletedTask;
    }
}
```

Send messages through `IPubloService`:

```csharp
using Publo.Abstraction.Services;

public sealed class UsersController(IPubloService publo)
{
    public Task CreateAsync(Guid userId, CancellationToken cancellationToken)
        => publo.SendAsync(new UserCreated(userId), cancellationToken);
}
```

## Provider Configuration

Kafka configuration is bound from the `KafkaPubloOptions` section:

```json
{
  "KafkaPubloOptions": {
    "topic": "events",
    "skipPolicy": "Soft",
    "consumerConfig": {
      "bootstrap.servers": "localhost:9092",
      "auto.offset.reset": "Latest"
    },
    "producerConfig": {
      "bootstrap.servers": "localhost:9092"
    }
  }
}
```

PostgreSQL configuration is bound from the `PostgresPubloOptions` section and also requires an
`IConnectionFactory` implementation:

```csharp
using System.Data.Common;
using Npgsql;
using Publo.Postgres.Infrastructure.Database;

public sealed class ConnectionFactory(IConfiguration configuration) : IConnectionFactory
{
    public string GetConnectionString()
        => configuration.GetConnectionString("Postgres")
           ?? throw new InvalidOperationException("Postgres connection string not found.");

    public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
        => Task.FromResult<DbConnection>(new NpgsqlConnection(GetConnectionString()));
}
```

```csharp
using Publo.Abstraction.Extensions;
using Publo.Postgres.Extensions;

builder.Services.AddPublo(publo => publo.UseNpgsql<ConnectionFactory>());
```

## Local Infrastructure

The repository includes `docker-compose.yaml` with Redpanda-compatible Kafka and PostgreSQL services:

```bash
docker compose up -d
```

The integration tests use Kafka on `localhost:9092`, PostgreSQL on `localhost:5432`, and the
PostgreSQL credentials from the compose file.
