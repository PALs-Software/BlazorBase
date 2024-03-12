using BlazorBase.RecurringBackgroundJobQueue.Abstracts;
using BlazorBase.RecurringBackgroundJobQueue.Models;
using BlazorBase.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BlazorBase.RecurringBackgroundJobQueue.Services;

public class RecurringBackgroundJobQueue
{
    #region Injects
    protected readonly ILogger<RecurringBackgroundJobQueue> Logger;
    protected readonly DbContext DbContext;
    protected readonly BaseErrorHandler BaseErrorHandler;
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IStringLocalizer<RecurringBackgroundJobQueue> Localizer;
    #endregion

    #region Member
    protected Dictionary<string, DateTime> NextRuntimeForBackgroundJobs { get; set; } = [];
    protected bool CurrentlyRunning = false;
    #endregion

    #region Init

    public RecurringBackgroundJobQueue(ILogger<RecurringBackgroundJobQueue> logger, DbContext database, BaseErrorHandler baseErrorHandler, IServiceProvider serviceProvider, IStringLocalizer<RecurringBackgroundJobQueue> localizer)
    {
        Logger = logger;
        DbContext = database;
        BaseErrorHandler = baseErrorHandler;
        ServiceProvider = serviceProvider;
        Localizer = localizer;

        foreach (var job in ServiceProvider.GetServices<IRecurringBackgroundJob>())
        {
            var nextRuntime = GetNextRuntimeFromJobEntry(job.Name) ?? DateTime.Now;
            NextRuntimeForBackgroundJobs.Add(job.Name, nextRuntime);
        }
    }

    #endregion

    public async Task TriggerBackgroundJobsAsync() // Must be triggered every minute
    {
        if (CurrentlyRunning)
            return;

        CurrentlyRunning = true;

        try
        {
            foreach (var job in ServiceProvider.GetServices<IRecurringBackgroundJob>())
            {
                var startTime = DateTime.Now;

                try
                {
                    if (DateTime.Now < NextRuntimeForBackgroundJobs[job.Name])
                        continue;
                    NextRuntimeForBackgroundJobs[job.Name] = job.GetNextExecutionTime(DateTime.Now);

                    await job.ExecuteJobAsync();
                    await UpdateBackgroundJobEntryDataAsync(job.Name, job.Log, startTime, NextRuntimeForBackgroundJobs[job.Name]);
                    await OnAfterJobExecutedAsync(job);
                }
                catch (Exception e)
                {
                    var errorText = $"Unexpected error in the background job \"{job.Name}\": {BaseErrorHandler.PrepareExceptionErrorMessage(e)}";
                    Logger.LogError(e, errorText);
                    await UpdateBackgroundJobEntryDataAsync(job.Name, job.Log, startTime, NextRuntimeForBackgroundJobs[job.Name], errorText);
                    await OnAfterUnexpectedErrorOccuredAsync(job, e, errorText);
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            CurrentlyRunning = false;
        }
    }

    public async Task ExecuteBackgroundJobManuallyAsync(string backgroundJobName)
    {
        var backgroundJobs = ServiceProvider.GetServices<IRecurringBackgroundJob>();
        var backgroundJob = backgroundJobs.Where(entry => entry.Name == backgroundJobName).FirstOrDefault();
        if (String.IsNullOrEmpty(backgroundJobName) || backgroundJob == null)
            throw new Exception(Localizer[$"A background job with the name \"{backgroundJobName}\" does not exists"]);

        var startTime = DateTime.Now;
        await backgroundJob.ExecuteJobAsync();
        await UpdateBackgroundJobEntryDataAsync(backgroundJob.Name, backgroundJob.Log, startTime);
    }

    public async Task UpdateBackgroundJobEntryDataAsync(string name, string log, DateTime startTime, DateTime? nextRuntime = null, string? error = null)
    {
        var entry = await DbContext.Set<RecurringBackgroundJobEntry>().FindAsync(name);
        if (entry == null)
        {
            entry = new RecurringBackgroundJobEntry() { Name = name };
            await DbContext.AddAsync(entry);
        }

        entry.LastRuntime = startTime;
        if (nextRuntime != null)
            entry.NextRuntime = nextRuntime.Value;

        entry.Log = ShortenTextToAllowedSize(GetFormattedLogText(log, startTime) + entry.Log);
        if (!String.IsNullOrEmpty(error))
            entry.LastErrors = ShortenTextToAllowedSize(GetFormattedLogText(error, startTime) + entry.LastErrors);

        await DbContext.SaveChangesAsync();
    }

    public static async Task AddMissingBackgroundJobEntriesToTheDatabaseAsync(IServiceProvider serviceProvider)
    {
        await serviceProvider.GetRequiredService<RecurringBackgroundJobQueue>().AddMissingBackgroundJobEntriesToTheDatabaseAsync();
    }

    public async Task AddMissingBackgroundJobEntriesToTheDatabaseAsync()
    {
        var backgroundJobs = ServiceProvider.GetServices<IRecurringBackgroundJob>();
        foreach (var backgroundJob in backgroundJobs)
        {
            var entry = await DbContext.Set<RecurringBackgroundJobEntry>().FindAsync(backgroundJob.Name);
            if (entry != null)
                continue;

            entry = new RecurringBackgroundJobEntry() { Name = backgroundJob.Name };
            await DbContext.AddAsync(entry);
        }

        await DbContext.SaveChangesAsync();
    }

    protected DateTime? GetNextRuntimeFromJobEntry(string name)
    {
        var entry = DbContext.Set<RecurringBackgroundJobEntry>().Find(name);
        return entry?.NextRuntime;
    }

    protected string GetFormattedLogText(string? text, DateTime startTime)
    {
        var formattedText = Environment.NewLine + $"---------------------------CURRENT RUN: {startTime:dd.MM.yy HH:mm:dd} ---------------------------" + Environment.NewLine;
        formattedText += (text ?? String.Empty);

        return formattedText;
    }

    protected string ShortenTextToAllowedSize(string text)
    {
        if (text.Length < 65536)
            return text;

        return text[..65536];
    }

    #region MISC
    protected virtual Task OnAfterJobExecutedAsync(IRecurringBackgroundJob job)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnAfterUnexpectedErrorOccuredAsync(IRecurringBackgroundJob job, Exception exception, string errorText)
    {
        return Task.CompletedTask;
    }
    #endregion
}
