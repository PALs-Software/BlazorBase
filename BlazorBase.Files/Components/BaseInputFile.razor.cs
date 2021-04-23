using BlazorBase.CRUD.Components;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.BaseDisplayComponent;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.Files.Components
{
    public partial class BaseInputFile : BaseInput
    {
        [Parameter] public string FileFilter { get; set; } = "*.";

        [Inject] protected IStringLocalizer<BaseInputFile> Localizer { get; set; }
        [Inject] protected BlazorBaseFileOptions Options { get; set; }

        protected bool ShowLoadingIndicator { get; set; } = false;
        protected int UploadProgress { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        public override Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
        {
            return Task.FromResult(typeof(BaseFile).IsAssignableFrom(displayItem.Property.PropertyType));
        }

        protected override async Task OnValueChangedAsync(object fileChangedEventArgs)
        {
            var eventServices = GetEventServices();

            var args = new OnBeforePropertyChangedArgs(Model, Property.Name, fileChangedEventArgs, eventServices);
            await OnBeforePropertyChanged.InvokeAsync(args);
            await Model.OnBeforePropertyChanged(args);
            fileChangedEventArgs = args.NewValue;

            bool valid = true;
            try
            {
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
                        baseFile.TempFileId = Guid.NewGuid();
                        newFile.TempFileId = baseFile.TempFileId;

                        baseFile.FileName = newFile.FileName;
                        baseFile.FileSize = newFile.FileSize;
                        baseFile.BaseFileType = newFile.BaseFileType;
                        baseFile.MimeFileType = newFile.MimeFileType;

                        baseFile.ForcePropertyRepaint(nameof(BaseFile.FileName));
                    }
                    else
                    {
                        if (Property.GetValue(Model) is BaseFile oldFile)
                        {
                            await oldFile.RemoveFileFromDisk(ServiceProvider, deleteOnlyTemporary: true);
                            Service.DbContext.Entry(oldFile).State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
                        }

                        await newFile.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(Model, eventServices));
                        newFile.TempFileId = Guid.NewGuid();

                        await newFile.OnBeforeAddEntry(new OnBeforeAddEntryArgs(newFile, false, eventServices));
                        Property.SetValue(Model, newFile);
                        await newFile.OnAfterAddEntry(new OnAfterAddEntryArgs(newFile, eventServices));

                        Service.DbContext.Entry(newFile).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }

                    if (file.Size != 0)
                    {
                        if (newFile.TempFileId == Guid.Empty)
                            throw new CRUDException(Localizer["The Id can not be empty"]);

                        if (!Directory.Exists(Options.TempFileStorePath))
                            Directory.CreateDirectory(Options.TempFileStorePath);

                        using var fileStream = File.Create(Path.Join(Options.TempFileStorePath, newFile.TempFileId.ToString()));
                        await file.WriteToStreamAsync(fileStream);
                    }

                    SetValidation(showValidation: false);
                }


            }
            catch (Exception ex)
            {
                valid = false;
                SetValidation(feedback: ex.Message);
            }
            finally
            {
                ShowLoadingIndicator = false;
            }

            if (valid)
                await ReloadForeignProperties(fileChangedEventArgs);

            var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, fileChangedEventArgs, valid, eventServices);
            await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
            await Model.OnAfterPropertyChanged(onAfterArgs);
        }

        void OnUploadProgressed(FileProgressedEventArgs e)
        {
            UploadProgress = (int)e.Percentage;
        }
    }
}
