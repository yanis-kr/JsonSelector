using Microsoft.Extensions.DependencyInjection;

namespace JsonSelector;

public static class JsonSelectorServiceExtensions
{
    public static IServiceCollection AddJsonSelector(this IServiceCollection services)
    {
        services.AddSingleton<IJsonSelector, JsonSelectorImpl>();
        return services;
    }
}
