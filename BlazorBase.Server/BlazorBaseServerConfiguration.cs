using BlazorBase.Server.Services;
using BlazorBase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.Server;

public static class BlazorBaseServerConfiguration
{
    /// <summary>
    /// Register blazor base server services.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseServer(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IBaseAuthenticationService, BaseAuthenticationService>();

        return serviceCollection;
    }
}
