using BuildingBlocks.CleanArchitecture.Application.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Events.InboxOutbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMq<TMessageBrokerOptionsSetup>(
        this IServiceCollection services,
        bool persistEventsInDb,
        params Assembly[] consumerAssemblies)
        where TMessageBrokerOptionsSetup: class, IConfigureOptions<MessageBrokerSettings>
    {
        services.ConfigureOptions<TMessageBrokerOptionsSetup>();

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            foreach (var assembly in consumerAssemblies)
            {
                config.AddConsumers(assembly);
            }

            config.UsingRabbitMq((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<MessageBrokerSettings>>().Value;

                cfg.Host(options.Host, "/", h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        if (persistEventsInDb)
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
