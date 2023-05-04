using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Attributes;
using BlazorBase.Files.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.Files.Components
{
    public partial class BaseFileInput : BaseInput, IBasePropertyCardInput, IBasePropertyListPartInput
    {
        #region Parameters
        [Parameter] public ulong? MaxFileSize { get; set; } = null;
        [Parameter] public string? FileFilter { get; set; } = null;
        #endregion

        #region Inject
        [Inject] protected IStringLocalizer<BaseFileInput> Localizer { get; set; } = null!;
        [Inject] protected IBlazorBaseFileOptions Options { get; set; } = null!;
        #endregion

        #region Member
        protected bool ShowLoadingIndicator = false;
        protected int UploadProgress = 0;
        protected FileEdit? FileEdit;
        protected BaseFileModal? BaseFileModal;
        protected bool FileEditIsResetting = false;
        protected EventServices EventServices = null!;

        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (MaxFileSize == null && Property.GetCustomAttribute(typeof(MaxFileSizeAttribute)) is MaxFileSizeAttribute maxFileSizeAttribute)
                MaxFileSize = maxFileSizeAttribute.MaxFileSize;

            if (FileFilter == null && Property.GetCustomAttribute(typeof(FileInputFilterAttribute)) is FileInputFilterAttribute filterAttribute)
                FileFilter = filterAttribute?.Filter ?? "*.";

            EventServices = new EventServices(ServiceProvider, ModelLocalizer, Service, MessageHandler);            
        }

        public Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
        {
            return Task.FromResult(typeof(BaseFile).IsAssignableFrom(displayItem.Property.PropertyType));
        }

        public Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs args) { return Task.CompletedTask; }

        public Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args) { return Task.CompletedTask; }

        public Task<bool> InputHasAdditionalContentChanges()
        {
            return Task.FromResult(false);
        }

        protected override async Task OnValueChangedAsync(object? fileChangedEventArgs, bool setCurrentValueAsString = true)
        {
            if (FileEditIsResetting || fileChangedEventArgs == null || FileEdit == null)
                return;

            var eventServices = GetEventServices();
            var oldValue = Property.GetValue(Model);
            bool valid = true;
            BaseFile? newFile = null;
            try
            {
                var files = ((FileChangedEventArgs)fileChangedEventArgs).Files;
                if (files.Length == 0)
                    return;

                var args = new OnBeforePropertyChangedArgs(Model, Property.Name, fileChangedEventArgs, oldValue, eventServices);
                await OnBeforePropertyChanged.InvokeAsync(args);
                await Model.OnBeforePropertyChanged(args);
                fileChangedEventArgs = args.NewValue;

                ShowLoadingIndicator = true;
                var file = files.First();
                if (MaxFileSize != null && MaxFileSize != 0 && (ulong)file.Size > MaxFileSize)
                    throw new IOException(Localizer["The file exceed the maximum allowed file size of {0} bytes", MaxFileSize]);

                newFile = (BaseFile)Activator.CreateInstance(RenderType)!;
                newFile.FileName = Path.GetFileNameWithoutExtension(file.Name);
                newFile.FileSize = file.Size;
                newFile.BaseFileType = Path.GetExtension(file.Name);
                newFile.MimeFileType = GetMimeTypeOfFile(file);

                if (Model is BaseFile baseFile)
                {
                    if (baseFile.TempFileId == Guid.Empty)
                    {
                        var tempFileStorePath = Options.TempFileStorePath;
                        string tempFilePath;
                        do
                        {
                            newFile.TempFileId = Guid.NewGuid();
                            tempFilePath = Path.Join(tempFileStorePath, newFile.GetPhysicalTemporaryFileName());
                        } while (File.Exists(tempFilePath));
                    }
                    else
                        newFile.TempFileId = baseFile.TempFileId;

                    baseFile.Hash = await WriteFileStreamToTempFileStore(file, newFile);
                    baseFile.TempFileId = newFile.TempFileId;
                    baseFile.FileName = newFile.FileName;
                    baseFile.FileSize = newFile.FileSize;
                    baseFile.BaseFileType = newFile.BaseFileType;
                    baseFile.MimeFileType = newFile.MimeFileType;

                    Model.ForcePropertyRepaint(nameof(BaseFile.FileName));
                }
                else
                {
                    if (oldValue is BaseFile oldFile)
                    {
                        await oldFile.RemoveFileFromDiskAsync(deleteOnlyTemporary: true);
                        var entry = Service.DbContext.Entry(oldFile);
                        entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
                    }

                    await newFile.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(Model, eventServices));
                    newFile.TempFileId = Guid.NewGuid();
                    await newFile.OnBeforeAddEntry(new OnBeforeAddEntryArgs(newFile, false, eventServices));

                    newFile.Hash = await WriteFileStreamToTempFileStore(file, newFile);
                    Property.SetValue(Model, newFile);
                    await newFile.OnAfterAddEntry(new OnAfterAddEntryArgs(newFile, eventServices));
                }

                SetValidation(showValidation: false);
            }
            catch (Exception e)
            {
                valid = false;
                SetValidation(feedback: ErrorHandler.PrepareExceptionErrorMessage(e));
            }
            finally
            {
                ShowLoadingIndicator = false;
                FileEditIsResetting = true;
                await FileEdit.Reset();
                FileEditIsResetting = false;
            }

            try
            {
                LastValueConversionFailed = !valid;
                if (valid)
                    await ReloadForeignProperties(fileChangedEventArgs);

                var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, newFile, oldValue, valid, eventServices);
                await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
                await Model.OnAfterPropertyChanged(onAfterArgs);
            }
            catch (Exception e)
            {
                LastValueConversionFailed = true;
                SetValidation(feedback: ErrorHandler.PrepareExceptionErrorMessage(e));
            }
        }

        protected async Task<string?> WriteFileStreamToTempFileStore(IFileEntry file, BaseFile newFile)
        {
            if (file.Size == 0)
                return null;

            if (newFile.TempFileId == Guid.Empty)
                throw new CRUDException(Localizer["The Id can not be empty"]);

            if (!Directory.Exists(Options.TempFileStorePath))
                Directory.CreateDirectory(Options.TempFileStorePath);

            using var fileStream = File.Create(Path.Join(Options.TempFileStorePath, newFile.GetPhysicalTemporaryFileName()));
            await file.WriteToStreamAsync(fileStream);
            fileStream.Position = 0;

            return BaseFile.ComputeSha256Hash(fileStream);
        }

        protected void OnUploadProgressed(FileProgressedEventArgs e)
        {
            UploadProgress = (int)e.Percentage;
        }

        protected async Task DeleteAttachedFilePropertyAsync()
        {
            if (Property.GetValue(Model) is not BaseFile oldFile)
                return;

            var eventServices = GetEventServices();
            var args = new OnBeforePropertyChangedArgs(Model, Property.Name, null, oldFile, eventServices);
            await OnBeforePropertyChanged.InvokeAsync(args);
            await Model.OnBeforePropertyChanged(args);

            await oldFile.ClearFileFromPropertyAsync(Model, Property, Service);

            var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, Property.GetValue(Model), oldFile, true, eventServices);
            await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
            await Model.OnAfterPropertyChanged(onAfterArgs);
        }

        protected virtual string GetMimeTypeOfFile(IFileEntry file)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(file.Name, out string? mimeFileType);
            return mimeFileType ?? "application/octet-stream";
        }
    }
}
