using BlazorBase.Mailing.Models;
using BlazorBase.Mailing.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Versioning;

namespace BlazorBase.Mailing;
public static class BlazorBaseMailingConfiguration
{
    /// <summary>
    /// Register blazor base mail handling and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static IServiceCollection AddBlazorBaseMailing(this IServiceCollection serviceCollection, Action<IBlazorBaseMailingOptions>? configureOptions = null)
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection
            .AddSingleton(configureOptions)
            .AddSingleton<BlazorBaseMailingOptions>()
            .AddTransient<BaseMailService>();

        return serviceCollection;
    }
}
