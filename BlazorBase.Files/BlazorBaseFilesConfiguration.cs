using BlazorBase.CRUD.Models;
using BlazorBase.Files.Components;
using BlazorBase.Files.Controller;
using BlazorBase.Files.Models;
using BlazorBase.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase.Files;
public static class BlazorBaseFilesConfiguration
{
    /// <summary>
    /// Register the blazore base file options instance
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static void RegisterBlazorBaseFileOptionsInstance(this IApplicationBuilder app)
    {
        _ = app.ApplicationServices.GetService<IBlazorBaseFileOptions>(); // Just getting options on startup so constructor will fill up the static instance for later use
    }

    /// <summary>
    /// Register blazor base file handling and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>

    public static IServiceCollection AddBlazorBaseFiles<TOptions>(this IServiceCollection serviceCollection, Action<TOptions> configureOptions = null, params string[] allowedUserAccessRoles)
        where TOptions : class, IBlazorBaseFileOptions
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection.AddSingleton(configureOptions)
        .AddTransient<IBlazorBaseFileOptions, TOptions>()

        .AddTransient<IBasePropertyCardInput, BaseFileInput>()
        .AddTransient<IBasePropertyListPartInput, BaseFileListPartInput>()
        .AddTransient<IBasePropertyListDisplay, BaseFileListDisplay>()

        .AddAuthorization(options =>
        {
            options.AddPolicy(nameof(BaseFileController), policy => policy.RequireRole(allowedUserAccessRoles));
        })

        .AddControllers().AddApplicationPart(typeof(Controller.BaseFileController).Assembly).AddControllersAsServices();

        return serviceCollection;
    }

    /// <summary>
    /// Register blazor base file handling and configures the default behaviour.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>

    public static IServiceCollection AddBlazorBaseFiles(this IServiceCollection serviceCollection, Action<IBlazorBaseFileOptions> configureOptions = null, params string[] allowedUserAccessRoles)
    {
        return AddBlazorBaseFiles<BlazorBaseFileOptions>(serviceCollection, configureOptions, allowedUserAccessRoles);
    }

}