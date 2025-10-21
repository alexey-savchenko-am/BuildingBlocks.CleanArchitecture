using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.CleanArchitecture.Presentation.Server.Swagger;

public sealed class ConfigureVersionedSwaggerOptions
    : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;
    private readonly IConfiguration _configuration;

    public ConfigureVersionedSwaggerOptions(
        IApiVersionDescriptionProvider provider,
        IConfiguration configuration)
    {
        _provider = provider;
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var swaggerSection = _configuration.GetSection("Swagger");

        var info = new OpenApiInfo()
        {
            Title = swaggerSection["Title"] ?? "Web API",
            Version = description.ApiVersion.ToString(),
            Description = swaggerSection["Description"] ?? string.Empty,
            Contact = new OpenApiContact
            {
                Name = swaggerSection["Contact:Name"],
                Email = swaggerSection["Contact:Email"]
            },
            License = new OpenApiLicense
            {
                Name = swaggerSection["License:Name"],
                Url = new Uri(swaggerSection["License:Url"] ?? "https://opensource.org/licenses/MIT")
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}