using BlazorBase.Models;
using BlazorBase.Modules;
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
    public static IServiceCollection AddBlazorBase(this IServiceCollection serviceCollection, Action<BlazorBaseOptions> configureOptions = null)
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection.AddSingleton(configureOptions)
        .AddSingleton<BlazorBaseOptions>()
        .AddSingleton<ErrorHandler>()
        .AddLocalization();

        //.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });           

        return serviceCollection;
    }
}
