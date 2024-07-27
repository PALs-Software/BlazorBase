using BlazorBase.CRUD.NumberSeries.Test.Mocks;
using Bunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace BlazorBase.CRUD.NumberSeries.Test;

public static class TestConfiguration
{
    public static void AddTestConfiguration(TestServiceProvider services)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
        CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentCulture;

        services.AddDbContext<DbContextMock>(options => options.UseInMemoryDatabase(databaseName: "BlazorBaseTestDbContextMock"));
        services.AddBlazorBaseCRUD<DbContextMock>(options =>
        {
            options.UseAsyncDbContextMethodsPerDefaultInBaseDbContext = false;
        });

        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
            options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("en") };
        });

        services.AddSingleton<NoSeriesService>();
    }
}
