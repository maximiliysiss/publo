# Publo.Abstraction

Core contracts and dependency injection helpers for Publo broadcast events.

Use this package when you need the common publishing and handling APIs for notifying every running
pod or application instance, or when you want to implement a custom Publo provider.

## Installation

```bash
dotnet add package Publo.Abstraction
```

## Public API

`IPubloService` is the application-facing service used to publish broadcast events:

```csharp
using Publo.Abstraction.Services;

public sealed class UsersService(IPubloService publo)
{
    public Task PublishAsync(Guid userId, CancellationToken cancellationToken)
        => publo.SendAsync(new UserCreated(userId), cancellationToken);
}

public sealed record UserCreated(Guid UserId);
```

`IPubloExecutor<T>` handles received broadcast events:

```csharp
using Publo.Abstraction.Executor;

public sealed class UserCreatedExecutor : IPubloExecutor<UserCreated>
{
    public Task HandleAsync(UserCreated message, CancellationToken cancellationToken)
    {
        // React to the broadcast event here.
        return Task.CompletedTask;
    }
}
```

`AddPublo` registers the publishing service and configures a provider. `AddPubloExecutor` registers
event handlers:

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
        // Publish or persist the broadcast event here.
        return Task.CompletedTask;
    }
}

builder.Services.AddPublo(publo => publo.UseProvider<InMemoryPubloProvider>());
```

Provider implementations are responsible for delivering received events to registered
`IPubloExecutor<T>` implementations.
