using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Publo.Abstraction.Builder;
using Publo.Abstraction.Executor;
using Publo.Abstraction.Services;

namespace Publo.Abstraction.Extensions;

/// <summary>
/// Provides dependency injection registration methods for Publo.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Publo services and applies provider configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="action">The action used to configure the Publo builder.</param>
    public static IServiceCollection AddPublo(this IServiceCollection services, Action<IPubloBuilder> action)
    {
        var publoBuilder = new PubloBuilder(services);

        action(publoBuilder);

        services.TryAddScoped<IPubloService, PubloService>();

        return services;
    }

    /// <summary>
    /// Registers an executor for messages of the specified type.
    /// </summary>
    /// <typeparam name="TMessage">The message type handled by the executor.</typeparam>
    /// <typeparam name="TExecutor">The executor implementation type.</typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="lifetime">The lifetime used to register the executor.</param>
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
