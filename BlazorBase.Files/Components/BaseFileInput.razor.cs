using BlazorBase.CRUD.Components;
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
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.Files.Components
{
    public partial class BaseFileInput : BaseInput, IBasePropertyCardInput, IBasePropertyListPartInput
    {
        #region Parameters
        [Parameter] public string FileFilter { get; set; } = null;
        #endregion

        #region Inject
        [Inject] protected IStringLocalizer<BaseFileInput> Localizer { get; set; }
        [Inject] protected BlazorBaseFileOptions Options { get; set; }
        #endregion

        #region Member
        protected bool ShowLoadingIndicator = false;
        protected int UploadProgress = 0;
        protected FileEdit FileEdit = default;
        protected bool FileEditIsResetting = false;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var filterAttribute = Property.GetCustomAttribute(typeof(FileInputFilterAttribute)) as FileInputFilterAttribute;
            if (FileFilter == null)
                FileFilter = filterAttribute?.Filter ?? "*.";
        }

        public override Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
        {
            return Task.FromResult(typeof(BaseFile).IsAssignableFrom(displayItem.Property.PropertyType));
        }

        protected override async Task OnValueChangedAsync(object fileChangedEventArgs)
        {
            if (FileEditIsResetting)
                return;

            var eventServices = GetEventServices();
            bool valid = true;
            try
            {
                var args = new OnBeforePropertyChangedArgs(Model, Property.Name, fileChangedEventArgs, eventServices);
                await OnBeforePropertyChanged.InvokeAsync(args);
                await Model.OnBeforePropertyChanged(args);
                fileChangedEventArgs = args.NewValue;

                ShowLoadingIndicator = true;

                foreach (var file in ((FileChangedEventArgs)fileChangedEventArgs).Files)
                {
                    new FileExtensionContentTypeProvider().TryGetContentType(file.Name, out string mimeFileType);

                    var newFile = Activator.CreateInstance(Property.PropertyType) as BaseFile;
                    newFile.FileName = Path.GetFileNameWithoutExtension(file.Name);
                    newFile.FileSize = file.Size;
                    newFile.BaseFileType = Path.GetExtension(file.Name);
                    newFile.MimeFileType = mimeFileType;

                    if (Model is BaseFile baseFile)
                    {
                        if (baseFile.TempFileId == Guid.Empty)
                        {
                            var tempFileStorePath = BlazorBaseFileOptions.Instance.TempFileStorePath;
                            string tempFilePath;
                            do
                            {
                                newFile.TempFileId = Guid.NewGuid();
                                tempFilePath = Path.Join(tempFileStorePath, newFile.TempFileId.ToString());
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
                        if (Property.GetValue(Model) is BaseFile oldFile)
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

                var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, fileChangedEventArgs, valid, eventServices);
                await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
                await Model.OnAfterPropertyChanged(onAfterArgs);
            }
            catch (Exception e)
            {
                LastValueConversionFailed = true;
                SetValidation(feedback: ErrorHandler.PrepareExceptionErrorMessage(e));
            }           
        }

        protected async Task<string> WriteFileStreamToTempFileStore(IFileEntry file, BaseFile newFile)
        {
            if (file.Size == 0)
                return null;

            if (newFile.TempFileId == Guid.Empty)
                throw new CRUDException(Localizer["The Id can not be empty"]);

            if (!Directory.Exists(Options.TempFileStorePath))
                Directory.CreateDirectory(Options.TempFileStorePath);

            using var fileStream = File.Create(Path.Join(Options.TempFileStorePath, newFile.TempFileId.ToString()));
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

            await oldFile.ClearFileFromPropertyAsync(Model, Property, Service);
        }

    }
}
