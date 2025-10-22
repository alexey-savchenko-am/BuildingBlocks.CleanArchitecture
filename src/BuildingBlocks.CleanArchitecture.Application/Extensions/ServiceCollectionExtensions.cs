using BuildingBlocks.CleanArchitecture.Application.Auth.JWT;
using BuildingBlocks.CleanArchitecture.Application.CQRS.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.CleanArchitecture.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCqrsHandlers(
        this IServiceCollection services,
        Func<CqrsHandlerSettings> handlersSettingsFactory,
        params Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);

            var settings = handlersSettingsFactory();

            if (settings.ValidateHandlerData)
            {
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            }

            if (settings.LogHandlerExecution)
            {
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            }
        });

        return services;
    }

    public static IServiceCollection AddJwtSupport(this IServiceCollection services)
    {
        services.AddSingleton<IJwtTokenGenerator, JwtTokenService>();
        services.AddSingleton<IJwtTokenValidator, JwtTokenService>();

        return services;
    }
}

public record CqrsHandlerSettings(bool LogHandlerExecution = true, bool ValidateHandlerData = true);