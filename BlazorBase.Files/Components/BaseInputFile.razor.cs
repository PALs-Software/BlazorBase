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

        protected bool ShowLoadingIndicator { get; set; } = false;
        protected int UploadProgress { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        public override Task<bool> IsHandlingPropertyAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
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
                    using var stream = new MemoryStream();
                    await file.WriteToStreamAsync(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    new FileExtensionContentTypeProvider().TryGetContentType(file.Name, out string mimeFileType);
                    var newFile = new BaseFile
                    {
                        FileName = Path.GetFileNameWithoutExtension(file.Name),
                        FileSize = file.Size,
                        BaseFileType = Path.GetExtension(file.Name),
                        MimeFileType = mimeFileType,
                        Data = stream.ToArray()
                    };

                    if (Model is BaseFile baseFile)
                    {
                        baseFile.FileName = newFile.FileName;
                        baseFile.FileSize = newFile.FileSize;
                        baseFile.BaseFileType = newFile.BaseFileType;
                        baseFile.MimeFileType = newFile.MimeFileType;
                        baseFile.Data = newFile.Data;
                    }
                    else
                    {
                        await newFile.OnBeforeAddEntry(new OnBeforeAddEntryArgs(newFile, false, eventServices));
                        Property.SetValue(Model, newFile);
                        await newFile.OnAfterAddEntry(new OnAfterAddEntryArgs(newFile, eventServices));
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
