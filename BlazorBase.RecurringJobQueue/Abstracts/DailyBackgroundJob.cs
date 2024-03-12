namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public abstract class DailyBackgroundJob : RecurringBackgroundJob, IDailyBackgroundJob
{
    public abstract TimeSpan ExecutionTime { get; }

    public override DateTime GetNextExecutionTime(DateTime lastRunTime)
    {
        return lastRunTime.AddDays(1).Date + ExecutionTime;
    }
}
