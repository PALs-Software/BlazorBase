using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.NumberSeries.Test.Libraries;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BlazorBase.CRUD.NumberSeries.Test.Tests;

public class NoSeriesTest : TestContext
{
    protected IBaseDbContext DbContext { get; set; }
    protected NoSeriesService NoSeriesService { get; set; }
    protected EventServices EventServices { get; set; }


    public NoSeriesTest()
    {
        TestConfiguration.AddTestConfiguration(Services);

        DbContext = Services.GetRequiredService<IBaseDbContext>();
        NoSeriesService = Services.GetRequiredService<NoSeriesService>();

        EventServices = new EventServices(Services, DbContext, Services.GetRequiredService<IStringLocalizer>());
    }

    [Fact]
    public void TestEndingNoAutoFill()
    {
        // Setup
        var series = new NoSeries();

        // Test
        series.StartingNo = "A-000";
        series.OnAfterPropertyChanged(new OnAfterPropertyChangedArgs(series, nameof(NoSeries.StartingNo), series.StartingNo, null, true, EventServices));

        // Validate
        Assert.Equal("A-999", series.EndingNo);
    }

    [Fact]
    public async Task TestFirstIncreaseNoAsync()
    {
        // Setup
        var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(DbContext);

        // Test
        var nextNo = await NoSeriesService.GetNextNoAsync(DbContext, seriesId);
        var noSeries = await DbContext.FindAsync<NoSeries>(seriesId);

        // Validate
        Assert.Equal("A-000", nextNo);
        Assert.Equal("A-000", noSeries?.LastNoUsed);
        Assert.Equal(999, noSeries?.EndingNoNumeric);
        Assert.Equal(0, noSeries?.LastNoUsedNumeric);
        Assert.Equal(3, noSeries?.NoOfDigits);
    }

    [Fact]
    public async Task TestRealIncreaseNoAsync()
    {
        // Setup
        var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(DbContext);

        // Test
        await NoSeriesService.GetNextNoAsync(DbContext, seriesId);
        var nextNo = await NoSeriesService.GetNextNoAsync(DbContext, seriesId);
        var noSeries = await DbContext.FindAsync<NoSeries>(seriesId);

        // Validate
        Assert.Equal("A-001", nextNo);
        Assert.Equal("A-001", noSeries?.LastNoUsed);
        Assert.Equal(1, noSeries?.LastNoUsedNumeric);
    }

    [Fact]
    public async Task TestIncreaseNoUntilEndAsync()
    {
        // Setup
        var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(DbContext);

        // Test
        String nextNo = String.Empty;
        for (int i = 0; i <= 999; i++)
            nextNo = await NoSeriesService.GetNextNoAsync(DbContext, seriesId);
        var noSeries = await DbContext.FindAsync<NoSeries>(seriesId);

        // Validate
        Assert.Equal("A-999", nextNo);
        Assert.Equal("A-999", noSeries?.LastNoUsed);
        Assert.Equal(999, noSeries?.LastNoUsedNumeric);
    }

    [Fact]
    public async Task TestIncreaseNoOverEndingNoAsync()
    {
        // Setup
        var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(DbContext);

        // Test
        String nextNo = String.Empty;
        for (int i = 0; i <= 999; i++)
            nextNo = await NoSeriesService.GetNextNoAsync(DbContext, seriesId);

        // Validate
        var exception = await Assert.ThrowsAsync<CRUDException>(async () => await NoSeriesService.GetNextNoAsync(DbContext, seriesId));
        Assert.Equal("The defined maximum of the no series is reached, please create a new number series", exception.Message);
    }

    [Fact]
    public async Task TestNoSeriesCanNotFoundExceptionAsync()
    {
        // Setup
        var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(DbContext);
        seriesId = "999";

        // Validate
        var exception = await Assert.ThrowsAsync<CRUDException>(async () => await NoSeriesService.GetNextNoAsync(DbContext, seriesId));
        Assert.Equal("Cant get next number in series, because number series can not be found with the key \"999\"", exception.Message);
    }
}

