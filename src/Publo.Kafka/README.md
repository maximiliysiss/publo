# Publo.Kafka

Kafka provider for Publo broadcast events. It publishes events to one Kafka topic and runs a hosted
consumer that dispatches events to `IPubloExecutor<T>` handlers in each running pod or application
instance.

The provider uses `Confluent.Kafka` and stores the .NET message type in the Kafka message key so the
consumer can resolve the matching executor.

Use this provider when your services already have Kafka-compatible infrastructure and you want an
easy way to notify every running pod about cluster-wide events such as cache invalidations,
configuration refreshes, and local state updates.

## Installation

```bash
dotnet add package Publo.Kafka
```

## Registration

```csharp
using Publo.Abstraction.Extensions;
using Publo.Kafka.Extensions;

builder.Services
    .AddPublo(publo => publo.UseKafka())
    .AddPubloExecutor<UserCreated, UserCreatedExecutor>();
```

## Configuration

`UseKafka` binds options from the `KafkaPubloOptions` configuration section:

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

Options:

| Name | Default | Description |
| --- | --- | --- |
| `Topic` | empty string | Kafka topic used for producing and consuming Publo broadcast events. |
| `SkipPolicy` | `Soft` | Controls how unsupported messages are handled. |
| `ConsumerConfig` | empty dictionary | Values passed to `Confluent.Kafka.ConsumerConfig`. |
| `ProducerConfig` | empty dictionary | Values passed to `Confluent.Kafka.ProducerConfig`. |

`SkipPolicy.Strict` throws when the consumer cannot read, resolve, or deserialize a message.
`SkipPolicy.Soft` skips unsupported messages when possible and continues processing.

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

## Local Kafka

The repository `docker-compose.yaml` starts Redpanda on `localhost:9092` and creates a `test` topic:

```bash
docker compose up -d kafka redpanda-setup redpanda-console
```

Redpanda Console is exposed at `http://localhost:9000`.
