using BlazorBase.CRUD.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        public static IServiceCollection AddBlazorBaseCRUD(this IServiceCollection serviceCollection, Action<BlazorBaseCRUDOptions> configureOptions = null)
        {
            // If options handler is not defined we will get an exception so
            // we need to initialize and empty action.
            if (configureOptions == null)
                configureOptions = (e) => { };

            serviceCollection.AddSingleton(configureOptions);
            serviceCollection.AddSingleton<BlazorBaseCRUDOptions>();

            return serviceCollection;
        }
    }
}
