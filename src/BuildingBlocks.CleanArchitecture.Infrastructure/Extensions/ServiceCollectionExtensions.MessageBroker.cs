using BuildingBlocks.CleanArchitecture.Application.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Events.InboxOutbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq(
       this IServiceCollection services,
       Func<MessageBrokerSettings> settingsFactory,
       params Assembly[] consumerAssemblies)
    {
        var settings = settingsFactory();

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            foreach (var assembly in consumerAssemblies)
            {
                config.AddConsumers(assembly);
            }

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, "/", h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        if (settings.PersistEventsInDb)
        {
            services.AddScoped<IEventBus, PersistentEventBus>();
        }
        else
        {
            services.AddScoped<IEventBus, DefaultEventBus>();
        }

        return services;
    }
}
