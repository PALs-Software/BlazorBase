using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.Files.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.Files.Models;

public interface IBaseFile : IBaseModel
{
    #region Properties
    
    Guid Id { get; set; }
    string FileName { get; set; }
    string? Description { get; set; }
    string BaseFileType { get; set; }
    string MimeFileType { get; set; }
    long FileSize { get; set; }
    string? Hash { get; set; }
    Guid TempFileId { get; set; }

    #endregion

    #region File Handling

    string GetFileNameWithExtension();
    string GetTemporaryFileNameWithExtension();

    string GetPhysicalFileName();
    string GetPhysicalTemporaryFileName();

    string? GetFileLink(bool useThumbnailIfImage = false, bool ignoreTemporaryLink = false);

    string GetPhysicalFilePath();

    Task<byte[]> GetFileContentAsync();
    Task<string?> GetFileAsBase64StringAsync();

    Task RemoveFileFromDiskAsync(bool deleteOnlyTemporary = false);
    Task ClearFileFromPropertyAsync(IBaseModel model, string propertyName, IBaseDbContext dbContext);
    Task ClearFileFromPropertyAsync(IBaseModel model, PropertyInfo property, IBaseDbContext dbContext);

    void RecalculateHashAndSize(IBaseFileService baseFileService);
    #endregion

    #region File Type
    bool IsImage();
    bool IsAudio();
    bool IsVideo();
    bool IsDocument(); 
    bool IsOfficeFile();
    #endregion

    #region Image Handling

    string GetPhysicalThumbnailName();
    string GetPhysicalTemporaryThumbnailName();
    string GetPhysicalThumbnailPath();
    Task CreateThumbnailAsync(IImageService imageService, byte[] bytes);

    #endregion
}
