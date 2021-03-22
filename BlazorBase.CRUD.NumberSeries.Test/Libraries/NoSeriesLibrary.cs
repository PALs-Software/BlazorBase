using BlazorBase.CRUD.NumberSeries.Test.Mocks;
using BlazorBase.CRUD.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries.Test.Libraries
{
    public static class NoSeriesLibrary
    {

        public static async Task<string> AddBasicNoSeriesToDbAsync(BaseService baseService)
        {
            var entry = new NoSeries()
            {
                Id = "000",
                StartingNo = "A-000",
                EndingNo = "A-999"
            };

            await baseService.AddEntryAsync(entry);

            return entry.Id;
        }
    }
}
