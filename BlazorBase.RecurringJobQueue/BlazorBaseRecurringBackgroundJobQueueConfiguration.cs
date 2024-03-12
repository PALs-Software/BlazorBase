using BlazorBase.CRUD.Models;
using BlazorBase.RecurringBackgroundJobQueue.Abstracts;
using BlazorBase.RecurringBackgroundJobQueue.Components;
using BlazorBase.RecurringBackgroundJobQueue.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.RecurringBackgroundJobQueue;

public static class BlazorBaseRecurringBackgroundJobQueueConfiguration
{
    /// <summary>
    /// Register blazor base recurring background job queue and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>

    public static IServiceCollection AddBlazorBaseRecurringBackgroundJob(this IServiceCollection serviceCollection, string[] allowedUserAccessRoles, params Type[] recurringBackgroundJobs)
    {
        serviceCollection.AddSingleton<Services.RecurringBackgroundJobQueue>();

        foreach (var job in recurringBackgroundJobs)
        {
            if (job.GetInterface(nameof(IRecurringBackgroundJob)) == null)
                throw new ArgumentException($"The recurring background job {job.FullName} must implement the interface IRecurringBackgroundJob");

            serviceCollection.AddTransient(typeof(IRecurringBackgroundJob), job);
        }

        serviceCollection.AddAuthorizationBuilder()
            .AddPolicy(nameof(RecurringBackgroundJobEntry), policy => policy.RequireRole(allowedUserAccessRoles));

        serviceCollection.AddTransient<IBasePropertyCardInput, RecurringBackgroundJobLog>();
        return serviceCollection;
    }
}