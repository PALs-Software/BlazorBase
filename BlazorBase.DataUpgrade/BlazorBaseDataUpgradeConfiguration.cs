
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.DataUpgrade;

public static class BlazorBaseDataUpgradeConfiguration
{
    /// <summary>
    /// Register blazor base data upgrade handling and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>

    public static IServiceCollection AddBlazorBaseDataUpgrade(this IServiceCollection serviceCollection, string[] allowedUserAccessRoles, params Type[] dataUpgradeSteps)
    {
        serviceCollection.AddSingleton<DataUpgradeService>();

        foreach (var dataUpgradeStep in dataUpgradeSteps)
        {
            if (dataUpgradeStep.GetInterface(nameof(IDataUpgradeStep)) == null)
                throw new ArgumentException($"The data upgrade step {dataUpgradeStep.FullName} must implement the interface IDataUpgradeStep");

            serviceCollection.AddSingleton(typeof(IDataUpgradeStep), dataUpgradeStep);
        }

        serviceCollection.AddAuthorization(options =>
        {
            options.AddPolicy(nameof(DataUpgradeEntry), policy => policy.RequireRole(allowedUserAccessRoles));
        });

        return serviceCollection;
    }
}