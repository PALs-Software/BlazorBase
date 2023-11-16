using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazorBase.Files.Services;

public class BaseFileService : IBaseFileService
{

    #region Injects
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IBlazorBaseFileOptions Options;
    protected readonly IImageService ImageService;
    #endregion

    public BaseFileService(IServiceProvider serviceProvider, IBlazorBaseFileOptions options, IImageService imageService)
    {
        ServiceProvider = serviceProvider;
        Options = options;
        ImageService = imageService;
    }

    public virtual async Task<IBaseFile> CreateFileAsync(EventServices eventServices, string fileName, string baseFileType, string mimeFileType, byte[] fileContent)
    {
        var fileType = ServiceProvider.GetRequiredService<IBaseFile>().GetType();
        var file = (IBaseFile?)Activator.CreateInstance(fileType);
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        file.FileName = fileName;
        file.FileSize = fileContent.Length;
        file.BaseFileType = baseFileType;
        file.MimeFileType = mimeFileType;
        file.Hash = ComputeSha256Hash(fileContent);
        await file.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(file, eventServices));

        return await FinishCreateFileTaskAsync(file, fileContent);
    }

    protected virtual async Task<IBaseFile> FinishCreateFileTaskAsync(IBaseFile file, byte[] fileContent)
    {
        if (!Directory.Exists(Options.TempFileStorePath))
            Directory.CreateDirectory(Options.TempFileStorePath);

        string tempFilePath;
        do
        {
            file.TempFileId = Guid.NewGuid();
            tempFilePath = Path.Join(Options.TempFileStorePath, file.GetPhysicalTemporaryFileName());
        } while (File.Exists(tempFilePath));

        await File.WriteAllBytesAsync(tempFilePath, fileContent);

        if (Options.UseImageThumbnails && file.IsImage())
            await file.CreateThumbnailAsync(ImageService, fileContent);

        return file;
    }

    public virtual async Task<IBaseFile> CreateCopyAsync(IBaseFile sourceFile, EventServices eventServices)
    {
        var fileContent = await sourceFile.GetFileContentAsync() ?? Array.Empty<byte>();
        return await CreateFileAsync(eventServices, sourceFile.FileName, sourceFile.BaseFileType, sourceFile.MimeFileType, fileContent);
    }

    public virtual string ComputeSha256Hash(Func<SHA256, byte[]> computeHash)
    {
        using SHA256 sha256Hash = SHA256.Create();
        byte[] Hashbytes = computeHash(sha256Hash);

        var hash = String.Empty;
        foreach (var hashByte in Hashbytes)
            hash += $"{hashByte:X2}";

        return hash;
    }

    public virtual string ComputeSha256Hash(FileStream fileStream)
    {
        return ComputeSha256Hash((hasher) => hasher.ComputeHash(fileStream));
    }

    public virtual string ComputeSha256Hash(byte[] buffer)
    {
        return ComputeSha256Hash((hasher) => hasher.ComputeHash(buffer));
    }
}
