using BlazorBase.Backup.Controller;
using BlazorBase.Backup.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.Backup;

public static class BlazorBaseBackupConfiguration
{
    /// <summary>
    /// Register blazor base backup and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseBackup(this IServiceCollection serviceCollection, params string[] allowedUserAccessRoles)
    {
        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(nameof(BlazorBaseBackupFileController), policy => policy.RequireRole(allowedUserAccessRoles));
        });

        serviceCollection.AddTransient<BackupWebsiteService>();

        return serviceCollection;
    }
}
