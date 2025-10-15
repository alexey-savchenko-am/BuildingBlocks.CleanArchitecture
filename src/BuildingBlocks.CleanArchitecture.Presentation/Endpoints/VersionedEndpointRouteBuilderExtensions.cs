using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CleanArchitecture.Presentation.Endpoints;

public static class VersionedEndpointRouteBuilderExtensions
{
    public static void MapVersionedEndpoints(this WebApplication builder, ApiVersionSet versions)
    {
        var scope = builder.Services.CreateScope();

        var endpoints = scope.ServiceProvider.GetServices<IVersionedEndpoint>();

        foreach(var endpoint in endpoints)
        {
            endpoint.ConfigureRoute(builder, versions);
        }
    }
}
