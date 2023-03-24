using BlazorBase.CRUD.NumberSeries;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.CRUD
{
    public static class BlazorBaseNumberSeriesConfiguration
    {
        /// <summary>
        /// Register blazor base number series and configures the default behaviour.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorBaseNumberSeries(this IServiceCollection serviceCollection, params string[] allowedUserAccessRoles)
        {
            serviceCollection.AddSingleton<NoSeriesService>();

            serviceCollection.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(NoSeries), policy => policy.RequireRole(allowedUserAccessRoles));
            });

            return serviceCollection;
        }
    }
}
