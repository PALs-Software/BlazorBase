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
        public BaseFile DisplayFile { get { return this; } }

        [NotMapped]
        public Guid TempFileId { get; set; }

        #region CRUD

        public override async Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args)
        {
            do
            {
                Id = Guid.NewGuid();
            } while (await args.EventServices.BaseService.GetAsync(GetType(), Id) != null);

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
        public string GetFileLink()
        {
            if (String.IsNullOrEmpty(MimeFileType))
                return null;

            if (TempFileId == Guid.Empty)
                return $"/api/BaseFile/GetFile/{BaseFileController.EncodeUrl(MimeFileType)}/{Id}?hash={Hash}"; //Append Hash for basic browser file cache refresh notification
            else
                return $"/api/BaseFile/GetTemporaryFile/{BaseFileController.EncodeUrl(MimeFileType)}/{TempFileId}?hash={Hash}";
        }

        public string GetPhysicalFilePath()
        {
            var options = BlazorBaseFileOptions.Instance;
            if (TempFileId == Guid.Empty)
                return Path.Join(options.FileStorePath, Id.ToString());
            else
                return Path.Join(options.TempFileStorePath, TempFileId.ToString());
        }

        public async Task<byte[]> GetFileContentAsync()
        {
            if (String.IsNullOrEmpty(MimeFileType) || FileSize == 0 || String.IsNullOrEmpty(Hash))
                return null;

            var options = BlazorBaseFileOptions.Instance;
            string path;
            if (TempFileId == Guid.Empty)
                path = Path.Join(options.FileStorePath, Id.ToString());
            else
                path = Path.Join(options.TempFileStorePath, TempFileId.ToString());

            if (!File.Exists(path))
                return null;

            return await File.ReadAllBytesAsync(path);
        }

        public async Task<string> GetFileAsBase64StringAsync()
        {
            var content = await GetFileContentAsync();
            if (content == null)
                return null;

            return Convert.ToBase64String(content);
        }

        public async Task<T> CreateCopyAsync<T>(EventServices service) where T : BaseFile, new()
        {
            return await CreateFileAsync<T>(service, FileName, BaseFileType, MimeFileType, await GetFileContentAsync());
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

                var tempFilePath = Path.Join(options.TempFileStorePath, TempFileId.ToString());
                if (File.Exists(tempFilePath))
                    File.Move(tempFilePath, Path.Join(options.FileStorePath, Id.ToString()), true);

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
                var filePath = deleteOnlyTemporary ? Path.Join(options.TempFileStorePath, TempFileId.ToString()) : Path.Join(options.FileStorePath, Id.ToString());

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
            await file.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(file, eventServices));

            var options = BlazorBaseFileOptions.Instance;
            if (!Directory.Exists(options.TempFileStorePath))
                Directory.CreateDirectory(options.TempFileStorePath);

            string tempFilePath;
            do
            {
                file.TempFileId = Guid.NewGuid();
                tempFilePath = Path.Join(options.TempFileStorePath, file.TempFileId.ToString());
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
