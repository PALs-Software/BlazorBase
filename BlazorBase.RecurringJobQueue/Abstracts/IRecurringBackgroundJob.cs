namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public interface IRecurringBackgroundJob
{
    string Name { get; }
    string Description { get; }
    string Log { get; }

    Task ExecuteJobAsync();
    DateTime GetNextExecutionTime(DateTime lastRunTime);
}
