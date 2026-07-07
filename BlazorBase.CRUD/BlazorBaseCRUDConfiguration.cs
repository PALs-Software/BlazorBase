using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ModelServiceProviderInjection;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.Translation;
using BlazorBase.MessageHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
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
    public static IServiceCollection AddBlazorBaseCRUD<TOptions, TDbContextImplementation>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null)
        where TOptions : class, IBlazorBaseCRUDOptions
        where TDbContextImplementation : DbContext
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection.AddSingleton(configureOptions)
        .AddTransient<IBlazorBaseCRUDOptions, TOptions>()
        .AddTransient<IBaseDbContext, BaseDbContext>()
        .AddTransient<DbContext, TDbContextImplementation>()
        .AddSingleton<BaseParser>()
        .AddScoped<ScopedServiceProvider>()
        .AddSingleton<IStringLocalizerFactory, BaseResourceManagerStringLocalizerFactory>()

        .AddDataProtection();

        serviceCollection.AddBlazorBaseMessageHandling();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor base crud and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorBaseCRUD<TDbContextImplementation>(this IServiceCollection serviceCollection, Action<IBlazorBaseCRUDOptions>? configureOptions = null)
        where TDbContextImplementation : DbContext
    {
        return AddBlazorBaseCRUD<BlazorBaseCRUDOptions, TDbContextImplementation>(serviceCollection, configureOptions);
    }

    /// <summary>
    /// Initializes the static <see cref="BlazorBaseDataProtection"/> provider so that <c>EncryptString</c> /
    /// <c>DecryptString</c> extension methods can be used from contexts without dependency injection.
    /// Call once during application startup, after the <see cref="IApplicationBuilder"/> has been built.
    /// </summary>
    public static IApplicationBuilder UseBlazorBaseCRUD(this IApplicationBuilder app)
    {
        var provider = app.ApplicationServices.GetRequiredService<IDataProtectionProvider>();
        BlazorBaseDataProtection.Initialize(provider);
        return app;
    }
}
