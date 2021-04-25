using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.Files.Components;
using BlazorBase.Files.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD
{
    public static class BlazorBaseFilesConfiguration
    {
        /// <summary>
        /// Register blazor base number series and configures the default behaviour.
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
            .AddTransient<IBasePropertyListDisplay, BaseFileListDisplay>();

            return serviceCollection;
        }
    }
}
