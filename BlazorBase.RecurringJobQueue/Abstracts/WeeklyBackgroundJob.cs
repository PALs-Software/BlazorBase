namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public abstract class WeeklyBackgroundJob : RecurringBackgroundJob, IDailyBackgroundJob
{
    public virtual bool RunOnMondays { get; }
    public virtual bool RunOnTuesdays { get; }
    public virtual bool RunOnWednesdays { get; }
    public virtual bool RunOnThursdays { get; }
    public virtual bool RunOnFridays { get; }
    public virtual bool RunOnSaturdays { get; }
    public virtual bool RunOnSundays { get; }

    public abstract TimeSpan ExecutionTime { get; }

    public override DateTime GetNextExecutionTime(DateTime lastRunTime) {
        if (!RunOnMondays && !RunOnTuesdays && !RunOnWednesdays && !RunOnThursdays && !RunOnFridays && !RunOnSaturdays && !RunOnSundays)
            throw new Exception("Error by calculating next execution time of this background job: At least one execution day must be switched on.");

        int skipNoOfDays = 1;
        for (int i = 1; i <= 7; i++)
        {
            skipNoOfDays = i;
            var dayOfWeek = lastRunTime.AddDays(i).DayOfWeek;
            if (dayOfWeek == DayOfWeek.Monday && RunOnMondays)
                break;
            if (dayOfWeek == DayOfWeek.Tuesday && RunOnTuesdays)
                break;
            if (dayOfWeek == DayOfWeek.Wednesday && RunOnWednesdays)
                break;
            if (dayOfWeek == DayOfWeek.Thursday && RunOnThursdays)
                break;
            if (dayOfWeek == DayOfWeek.Friday && RunOnFridays)
                break;
            if (dayOfWeek == DayOfWeek.Saturday && RunOnSaturdays)
                break;
            if (dayOfWeek == DayOfWeek.Sunday && RunOnSundays)
                break;
        }

        return lastRunTime.AddDays(skipNoOfDays).Date + ExecutionTime;
    }
}
