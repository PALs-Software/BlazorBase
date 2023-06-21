using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.DataUpgrade;

public class DataUpgradeService
{
    #region Injects
    protected readonly IServiceProvider ServiceProvider;
    protected readonly DbContext DbContext;
    #endregion

    #region Init

    public DataUpgradeService(IServiceProvider serviceProvider, DbContext dbContext)
    {
        ServiceProvider = serviceProvider;
        DbContext = dbContext;
    }

    #endregion

    public static Task StartDataUpgradeAsync(IServiceProvider serviceProvider)
    {
        var dataUpgradeService = serviceProvider.GetRequiredService<DataUpgradeService>();
        return dataUpgradeService.StartDataUpgradeAsync();
    }

    public async Task StartDataUpgradeAsync()
    {
        var dataUpgradeSteps = ServiceProvider.GetServices<IDataUpgradeStep>();
        foreach (var dataUpgradeStep in dataUpgradeSteps)
        {
            if (await DataUpgradeStepAlreadyExecutedAsync(dataUpgradeStep))
                continue;

            var startTime = DateTime.Now;
            dataUpgradeStep.Log($"Start data upgrade step {dataUpgradeStep.Id}", true);

            await dataUpgradeStep.DataUpgradeProcedure();

            var duration = DateTime.Now - startTime;
            dataUpgradeStep.Log("");
            dataUpgradeStep.Log($"Finished data upgrade step {dataUpgradeStep.Id}, execution took {duration:hh\\:mm\\:ss}");
            await SetDataUpgradeStepToExecutedAsync(dataUpgradeStep);
        }
    }

    protected Task<bool> DataUpgradeStepAlreadyExecutedAsync(IDataUpgradeStep step)
    {
        return DbContext.Set<DataUpgradeEntry>().AnyAsync(entry => entry.Id == step.Id);
    }

    protected Task SetDataUpgradeStepToExecutedAsync(IDataUpgradeStep step)
    {
        DbContext.Set<DataUpgradeEntry>().Add(new DataUpgradeEntry()
        {
            Id = step.Id,
            Description = step.Description,
            Log = step.LogText
        });

        return DbContext.SaveChangesAsync();
    }
}
