using Microsoft.Extensions.DependencyInjection;

namespace JsonSelector;

/// <summary>
/// Extension methods for registering JsonSelector services.
/// </summary>
public static class JsonSelectorServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IJsonSelector"/> as a singleton in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJsonSelector(this IServiceCollection services)
    {
        services.AddSingleton<IJsonSelector, JsonSelectorImpl>();
        return services;
    }
}
