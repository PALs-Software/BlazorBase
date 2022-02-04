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
    public static IServiceCollection AddBlazorBase<TOptions>(this IServiceCollection serviceCollection, Action<TOptions> configureOptions = null)
        where TOptions : class, IBlazorBaseOptions
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection
            .AddSingleton(configureOptions)
            .AddSingleton<IBlazorBaseOptions, TOptions>()
            .AddSingleton<BaseErrorHandler>()
            .AddLocalization();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor base and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBase(this IServiceCollection serviceCollection, Action<IBlazorBaseOptions> configureOptions = null)
    {
        return AddBlazorBase<BlazorBaseOptions>(serviceCollection, configureOptions);
    }

    /// <summary>
    /// Register blazor base and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBase<TOptions>(this IServiceCollection serviceCollection, Type optionsImportFromDatabaseEntryType)
        where TOptions : class, IBlazorBaseOptions
    {
        return AddBlazorBase<TOptions>(serviceCollection, options =>
        {
            options.OptionsImportMode = BaseOptionsImportMode.Database;
            options.OptionsImportFromDatabaseEntryType = optionsImportFromDatabaseEntryType;
        });
    }

    /// <summary>
    /// Register blazor base and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBase(this IServiceCollection serviceCollection, Type optionsImportFromDatabaseEntryType)
    {
        return AddBlazorBase<BlazorBaseOptions>(serviceCollection, optionsImportFromDatabaseEntryType);
    }

    
}
