namespace BlazorBase.Restore;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        var restoreService = new RestoreService();
        restoreService.StartWebsiteRestore();
    }
}
