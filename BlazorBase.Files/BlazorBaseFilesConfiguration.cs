using BlazorBase.CRUD.Models;
using BlazorBase.Files.Components;
using BlazorBase.Files.Controller;
using BlazorBase.Files.Models;
using BlazorBase.Files.Services;
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

    public static IServiceCollection AddBlazorBaseFiles<TOptions, TBaseFile>(this IServiceCollection serviceCollection, Action<TOptions>? configureOptions = null, params string[] allowedUserAccessRoles)
        where TOptions : class, IBlazorBaseFileOptions
        where TBaseFile : class, IBaseFile
    {
        // If options handler is not defined we will get an exception so
        // we need to initialize and empty action.
        if (configureOptions == null)
            configureOptions = (e) => { };

        serviceCollection.AddSingleton(configureOptions)
        .AddTransient<IBlazorBaseFileOptions, TOptions>()

        .AddTransient<IBaseFile, TBaseFile>()
        .AddTransient<IBasePropertyCardInput, BaseFileInput>()
        .AddTransient<IBasePropertyCardInput, MultiFileUploadInput>()
        .AddTransient<IBasePropertyListPartInput, BaseFileListPartInput>()
        .AddTransient<IBasePropertyListDisplay, BaseFileListDisplay>()
        .AddSingleton<IImageService, MagickImageService>() // As System.Drawing.Common currently not supporting webp images (loading throws error) use third party library
        .AddSingleton<IBaseFileService, BaseFileService>()


        .AddAuthorization(options =>
        {
            if (allowedUserAccessRoles.Length == 0)
                options.AddPolicy(nameof(BaseFileController), policy => policy.RequireAuthenticatedUser());
            else
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
    public static IServiceCollection AddBlazorBaseFiles<TBaseFile>(this IServiceCollection serviceCollection, Action<IBlazorBaseFileOptions>? configureOptions = null, params string[] allowedUserAccessRoles)
         where TBaseFile : class, IBaseFile
    {
        return AddBlazorBaseFiles<BlazorBaseFileOptions, TBaseFile>(serviceCollection, configureOptions, allowedUserAccessRoles);
    }

}