using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.NumberSeries;
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
    public static class BlazorBaseNumberSeriesConfiguration
    {
        /// <summary>
        /// Register blazor base number series and configures the default behaviour.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddBlazorBaseNumberSeries(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<NoSeriesService>();
            return serviceCollection;
        }
    }
}
