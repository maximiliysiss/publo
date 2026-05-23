using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Publo.Abstraction.Provider;

namespace Publo.Abstraction.Builder;

internal sealed class PubloBuilder(IServiceCollection services) : IPubloBuilder
{
    public IServiceCollection Services => services;

    public IPubloBuilder UseProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TProvider : class, IPubloProvider
    {
        var serviceDescriptor = new ServiceDescriptor(
            serviceType: typeof(IPubloProvider),
            implementationType: typeof(TProvider),
            lifetime: lifetime);

        services.TryAdd(serviceDescriptor);

        return this;
    }
}
