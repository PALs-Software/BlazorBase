using BlazorBase.Mailing.Models;
using BlazorBase.Mailing.Services;
using BlazorBase.Models;
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
    public static IServiceCollection AddBlazorBaseMailing<TOptions>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null)
        where TOptions : class, IBlazorBaseMailingOptions
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection
            .AddSingleton(configureOptions)
            .AddTransient<IBlazorBaseMailingOptions, TOptions>()
            .AddTransient<BaseMailService>();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor base mail handling and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    [SupportedOSPlatform("windows")]
    public static IServiceCollection AddBlazorBaseMailing(this IServiceCollection serviceCollection, Action<IBlazorBaseMailingOptions>? configureOptions = null)
    {
        return AddBlazorBaseMailing<BlazorBaseMailingOptions>(serviceCollection, configureOptions);
    }

}
