using Microsoft.Extensions.DependencyInjection;
using Publo.Abstraction.Provider;

namespace Publo.Abstraction.Builder;

public interface IPubloBuilder
{
    IServiceCollection Services { get; }
    IPubloBuilder UseProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TProvider : class, IPubloProvider;
}
