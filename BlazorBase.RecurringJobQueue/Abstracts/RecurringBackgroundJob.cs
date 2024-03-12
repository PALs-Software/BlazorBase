namespace BlazorBase.RecurringBackgroundJobQueue.Abstracts;

public abstract class RecurringBackgroundJob : IRecurringBackgroundJob
{
    public RecurringBackgroundJob()
    {
        Name = GetType().Name;
    }

    public string Name { get; protected set; }
    public abstract string Description { get; }
    public string Log { get; protected set; } = String.Empty;
    
    public abstract Task ExecuteJobAsync();

    public abstract DateTime GetNextExecutionTime(DateTime lastRunTime);

    public void WriteLog(string message)
    {
        Log += $"[{DateTime.Now:dd.MM.yy HH:mm:dd}] " + message + Environment.NewLine;
    }
}
