using BlazorBase.Models;
using BlazorBase.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase;
public static class BlazorBaseConfiguration
{
    /// <summary>
    /// Register blazor base and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBase<TOptions>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null)
        where TOptions : class, IBlazorBaseOptions
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection
            .AddSingleton(configureOptions)
            .AddSingleton<BaseErrorHandler>()
            .AddTransient<IBlazorBaseOptions, TOptions>()
            .AddLocalization();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor base and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBase(this IServiceCollection serviceCollection, Action<IBlazorBaseOptions>? configureOptions = null)
    {
        return AddBlazorBase<BlazorBaseOptions>(serviceCollection, configureOptions);
    }
}
