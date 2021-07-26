using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.NumberSeries.Test.Libraries;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Bunit;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Threading;
using Xunit;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.NumberSeries.Test.Tests
{
    public class NoSeriesTest : TestContext
    {
        protected BaseService BaseService { get; set; }
        protected NoSeriesService NoSeriesService { get; set; }
        protected IMessageHandler MessageHandler { get; set; }
        protected EventServices EventServices { get; set; }


        public NoSeriesTest()
        {
            TestConfiguration.AddTestConfiguration(Services);

            BaseService = Services.GetService<BaseService>();
            NoSeriesService = Services.GetService<NoSeriesService>();
            MessageHandler = Services.GetService<IMessageHandler>();

            EventServices = new EventServices()
            {
                ServiceProvider = Services,
                Localizer = Services.GetService<IStringLocalizer>(),
                BaseService = BaseService,
                MessageHandler = MessageHandler
            };
        }

        [Fact]
        public void TestEndingNoAutoFill()
        {
            // Setup
            var series = new NoSeries();

            // Test
            series.StartingNo = "A-000";
            series.OnAfterPropertyChanged(new OnAfterPropertyChangedArgs(series, nameof(NoSeries.StartingNo), series.StartingNo, true, EventServices));

            // Validate
            Assert.Equal("A-999", series.EndingNo);
        }

        [Fact]
        public async void TestFirstIncreaseNo()
        {
            // Setup
            var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(BaseService);

            // Test
            var nextNo = await NoSeriesService.GetNextNoAsync(BaseService, seriesId);
            var noSeries = await BaseService.GetAsync<NoSeries>(seriesId);

            // Validate
            Assert.Equal("A-000", nextNo);
            Assert.Equal("A-000", noSeries.LastNoUsed);
            Assert.Equal(999, noSeries.EndingNoNumeric);
            Assert.Equal(0, noSeries.LastNoUsedNumeric);
            Assert.Equal(3, noSeries.NoOfDigits);
        }

        [Fact]
        public async void TestRealIncreaseNo()
        {
            // Setup
            var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(BaseService);

            // Test
            await NoSeriesService.GetNextNoAsync(BaseService, seriesId);
            var nextNo = await NoSeriesService.GetNextNoAsync(BaseService, seriesId);
            var noSeries = await BaseService.GetAsync<NoSeries>(seriesId);

            // Validate
            Assert.Equal("A-001", nextNo);
            Assert.Equal("A-001", noSeries.LastNoUsed);
            Assert.Equal(1, noSeries.LastNoUsedNumeric);
        }

        [Fact]
        public async void TestIncreaseNoUntilEnd()
        {
            // Setup
            var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(BaseService);

            // Test
            String nextNo = String.Empty;
            for (int i = 0; i <= 999; i++)
                nextNo = await NoSeriesService.GetNextNoAsync(BaseService, seriesId);
            var noSeries = await BaseService.GetAsync<NoSeries>(seriesId);

            // Validate
            Assert.Equal("A-999", nextNo);
            Assert.Equal("A-999", noSeries.LastNoUsed);
            Assert.Equal(999, noSeries.LastNoUsedNumeric);
        }

        [Fact]
        public async void TestIncreaseNoOverEndingNo()
        {
            // Setup
            var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(BaseService);

            // Test
            String nextNo = String.Empty;
            for (int i = 0; i <= 999; i++)
                nextNo = await NoSeriesService.GetNextNoAsync(BaseService, seriesId);

            // Validate
            var exception = await Assert.ThrowsAsync<CRUDException>(async () => await NoSeriesService.GetNextNoAsync(BaseService, seriesId));
            Assert.Equal("The defined maximum of the no series is reached, please create a new number series", exception.Message);
        }

        [Fact]
        public async void TestNoSeriesCanNotFoundException()
        {
            // Setup
            var seriesId = await NoSeriesLibrary.AddBasicNoSeriesToDbAsync(BaseService);
            seriesId = "999";

            // Validate
            var exception = await Assert.ThrowsAsync<CRUDException>(async () => await NoSeriesService.GetNextNoAsync(BaseService, seriesId));
            Assert.Equal("Cant get next number in series, because number series can not be found with the key \"999\"", exception.Message);
        }
    }
}

