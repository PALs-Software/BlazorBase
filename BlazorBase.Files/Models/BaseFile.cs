using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BlazorBase.CRUD.SortableItem;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Services;
using System.Reflection;
using BlazorBase.Files.Services;

namespace BlazorBase.Files.Models;

[Route("/BaseFiles")]
public class BaseFile : BaseModel, IBaseFile, ISortableItem
{
    public int SortIndex { get; set; }

    [Key]
    public Guid Id { get; set; }

    [Required]
    [DisplayKey]
    [Visible(DisplayOrder = 200)]
    public string FileName { get; set; } = null!;

    [Visible(DisplayOrder = 300)]
    public string? Description { get; set; }

    [Required]
    [Editable(false)]
    [Visible(DisplayOrder = 400)]
    public string BaseFileType { get; set; } = null!;

    [Required]
    [Editable(false)]
    [Visible(DisplayOrder = 500)]
    public string MimeFileType { get; set; } = null!;

    [Editable(false)]
    [Visible(DisplayOrder = 600)]
    public long FileSize { get; set; }

    [Editable(false)]
    [Visible(DisplayOrder = 700)]
    public string? Hash { get; set; }

    /// <summary>
    /// This property is only needed to show the file in the general base file list and card.
    /// </summary>
    [NotMapped]
    [Editable(false)]
    [Visible(DisplayOrder = 100)]
    public virtual BaseFile DisplayFile { get { return this; } }

    [NotMapped]
    public Guid TempFileId { get; set; }

    #region CRUD

