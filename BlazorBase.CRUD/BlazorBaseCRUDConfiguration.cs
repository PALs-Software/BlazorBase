using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
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
    public static class BlazorBaseCRUDConfiguration
    {
        /// <summary>
        /// Register blazor base crud and configures the default behaviour.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorBaseCRUD<TDbContextImplementation>(this IServiceCollection serviceCollection, Action<BlazorBaseCRUDOptions> configureOptions = null) where TDbContextImplementation : DbContext
        {
            // If options handler is not defined we will get an exception so
            // we need to initialize and empty action.
            if (configureOptions == null)
                configureOptions = (e) => { };

            serviceCollection.AddSingleton(configureOptions)
            .AddSingleton<BlazorBaseCRUDOptions>()
            .AddSingleton<GenericClassStringLocalizer>()
            .AddTransient<BaseService>()
            .AddTransient<DbContext, TDbContextImplementation>();
            

            return serviceCollection;
        }
    }
}
