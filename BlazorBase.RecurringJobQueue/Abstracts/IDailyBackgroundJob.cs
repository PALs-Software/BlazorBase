namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public interface IDailyBackgroundJob : IRecurringBackgroundJob
{
    TimeSpan ExecutionTime { get; }
}
