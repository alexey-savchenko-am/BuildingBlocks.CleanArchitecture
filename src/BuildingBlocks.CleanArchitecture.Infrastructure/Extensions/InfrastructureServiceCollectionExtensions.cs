using BuildingBlocks.CleanArchitecture.Domain.Data;
using BuildingBlocks.CleanArchitecture.Infrastructure.Events;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.Connection;
using BuildingBlocks.CleanArchitecture.Infrastructure.Persistence.Database.Interceptors;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPostgres<TDbContext, TDatabaseOptionsSetup>(
        this IServiceCollection services)
        where TDbContext : DbContext
        where TDatabaseOptionsSetup: class, IConfigureOptions<DatabaseOptions>
    {
        services.ConfigureOptions<TDatabaseOptionsSetup>();
        services.AddSingleton<IDbConnectionFactory, NpgConnectionFactory>();
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

        services.AddDbContext<DbContext, TDbContext>((provider, builder) =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var outboxMessagesInterceptor = provider.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();
            builder.UseNpgsql(options.ConnectionString, actions =>
            {
                actions.EnableRetryOnFailure(options.MaxRetryCount);
                actions.CommandTimeout(options.CommandTimeout);
            });
            builder.AddInterceptors(outboxMessagesInterceptor);
            builder.EnableDetailedErrors(options.EnableDetailedErrors);
            builder.EnableSensitiveDataLogging(options.EnableSensitiveDataLogging);
        });

        return services;
    }

    public static IServiceCollection AddSession(this IServiceCollection services)
    {
        services.AddScoped<ISession, Session>();
        return services;
    }

    public static IServiceCollection AddRabbitMq(
           this IServiceCollection services,
           Func<MessageBrokerSettings> settingsFactory,
           params Assembly[] consumerAssemblies)
    {
        var settings = settingsFactory();

        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            foreach(var assembly in consumerAssemblies)
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

        return services;
    }

}
