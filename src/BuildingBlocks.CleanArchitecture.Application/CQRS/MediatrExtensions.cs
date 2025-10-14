using BuildingBlocks.CleanArchitecture.Application.CQRS.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.CleanArchitecture.Application.CQRS;

public static class MediatrExtensions
{
    public static void AddMeiatrWithBehaviors(
        this IServiceCollection services,
        Func<MediatrSettings> mediatrSettingsFactory,
        params Assembly[] assemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);

            var settings = mediatrSettingsFactory();

            if (settings.UseValidationBehavior)
            {
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            }

            if (settings.UseLoggingBehavior)
            {
                config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            }
        });
    }
}

public class MediatrSettings
{
    public MediatrSettings(bool useLoggingBehavior, bool useValidationBehavior)
    {
        UseLoggingBehavior = useLoggingBehavior;
        UseValidationBehavior = useValidationBehavior;
    }
    public bool UseLoggingBehavior { get; set; } = false;
    public bool UseValidationBehavior { get; set; } = false;
}