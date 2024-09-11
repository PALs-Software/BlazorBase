using BlazorBase.Backup.Controller;
using BlazorBase.Files.Models;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Data.SqlClient;
using System.IO.Compression;
using static BlazorBase.Backup.Controller.BlazorBaseBackupFileController;

namespace BlazorBase.Backup.Services;

public class BackupWebsiteService(IConfiguration configuration, DbContext dbContext, IStringLocalizer<BackupWebsiteService> localizer, IJSRuntime jsRuntime, IMessageHandler messageHandler)
{
    #region Injects
    protected readonly IConfiguration Configuration = configuration;
    protected readonly DbContext DbContext = dbContext;
    protected readonly IStringLocalizer<BackupWebsiteService> Localizer = localizer;
    protected readonly IJSRuntime JSRuntime = jsRuntime;
    protected readonly IMessageHandler MessageHandler = messageHandler;
    #endregion

    public virtual async Task CreateAndDownloadWebsiteBackupAsync()
    {
        var progressId = MessageHandler.ShowLoadingProgressMessage(
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Cleanup "],
            showProgressInText: true
        );

        var tempFileStorePath = BlazorBaseFileOptions.Instance.TempFileStorePath;
        if (!Directory.Exists(tempFileStorePath))
            Directory.CreateDirectory(tempFileStorePath);

        DeleteOldBrokenBackups();

        var databaseProvider = Configuration["DatabaseProvider"];
        var archivePath = Path.Join(tempFileStorePath, $"{Guid.NewGuid()}.WebsiteBackup.zip");
        using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            if (databaseProvider != null && databaseProvider == "SQLite")
                await CreateSQLiteDatabaseBackupAsync(archive, progressId);
            else
                await CreateMSSQLDatabaseBackupAsync(archive, progressId);

            CreateFileStoreBackup(archive, progressId);
        }

        await BlazorBaseBackupFileController.AddFileRequestAsync(JSRuntime, new FileRequest(archivePath, $"WebsiteBackup_{DateTime.Now:yyyy_MM_dd}.zip", System.Net.Mime.MediaTypeNames.Application.Zip, FileOptions.DeleteOnClose));

        MessageHandler.CloseLoadingProgressMessage(progressId);
    }

    protected virtual async Task CreateMSSQLDatabaseBackupAsync(ZipArchive zipArchive, ulong progressId)
    {
        MessageHandler.UpdateLoadingProgressMessage(
            id: progressId,
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Creating Database Backup "],
            currentProgress: 10
        );

        var backupPath = Path.Join(BlazorBaseFileOptions.Instance.TempFileStorePath, $"{Guid.NewGuid()}_DatabaseBackup.bak");

        try
        {
            var sqlConStrBuilder = new SqlConnectionStringBuilder(DbContext.Database.GetDbConnection().ConnectionString);

            using var connection = new SqlConnection(sqlConStrBuilder.ConnectionString);
            var query = $"BACKUP DATABASE {sqlConStrBuilder.InitialCatalog} TO DISK='{backupPath}'";

            using var command = new SqlCommand(query, connection);
            connection.Open();
            var result = command.ExecuteNonQuery();

            var i = 0;
            do
            {
                i += 1;
                await Task.Delay(2000);

            } while (!File.Exists(backupPath) && i < 10);

            await Task.Delay(2000);

            zipArchive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath), CompressionLevel.Optimal);
        }
        finally
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }
    }

    protected virtual Task CreateSQLiteDatabaseBackupAsync(ZipArchive zipArchive, ulong progressId)
    {
        MessageHandler.UpdateLoadingProgressMessage(
            id: progressId,
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Creating Database Backup "],
            currentProgress: 10
        );

        var backupPath = Path.Join(BlazorBaseFileOptions.Instance.TempFileStorePath, $"{Guid.NewGuid()}_DatabaseBackup.db");
        var backupShmPath = Path.Join(BlazorBaseFileOptions.Instance.TempFileStorePath, $"{Guid.NewGuid()}_DatabaseBackup.db-shm");
        var backupWalPath = Path.Join(BlazorBaseFileOptions.Instance.TempFileStorePath, $"{Guid.NewGuid()}_DatabaseBackup.db-wal");
        
        try
        {
            var connectionString = DbContext.Database.GetDbConnection().ConnectionString;
            var connectionStringParts = connectionString.Split(";");
            var dataSource = connectionStringParts.FirstOrDefault(x => x.Contains("Data Source"))?.Split("=")[1];
            if (dataSource == null || !File.Exists(dataSource))
                throw new Exception($"Can not find sqlite database file, specified in the connection strings data source: {dataSource}");

            File.Copy(dataSource, backupPath, true);
            zipArchive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath), CompressionLevel.Optimal);

            var dbShmFilePath = dataSource + "-shm";
            if (File.Exists(dbShmFilePath))
            {
                File.Copy(dbShmFilePath, backupShmPath, true);
                zipArchive.CreateEntryFromFile(backupShmPath, Path.GetFileName(backupShmPath), CompressionLevel.Optimal);
            }

            var dbWalFilePath = dataSource + "-wal";
            if (File.Exists(dbWalFilePath))
            {
                File.Copy(dbWalFilePath, backupWalPath, true);
                zipArchive.CreateEntryFromFile(backupWalPath, Path.GetFileName(backupWalPath), CompressionLevel.Optimal);
            }

            return Task.CompletedTask;
        }
        finally
        {
            if (File.Exists(backupPath)) File.Delete(backupPath);
            if (File.Exists(backupShmPath)) File.Delete(backupShmPath);
            if (File.Exists(backupWalPath)) File.Delete(backupWalPath);
        }
    }

    protected virtual void CreateFileStoreBackup(ZipArchive zipArchive, ulong progressId)
    {
        MessageHandler.UpdateLoadingProgressMessage(
            id: progressId,
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Creating File Backup "],
            currentProgress: 20
        );

        var files = Directory.GetFiles(BlazorBaseFileOptions.Instance.FileStorePath);
        for (int i = 0; i < files.Length; i++)
        {
            zipArchive.CreateEntryFromFile(files[i], Path.GetFileName(files[i]), CompressionLevel.Optimal);

            MessageHandler.UpdateLoadingProgressMessage(
                id: progressId,
                message: Localizer["Create Website Backup"],
                progressText: Localizer["Creating File Backup "],
                currentProgress: 21 + (int)((double)i / files.Length * 0.78 * 100)
            );
        }
    }

    protected virtual void DeleteOldBrokenBackups()
    {
        foreach (var filePath in Directory.GetFiles(BlazorBaseFileOptions.Instance.TempFileStorePath, "*.WebsiteBackup.zip"))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.CreationTime < DateTime.Now.AddDays(-1))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception) { }
            }
        }
    }
}

