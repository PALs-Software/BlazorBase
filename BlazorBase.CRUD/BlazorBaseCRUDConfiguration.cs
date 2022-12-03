using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.Translation;
using BlazorBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;

namespace BlazorBase.CRUD;
public static class BlazorBaseCRUDConfiguration
{
    /// <summary>
    /// Register blazor base crud and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseCRUD<TOptions, TDbContextImplementation>(this IServiceCollection serviceCollection, Action<TOptions> configureOptions = null)
        where TOptions : class, IBlazorBaseCRUDOptions
        where TDbContextImplementation : DbContext
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection.AddSingleton(configureOptions)
        .AddTransient<IBlazorBaseCRUDOptions, TOptions>()
        .AddSingleton<BaseParser>()
        .AddTransient<BaseService>()
        .AddTransient<DbContext, TDbContextImplementation>()
        .AddSingleton<IStringLocalizerFactory, BaseResourceManagerStringLocalizerFactory>()

        .AddBlazorBaseMessageHandling();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor base crud and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseCRUD<TDbContextImplementation>(this IServiceCollection serviceCollection, Action<IBlazorBaseCRUDOptions> configureOptions = null)
        where TDbContextImplementation : DbContext
    {
        return AddBlazorBaseCRUD<BlazorBaseCRUDOptions, TDbContextImplementation>(serviceCollection, configureOptions);
    }
}
