using BlazorBase.Backup.Controller;
using BlazorBase.CRUD.Services;
using BlazorBase.Files.Models;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Data.SqlClient;
using System.IO.Compression;
using static BlazorBase.Backup.Controller.BlazorBaseBackupFileController;

namespace BlazorBase.Backup.Services;

public class BackupWebsiteService
{
    #region Injects

    protected readonly BaseService BaseService;
    protected readonly IStringLocalizer<BackupWebsiteService> Localizer;
    protected readonly IJSRuntime JSRuntime;
    protected readonly IMessageHandler MessageHandler;

    #endregion

    public BackupWebsiteService(BaseService baseService, IStringLocalizer<BackupWebsiteService> localizer, IJSRuntime jsRuntime, IMessageHandler messageHandler)
    {
        BaseService = baseService;
        Localizer = localizer;
        JSRuntime = jsRuntime;
        MessageHandler = messageHandler;
    }

    public virtual async Task CreateAndDownloadWebsiteBackupAsync()
    {
        var progressId = MessageHandler.ShowLoadingProgressMessage(
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Cleanup"],
            showProgressInText: true
        );

        var tempFileStorePath = BlazorBaseFileOptions.Instance.TempFileStorePath;
        if (!Directory.Exists(tempFileStorePath))
            Directory.CreateDirectory(tempFileStorePath);

        DeleteOldBrokenBackups();

        var archivePath = Path.Join(tempFileStorePath, $"{Guid.NewGuid()}.WebsiteBackup.zip");
        using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            await CreateDatabaseBackupAsync(archive, progressId);
            CreateFileStoreBackup(archive, progressId);
        }

        await BlazorBaseBackupFileController.AddFileRequestAsync(JSRuntime, new FileRequest(archivePath, $"WebsiteBackup_{DateTime.Now:yyyy_MM_dd}.zip", System.Net.Mime.MediaTypeNames.Application.Zip, FileOptions.DeleteOnClose));

        MessageHandler.CloseLoadingProgressMessage(progressId);
    }

    protected virtual async Task CreateDatabaseBackupAsync(ZipArchive zipArchive, Guid progressId)
    {
        MessageHandler.UpdateLoadingProgressMessage(
            id: progressId,
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Creating Database Backup"],
            currentProgress: 10
        );

        var backupPath = Path.Join(BlazorBaseFileOptions.Instance.TempFileStorePath, $"{Guid.NewGuid()}_DatabaseBackup.bak");

        try
        {
            var sqlConStrBuilder = new SqlConnectionStringBuilder(BaseService.DbContext.Database.GetDbConnection().ConnectionString);

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

            File.Delete(backupPath);
        }
        catch (Exception)
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);

            throw;
        }
    }

    protected virtual void CreateFileStoreBackup(ZipArchive zipArchive, Guid progressId)
    {
        MessageHandler.UpdateLoadingProgressMessage(
            id: progressId,
            message: Localizer["Create Website Backup"],
            progressText: Localizer["Creating File Backup"],
            currentProgress: 20
        );

        var files = Directory.GetFiles(BlazorBaseFileOptions.Instance.FileStorePath);
        for (int i = 0; i < files.Length; i++)
        {
            zipArchive.CreateEntryFromFile(files[i], Path.GetFileName(files[i]), CompressionLevel.Optimal);

            MessageHandler.UpdateLoadingProgressMessage(
                id: progressId,
                message: Localizer["Create Website Backup"],
                progressText: Localizer["Creating File Backup"],
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

