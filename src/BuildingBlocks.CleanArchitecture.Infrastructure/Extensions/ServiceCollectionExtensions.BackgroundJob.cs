using BuildingBlocks.CleanArchitecture.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace BuildingBlocks.CleanArchitecture.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddJob<TJob>(
        this IServiceCollection services,
         TimeSpan? interval = null)
        where TJob : IJob
    {
        var schedule = interval ?? TimeSpan.FromSeconds(15);

        services.AddQuartz(config =>
        {
            var jobKey = new JobKey(typeof(TJob).Name);

            config.AddJob<TJob>(opts => opts.WithIdentity(jobKey));

            config.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity($"{typeof(TJob).Name}-trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(schedule).RepeatForever()));
        });

        return services;
    }

    /// <summary>
    /// Applies all registered jobs and starts the scheduler.
    /// Don't forget to call this method.
    /// </summary>
    public static void ApplyJobs(this IServiceCollection services, bool waitForJobsToComplete = true)
    {
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = waitForJobsToComplete;
        });
    }

    public static IServiceCollection AddOutboxJob(this IServiceCollection services, TimeSpan? interval = null)
    {
        services.AddJob<ProcessOutboxMessagesJob>(interval);    
        return services;
    }

    public static IServiceCollection AddInboxJob(this IServiceCollection services, TimeSpan? interval = null)
    {
        services.AddJob<ProcessInboxMessagesJob>(interval);
        return services;
    }
}
