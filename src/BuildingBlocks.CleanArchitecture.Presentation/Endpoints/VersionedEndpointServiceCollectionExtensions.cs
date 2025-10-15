using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CleanArchitecture.Presentation.Endpoints;

public static class VersionedEndpointServiceCollectionExtensions
{
    public static IServiceCollection AddVersionedEndpoints(this IServiceCollection services)
    {
        var endpoints = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => t.GetInterfaces().Contains(typeof(IVersionedEndpoint)))
            .Where(t => !t.IsInterface);

        foreach(var endpoint in endpoints)
        {
            services.AddScoped(typeof(IVersionedEndpoint), endpoint);
        }

        return services;
    }
}