    public override async Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args)
    {
        Id = await args.EventServices.DbContext.GetNewPrimaryKeyTSAsync(GetType());

        (await args.EventServices.DbContext.EntryAsync(this)).State = EntityState.Added; //Needed for some Reason, because ef not detect that when a basefile is a navigation property and is newly added, it must add the base file before it add or update the entity itself
    }

    public override async Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args)
    {
        var entry = await args.EventServices.DbContext.EntryAsync(this);
        if (entry.State != EntityState.Detached) // Only relevant if removed from another entity as list property
            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;

        await base.OnAfterRemoveEntry(args);
    }

    public override async Task OnAfterDbContextAddedEntry(OnAfterDbContextAddedEntryArgs args)
    {
        await CopyTempFileToFileStoreAsync();
    }

    public override async Task OnAfterDbContextModifiedEntry(OnAfterDbContextModifiedEntryArgs args)
    {
        await CopyTempFileToFileStoreAsync();
    }

    public override async Task OnAfterDbContextDeletedEntry(OnAfterDbContextDeletedEntryArgs args)
    {
        await RemoveFileFromDiskAsync();
    }
    #endregion

    #region File Handling

    public string GetFileNameWithExtension() => $"{Id}{BaseFileType}";
    public string GetTemporaryFileNameWithExtension() => $"{TempFileId}{BaseFileType}";

    public string GetPhysicalFileName() => $"{Id}_{MimeFileType.Replace("/", "'").Replace(".", "^")}";
    public string GetPhysicalTemporaryFileName() => $"{TempFileId}_{MimeFileType.Replace("/", "'").Replace(".", "^")}";

    public virtual string? GetFileLink(bool useThumbnailIfImage = false, bool ignoreTemporaryLink = false)
    {
        if (String.IsNullOrEmpty(BaseFileType))
            return null;

        string additionalParameters = String.Empty;
        if (BlazorBaseFileOptions.Instance.UseImageThumbnails && useThumbnailIfImage && IsImage())
            additionalParameters = $"thumbnail=true&";

        if (TempFileId == Guid.Empty || ignoreTemporaryLink)
            return $"/{BlazorBaseFileOptions.Instance.ControllerRoute}/GetFile/{Id}/{Uri.EscapeDataString(FileName)}?{additionalParameters}hash={Hash}"; //Append Hash for basic browser file cache refresh notification
        else
            return $"/{BlazorBaseFileOptions.Instance.ControllerRoute}/GetTemporaryFile/{TempFileId}/{Uri.EscapeDataString(FileName)}?{additionalParameters}hash={Hash}";
    }

    public string GetPhysicalFilePath()
    {
        var options = BlazorBaseFileOptions.Instance;
        if (TempFileId == Guid.Empty)
            return Path.Join(options.FileStorePath, GetPhysicalFileName());
        else
            return Path.Join(options.TempFileStorePath, GetPhysicalTemporaryFileName());
    }

    public virtual Task<byte[]> GetFileContentAsync()
    {
        if (String.IsNullOrEmpty(BaseFileType) || FileSize == 0 || String.IsNullOrEmpty(Hash))
            return Task.FromResult(Array.Empty<byte>());

        string path = GetPhysicalFilePath();
        if (!File.Exists(path))
            return Task.FromResult(Array.Empty<byte>());

        return File.ReadAllBytesAsync(path);
    }

    public async Task<string?> GetFileAsBase64StringAsync()
    {
        var content = await GetFileContentAsync();
        if (content == null)
            return null;

        return Convert.ToBase64String(content);
    }

    protected async Task CopyTempFileToFileStoreAsync()
    {
        await Task.Run(() =>
        {
            if (FileSize == 0 || TempFileId == Guid.Empty)
                return;

            var options = BlazorBaseFileOptions.Instance;
            if (!Directory.Exists(options.FileStorePath))
                Directory.CreateDirectory(options.FileStorePath);

            var tempFilePath = Path.Join(options.TempFileStorePath, GetPhysicalTemporaryFileName());
            if (File.Exists(tempFilePath))
            {
                var oldFilesWithOtherMimeTypes = Directory.EnumerateFiles(options.FileStorePath, $"{Id}*");
                foreach (var oldFilePath in oldFilesWithOtherMimeTypes)
                    File.Delete(oldFilePath);

                File.Move(tempFilePath, Path.Join(options.FileStorePath, GetPhysicalFileName()), true);
            }

            if (options.UseImageThumbnails && IsImage())
            {
                var tempThumbnailFilePath = Path.Join(options.TempFileStorePath, GetPhysicalTemporaryThumbnailName());
                if (File.Exists(tempThumbnailFilePath))
                    File.Move(tempThumbnailFilePath, Path.Join(options.FileStorePath, GetPhysicalThumbnailName()), true);
            }

            TempFileId = Guid.Empty;
        });
    }

    public async Task RemoveFileFromDiskAsync(bool deleteOnlyTemporary = false)
    {
        await Task.Run(() =>
        {
            if (deleteOnlyTemporary && TempFileId == Guid.Empty)
                return;

            var options = BlazorBaseFileOptions.Instance;
            var filePath = deleteOnlyTemporary ? Path.Join(options.TempFileStorePath, GetPhysicalTemporaryFileName()) : Path.Join(options.FileStorePath, GetPhysicalFileName());

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (options.UseImageThumbnails && IsImage())
            {
                var thumbnailPath = deleteOnlyTemporary ? Path.Join(options.TempFileStorePath, GetPhysicalTemporaryThumbnailName()) : Path.Join(options.FileStorePath, GetPhysicalThumbnailName());
                if (File.Exists(thumbnailPath))
                    File.Delete(thumbnailPath);
            }
        });
    }

    public async Task ClearFileFromPropertyAsync(IBaseModel model, string propertyName, IBaseDbContext dbContext)
    {
        var property = model.GetType().GetProperty(propertyName);
        ArgumentNullException.ThrowIfNull(property);
        await ClearFileFromPropertyAsync(model, property, dbContext);
    }

    public async Task ClearFileFromPropertyAsync(IBaseModel model, PropertyInfo property, IBaseDbContext dbContext)
    {
        if (!property.CanWrite || property.GetValue(model) != this)
            return;

        await RemoveFileFromDiskAsync(deleteOnlyTemporary: true);
        var entityEntry = await dbContext.EntryAsync(this);
        entityEntry.State = entityEntry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;

        property.SetValue(model, null);
    }

    public void RecalculateHashAndSize(IBaseFileService baseFileService)
    {
        var path = GetPhysicalFilePath();
        if (!File.Exists(path))
        {
            Hash = null;
            FileSize = 0;
            return;
        }

        var bytes = File.ReadAllBytes(path);
        Hash = baseFileService.ComputeSha256Hash(bytes);
        FileSize = bytes.Length;
    }
    #endregion

    #region File Type   

    public bool IsImage()
    {
        return MimeFileType?.StartsWith("image") ?? false;
    }

    public bool IsAudio()
    {
        return MimeFileType?.StartsWith("audio") ?? false;
    }

    public bool IsVideo()
    {
        return MimeFileType?.StartsWith("video") ?? false;
    }

    public static readonly String[] ValidDocumentMimeTypes = new String[] {
        "text/plain",
        "text/xml",
        "application/xml",
        "application/json",
        "application/pdf"
    };

    public bool IsDocument()
    {
        return ValidDocumentMimeTypes.Contains(MimeFileType);
    }

    public static readonly String[] ValidOfficeFileMimeTypes = new String[] {
        "application/msword",
        "application/msexcel",
        "application/mspowerpoint",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    };

    public bool IsOfficeFile()
    {
        return ValidOfficeFileMimeTypes.Contains(MimeFileType);
    }
    #endregion

    #region Image Handling

    public string GetPhysicalThumbnailName() => $"{Id}-Thumbnail_{MimeFileType.Replace("/", "'").Replace(".", "^")}";
    public string GetPhysicalTemporaryThumbnailName() => $"{TempFileId}-Thumbnail_{MimeFileType.Replace("/", "'").Replace(".", "^")}";

    public string GetPhysicalThumbnailPath()
    {
        var options = BlazorBaseFileOptions.Instance;
        if (TempFileId == Guid.Empty)
            return Path.Join(options.FileStorePath, GetPhysicalThumbnailName());
        else
            return Path.Join(options.TempFileStorePath, GetPhysicalTemporaryThumbnailName());
    }

    public Task CreateThumbnailAsync(IImageService imageService, byte[] bytes)
    {
        if (!IsImage())
            throw new NotSupportedException("This method can only be used if the file is an image.");

        return imageService.CreateThumbnailAsync(bytes, BlazorBaseFileOptions.Instance.ImageThumbnailSize, GetPhysicalThumbnailPath());
    }

    public virtual Task<byte[]> GetThumbnailAsync()
    {
        if (String.IsNullOrEmpty(BaseFileType) || FileSize == 0 || String.IsNullOrEmpty(Hash) || !IsImage())
            return Task.FromResult(Array.Empty<byte>());

        string path = GetPhysicalThumbnailPath();
        if (!File.Exists(path))
            return Task.FromResult(Array.Empty<byte>());

        return File.ReadAllBytesAsync(path);
    }

    public async Task<string?> GetThumbnailAsBase64StringAsync()
    {
        var content = await GetThumbnailAsync();
        if (content == null)
            return null;

        return Convert.ToBase64String(content);
    }
    #endregion
}
