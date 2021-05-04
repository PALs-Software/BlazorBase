﻿using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using static BlazorBase.CRUD.Models.IBaseModel;
using System.Web;
using BlazorBase.Files.Controller;
using System.Net;
using Microsoft.EntityFrameworkCore;
using BlazorBase.CRUD.ViewModels;
using System.Security.Cryptography;

namespace BlazorBase.Files.Models
{
    [Route("/BaseFiles")]
    public class BaseFile : BaseModel
    {
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

        public override async Task OnAfterSaveChanges(OnAfterSaveChangesArgs args)
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

        public static async Task<TBaseFile> CreateFileAsync<TBaseFile>(EventServices eventServices, string fileName, string baseFileType, string mimeFileType, byte[] fileContent) where TBaseFile : BaseFile, new()
        {
            var file = new TBaseFile()
            {
                FileName = fileName,
                FileSize = fileContent.Length,
                BaseFileType = baseFileType,
                MimeFileType = mimeFileType,
                TempFileId = Guid.NewGuid(),
                Hash = ComputeSha256Hash(fileContent)
            };
            await file.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(file, eventServices));

            var options = BlazorBaseFileOptions.Instance;
            if (!Directory.Exists(options.TempFileStorePath))
                Directory.CreateDirectory(options.TempFileStorePath);

            await File.WriteAllBytesAsync(Path.Join(options.TempFileStorePath, file.TempFileId.ToString()), fileContent);

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
