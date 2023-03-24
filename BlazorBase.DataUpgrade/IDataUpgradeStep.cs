using System.Diagnostics;

namespace BlazorBase.DataUpgrade;

public interface IDataUpgradeStep
{
    string Id { get; }
    string Description { get; }
    string LogText { get; set; }
    Func<Task> DataUpgradeProcedure { get; }

    public void Log(string message, bool extraNewLine = false)
    {
        LogText += message + Environment.NewLine;
        if (extraNewLine)
            LogText += Environment.NewLine;

#if DEBUG
        Debug.WriteLine(message);
#endif
    }
}
