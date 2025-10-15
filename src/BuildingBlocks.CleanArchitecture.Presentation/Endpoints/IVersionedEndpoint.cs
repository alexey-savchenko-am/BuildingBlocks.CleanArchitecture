using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.CleanArchitecture.Presentation.Endpoints;

public interface IVersionedEndpoint
{
    void ConfigureRoute(IEndpointRouteBuilder app, ApiVersionSet versions);
}

public interface IVersionedEndpoint<TResponse, TRequest>
    : IVersionedEndpoint
{
    Task<TResponse> HandleAsync(TRequest request);
}
