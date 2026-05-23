using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Publo.Abstraction.Builder;
using Publo.Abstraction.Executor;
using Publo.Abstraction.Services;

namespace Publo.Abstraction.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublo(this IServiceCollection services, Action<IPubloBuilder> action)
    {
        var publoBuilder = new PubloBuilder(services);

        action(publoBuilder);

        services.TryAddScoped<IPubloService, PubloService>();

        return services;
    }

    public static IServiceCollection AddPubloExecutor<TMessage, TExecutor>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TMessage : class
        where TExecutor : class, IPubloExecutor<TMessage>
    {
        var serviceDescriptor = new ServiceDescriptor(
            serviceType: typeof(IPubloExecutor<TMessage>),
            implementationType: typeof(TExecutor),
            lifetime: lifetime);

        services.TryAdd(serviceDescriptor);

        return services;
    }
}
