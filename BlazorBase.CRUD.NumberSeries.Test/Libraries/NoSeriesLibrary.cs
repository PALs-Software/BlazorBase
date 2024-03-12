using BlazorBase.CRUD.Services;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries.Test.Libraries;

public static class NoSeriesLibrary
{

    public static async Task<string> AddBasicNoSeriesToDbAsync(IBaseDbContext dbContext)
    {
        var entry = new NoSeries()
        {
            Id = "000",
            StartingNo = "A-000",
            EndingNo = "A-999"
        };

        await dbContext.AddAsync(entry);

        return entry.Id;
    }
}
