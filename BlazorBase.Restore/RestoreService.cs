using Newtonsoft.Json;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace BlazorBase.Restore;

public class RestoreService
{
    public void StartWebsiteRestore()
    {
        Console.WriteLine("Restore website from backup");

        var projectPath = SelectProjectPath();
        if (projectPath == null)
            return;

        Console.WriteLine("Load app settings of the project");
        var appSettings = AppSettings.LoadAppSettings(projectPath);
        if (appSettings == null)
        {
            Console.WriteLine("The app settings of the project can not be read...");
            return;
        }

        Console.WriteLine();
        var backupFilePath = GetBackupFilePath();

        Console.WriteLine();
        RestoreFileStoreFromBackup(appSettings, backupFilePath);
        Console.WriteLine();
        RestoreDatabaseFromBackup(appSettings);

        Console.WriteLine();
        Console.WriteLine("Finished");
    }

    protected virtual string? SelectProjectPath()
    {
        Console.WriteLine("Select project:");

        var basePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\");
        var blazorBaseProjectPath = Path.GetFullPath(Path.Join(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\"));
        var projectFiles = Directory.GetFiles(basePath, "*.csproj", SearchOption.AllDirectories).Select(Path.GetFullPath).ToList();
        projectFiles = projectFiles.Where(entry => !entry.StartsWith(blazorBaseProjectPath)).ToList();
        var regex = new Regex("<ProjectReference Include=\".*?\\\\BlazorBase\\\\BlazorBase\\.Backup\\\\BlazorBase\\.Backup\\.csproj\" \\/>");
        var possibleProjects = new List<string>();

        foreach (var projectFile in projectFiles)
        {

            var content = File.ReadAllText(projectFile);
            if (regex.IsMatch(content))
                possibleProjects.Add(projectFile);
        }

        if (possibleProjects.Count == 0)
        {
            Console.WriteLine("No project found which uses BlazorBase.Backup...");
            return null;
        }

        if (possibleProjects.Count == 1)
        {
            Console.WriteLine($"Only one possible project available: Select project \"{Path.GetFileName(possibleProjects.First())}\"");
            return possibleProjects.First();
        }

        var userInputIsValid = false;
        int selectedProject = 0;
        do
        {
            for (int i = 0; i < possibleProjects.Count; i++)
            {
                Console.WriteLine($"Press the number of the project you want to restore");
                Console.WriteLine();
                Console.WriteLine($"{i + 1}: \"{Path.GetFileName(possibleProjects[i])}\"");
            }

            var userInput = Console.ReadLine();
            userInputIsValid = int.TryParse(userInput, out int result);
            if (userInputIsValid)
            {
                if (result > possibleProjects.Count || result < 1)
                    userInputIsValid = false;
                else
                    selectedProject = result - 1;
            }
        } while (!userInputIsValid);

        Console.WriteLine($"Project \"{Path.GetFileName(possibleProjects[selectedProject])}\" selected");

        return possibleProjects[selectedProject];
    }

    protected virtual string GetBackupFilePath()
    {
        Console.WriteLine("Select backup file...");
        var openFileDialog = new OpenFileDialog
        {
            Filter = "zip files (*.zip)|*.zip",
            RestoreDirectory = true
        };
        openFileDialog.ShowDialog();

        return openFileDialog.FileName;
    }
    protected virtual void RestoreFileStoreFromBackup(AppSettings appSettings, string backupPath)
    {
        Console.WriteLine($"Restore file store from backup {backupPath}");

        var fileStorePath = appSettings.FileStorePath;
        if (Directory.Exists(fileStorePath))
        {
            Console.WriteLine($"Clear current file store");
            Directory.Delete(fileStorePath, true);
        }

        Directory.CreateDirectory(fileStorePath);

        Console.WriteLine($"Extract backup to file store");
        ZipFile.ExtractToDirectory(backupPath, fileStorePath);
    }

    protected virtual string GetBakFilePath(string path)
    {
        var files = Directory.GetFiles(path, "*.bak");
        return files[0];
    }

    protected virtual void RestoreDatabaseFromBackup(AppSettings appSettings)
    {
        var bakPath = GetBakFilePath(appSettings.FileStorePath);
        var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(appSettings.ConnectionStrings.DefaultConnection);
        var dbName = sqlConnectionStringBuilder.InitialCatalog;
        sqlConnectionStringBuilder.InitialCatalog = "master";

        Console.WriteLine($"Restore database {dbName} from backup {bakPath}");

        using var connection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
        var query = $"RESTORE DATABASE {dbName} FROM DISK='{bakPath}' WITH REPLACE";

        using var command = new SqlCommand(query, connection);
        command.CommandTimeout = 0;
        connection.Open();
        var result = command.ExecuteNonQuery();
        Console.WriteLine($"Restore database result: {result}");

        if (result == -1)
            Console.WriteLine($"Result was -1: Database successfully restored");
        else
            Console.WriteLine($"The result was not -1: Something went wrong, please check the database recovery manuell");

        Console.WriteLine($"Delete bak file from temporary file store");
        File.Delete(bakPath);
    }

}
