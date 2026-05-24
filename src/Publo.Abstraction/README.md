# Publo.Abstraction

Core contracts and dependency injection helpers for Publo.

Use this package when you need the common sending and handling APIs, or when you want to implement a
custom Publo provider.

## Installation

```bash
dotnet add package Publo.Abstraction
```

## Public API

`IPubloService` is the application-facing service used to send messages:

```csharp
using Publo.Abstraction.Services;

public sealed class UsersService(IPubloService publo)
{
    public Task PublishAsync(Guid userId, CancellationToken cancellationToken)
        => publo.SendAsync(new UserCreated(userId), cancellationToken);
}

public sealed record UserCreated(Guid UserId);
```

`IPubloExecutor<T>` handles received messages:

```csharp
using Publo.Abstraction.Executor;

public sealed class UserCreatedExecutor : IPubloExecutor<UserCreated>
{
    public Task HandleAsync(UserCreated message, CancellationToken cancellationToken)
    {
        // Handle the message here.
        return Task.CompletedTask;
    }
}
```

`AddPublo` registers the sending service and configures a provider. `AddPubloExecutor` registers
message handlers:

```csharp
using Publo.Abstraction.Extensions;
using Publo.Kafka.Extensions;

builder.Services
    .AddPublo(publo => publo.UseKafka())
    .AddPubloExecutor<UserCreated, UserCreatedExecutor>();
```

## Custom Providers

Provider packages implement `IPubloProvider` and register it through `IPubloBuilder.UseProvider`.

```csharp
using Publo.Abstraction.Builder;
using Publo.Abstraction.Provider;

public sealed class InMemoryPubloProvider : IPubloProvider
{
    public Task SendAsync<T>(T message, CancellationToken cancellationToken)
    {
        // Send or persist the message here.
        return Task.CompletedTask;
    }
}

builder.Services.AddPublo(publo => publo.UseProvider<InMemoryPubloProvider>());
```

Provider implementations are responsible for delivering received messages to registered
`IPubloExecutor<T>` implementations.
