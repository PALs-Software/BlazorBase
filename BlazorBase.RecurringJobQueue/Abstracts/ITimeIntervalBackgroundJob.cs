namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public interface ITimeIntervalBackgroundJob : IRecurringBackgroundJob
{
    int TimerIntervalInMinutes { get; }
}
