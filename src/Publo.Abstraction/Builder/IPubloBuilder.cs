using Microsoft.Extensions.DependencyInjection;
using Publo.Abstraction.Provider;

namespace Publo.Abstraction.Builder;

/// <summary>
/// Configures Publo services and provider integrations.
/// </summary>
public interface IPubloBuilder
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Registers a concrete Publo provider implementation.
    /// </summary>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="lifetime">The lifetime used to register the provider.</param>
    IPubloBuilder UseProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TProvider : class, IPubloProvider;
}
