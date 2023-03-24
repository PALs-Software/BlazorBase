using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BlazorBase.Files.Controller;
using Microsoft.EntityFrameworkCore;
using BlazorBase.CRUD.ViewModels;
using System.Security.Cryptography;
using BlazorBase.CRUD.SortableItem;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Services;
using System.Reflection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Web;
using Microsoft.Extensions.Options;

namespace BlazorBase.Files.Models
{
    [Route("/BaseFiles")]
    public class BaseFile : BaseModel, ISortableItem
    {
        public int SortIndex { get; set; }

        [Key]
        public Guid Id { get; set; }

        [Required]
        [DisplayKey]
        [Visible(DisplayOrder = 200)]
        public string FileName { get; set; }

        [Visible(DisplayOrder = 300)]
        public string Description { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 400)]
        public string BaseFileType { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 500)]
        public string MimeFileType { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 600)]
        public long FileSize { get; set; }

        [Editable(false)]
        [Visible(DisplayOrder = 700)]
        public string Hash { get; set; }

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
            Id = await args.EventServices.BaseService.GetNewPrimaryKeyAsync(GetType());

            args.EventServices.BaseService.DbContext.Entry(this).State = EntityState.Added; //Needed for some Reason, because ef not detect that when a basefile is a navigation property and is newly added, it must add the base file before it add or update the entity itself
        }

        public override Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args)
        {
            var entry = args.EventServices.BaseService.DbContext.Entry(this);
            if (entry.State != EntityState.Detached) // Only relevant if removed from another entity as list property
                entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;

            return base.OnAfterRemoveEntry(args);
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

        public string GetFileLink(bool ignoreTemporaryLink = false)
        {
            if (String.IsNullOrEmpty(BaseFileType))
                return null;

            if (TempFileId == Guid.Empty || ignoreTemporaryLink)
                return $"/{BlazorBaseFileOptions.Instance.ControllerRoute}/GetFile/{Id}/{Uri.EscapeDataString(FileName)}?hash={Hash}"; //Append Hash for basic browser file cache refresh notification
            else
                return $"/{BlazorBaseFileOptions.Instance.ControllerRoute}/GetTemporaryFile/{TempFileId}/{Uri.EscapeDataString(FileName)}?hash={Hash}";
        }

        public string GetPhysicalFilePath()
        {
            var options = BlazorBaseFileOptions.Instance;
            if (TempFileId == Guid.Empty)
                return Path.Join(options.FileStorePath, GetPhysicalFileName());
            else
                return Path.Join(options.TempFileStorePath, GetPhysicalTemporaryFileName());
        }

        public Task<byte[]> GetFileContentAsync()
        {
            if (String.IsNullOrEmpty(BaseFileType) || FileSize == 0 || String.IsNullOrEmpty(Hash))
                return null;

            string path = GetPhysicalFilePath();
            if (!File.Exists(path))
                return null;

            return File.ReadAllBytesAsync(path);
        }

        public async Task<string> GetFileAsBase64StringAsync()
        {
            var content = await GetFileContentAsync();
            if (content == null)
                return null;

            return Convert.ToBase64String(content);
        }

        public async Task<T> CreateCopyAsync<T>(EventServices eventServices) where T : BaseFile, new()
        {
            var fileContent = await GetFileContentAsync();

            if (fileContent == null)
            {
                var localizer = eventServices.ServiceProvider.GetService<IStringLocalizer<T>>();
                throw new Exception(localizer["The file \"{0}\" can not be copied, because file with the id \"{1}\" can not be found on the hard disk. Maybe the file was deleted on the disk, but not the file entry.", FileName, Id]);
            }

            return await CreateFileAsync<T>(eventServices, FileName, BaseFileType, MimeFileType, fileContent);
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
                    var oldFilesWithOtherMimeTypes = Directory.EnumerateFiles(options.FileStorePath, $"{Id}_*");
                    foreach (var oldFilePath in oldFilesWithOtherMimeTypes)
                        File.Delete(oldFilePath);

                    File.Move(tempFilePath, Path.Join(options.FileStorePath, GetPhysicalFileName()), true);
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
            });
        }

        public async Task ClearFileFromPropertyAsync(IBaseModel model, string propertyName, BaseService service)
        {
            var property = model.GetType().GetProperty(propertyName);
            await ClearFileFromPropertyAsync(model, property, service);
        }

        public async Task ClearFileFromPropertyAsync(IBaseModel model, PropertyInfo property, BaseService service)
        {
            if (!property.CanWrite || property.GetValue(model) != this)
                return;

            await RemoveFileFromDiskAsync(deleteOnlyTemporary: true);
            service.DbContext.Entry(this).State = service.DbContext.Entry(this).State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;

            property.SetValue(model, null);
        }

        public static async Task<TBaseFile> CreateFileAsync<TBaseFile>(EventServices eventServices, string fileName, string baseFileType, string mimeFileType, byte[] fileContent) where TBaseFile : BaseFile, new()
        {
            var file = new TBaseFile()
            {
                FileName = fileName,
                FileSize = fileContent.Length,
                BaseFileType = baseFileType,
                MimeFileType = mimeFileType,
                Hash = ComputeSha256Hash(fileContent)
            };

            return (TBaseFile)await FinishCreateFileTaskAsync(file, eventServices, fileContent);
        }
        public static async Task<BaseFile> CreateFileAsync(Type FileType, EventServices eventServices, string fileName, string baseFileType, string mimeFileType, byte[] fileContent)
        {
            var file = (BaseFile)Activator.CreateInstance(FileType);
            file.FileName = fileName;
            file.FileSize = fileContent.Length;
            file.BaseFileType = baseFileType;
            file.MimeFileType = mimeFileType;
            file.Hash = ComputeSha256Hash(fileContent);

            return await FinishCreateFileTaskAsync(file, eventServices, fileContent);
        }

        protected static async Task<BaseFile> FinishCreateFileTaskAsync(BaseFile file, EventServices eventServices, byte[] fileContent)
        {
            await file.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(file, eventServices));

            var options = BlazorBaseFileOptions.Instance;
            if (!Directory.Exists(options.TempFileStorePath))
                Directory.CreateDirectory(options.TempFileStorePath);

            string tempFilePath;
            do
            {
                file.TempFileId = Guid.NewGuid();
                tempFilePath = Path.Join(options.TempFileStorePath, file.GetPhysicalTemporaryFileName());
            } while (File.Exists(tempFilePath));

            await File.WriteAllBytesAsync(tempFilePath, fileContent);

            return file;
        }

        public static string ComputeSha256Hash(Func<SHA256, byte[]> computeHash)
        {
            using SHA256 sha256Hash = SHA256.Create();
            byte[] Hashbytes = computeHash(sha256Hash);

            var hash = String.Empty;
            foreach (var hashByte in Hashbytes)
                hash += $"{hashByte:X2}";

            return hash;
        }

        public static string ComputeSha256Hash(FileStream fileStream)
        {
            return ComputeSha256Hash((hasher) => hasher.ComputeHash(fileStream));
        }

        public static string ComputeSha256Hash(byte[] buffer)
        {
            return ComputeSha256Hash((hasher) => hasher.ComputeHash(buffer));
        }

        public void RecalculateHashAndSize()
        {
            var path = GetPhysicalFilePath();
            if (!File.Exists(path))
            {
                Hash = null;
                FileSize = 0;
                return;
            }

            var bytes = File.ReadAllBytes(path);
            Hash = ComputeSha256Hash(bytes);
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
    }




}
