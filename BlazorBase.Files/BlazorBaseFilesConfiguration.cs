using BlazorBase.CRUD.Models;
using BlazorBase.Files.Components;
using BlazorBase.Files.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazorBase.Files
{
    public static class BlazorBaseFilesConfiguration
    {
        /// <summary>
        /// Register blazor base file handling and configures the default behaviour.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorBaseFiles(this IServiceCollection serviceCollection, Action<BlazorBaseFileOptions> configureOptions = null)
        {
            // If options handler is not defined we will get an exception so
            // we need to initialize and empty action.
            if (configureOptions == null)
                configureOptions = (e) => { };

            serviceCollection.AddSingleton(configureOptions)
            .AddSingleton<BlazorBaseFileOptions>()

            .AddTransient<IBasePropertyCardInput, BaseFileInput>()
            .AddTransient<IBasePropertyListPartInput, BaseFileListPartInput>()
            .AddTransient<IBasePropertyListDisplay, BaseFileListDisplay>()

            .AddControllers();

            return serviceCollection;
        }

        public static void RegisterBlazorBaseFileInstance(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<BlazorBaseFileOptions>()?.SetInstance();
        }
    }
}
