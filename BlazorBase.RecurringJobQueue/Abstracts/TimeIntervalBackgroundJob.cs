namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public abstract class TimeIntervalBackgroundJob : RecurringBackgroundJob, ITimeIntervalBackgroundJob
{
    public abstract int TimerIntervalInMinutes { get; }

    public override DateTime GetNextExecutionTime(DateTime lastRunTime)
    {
        return lastRunTime.AddMinutes(TimerIntervalInMinutes);
    }
}