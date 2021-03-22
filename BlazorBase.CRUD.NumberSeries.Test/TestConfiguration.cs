using BlazorBase.CRUD.NumberSeries.Test.Mocks;
using BlazorBase.CRUD.Services;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries.Test
{
    public static class TestConfiguration
    {
        public static void AddTestConfiguration(TestServiceProvider services)
        {
            services.AddTransient<BaseService>();
            services.AddTransient<DbContext, DbContextMock>();
            services.AddDbContext<DbContextMock>(options => options.UseInMemoryDatabase(databaseName: "BlazorBaseTestDbContextMock"));
        }
    }
}
