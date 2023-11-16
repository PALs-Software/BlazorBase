using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ModelServiceProviderInjection;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Attributes;
using BlazorBase.Files.Models;
using Blazorise;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.Files.Components;

public partial class MultiFileUploadInput : BaseFileInput
{
    #region Member
    protected string? CurrentUploadFileName { get; set; } = null;
    protected int NoOfFilesToUpload { get; set; } = 0;
    protected int CurrentFileUploadNo { get; set; } = 0;
    #endregion

    public override Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
    {
        return Task.FromResult(
            Attribute.IsDefined(displayItem.Property, typeof(ShowAsMultiFileUploadInputAttribute)) &&
            displayItem.IsListProperty
        );
    }

    protected override async Task OnValueChangedAsync(object? fileChangedEventArgs, bool setCurrentValueAsString = true)
    {
        if (FileEditIsResetting || fileChangedEventArgs == null || FileEdit == null)
            return;

        var eventServices = GetEventServices();
        var oldValue = Property.GetValue(Model);
        if (oldValue == null)
            Property.SetValue(Model, CreateGenericListInstance());

        var propertyList = (IList)Property.GetValue(Model)!;

        bool valid = true;
        IBaseFile? newFile = null;
        try
        {
            var files = ((FileChangedEventArgs)fileChangedEventArgs).Files;
            if (files.Length == 0)
                return;

            CurrentFileUploadNo = 1;
            NoOfFilesToUpload = files.Length;
            ShowLoadingIndicator = true;

            var args = new OnBeforePropertyChangedArgs(Model, Property.Name, fileChangedEventArgs, oldValue, eventServices);
            await OnBeforePropertyChanged.InvokeAsync(args);
            await Model.OnBeforePropertyChanged(args);
            fileChangedEventArgs = args.NewValue;

            foreach (var file in files)
            {
                if (MaxFileSize != null && MaxFileSize != 0 && (ulong)file.Size > MaxFileSize)
                    throw new IOException(Localizer["The file exceed the maximum allowed file size of {0} bytes", MaxFileSize]);
            }
       
            foreach (var file in files)
            {
                UploadProgress = 0;
                CurrentUploadFileName = file.Name;

                newFile = (IBaseFile)Activator.CreateInstance(ServiceProvider.GetRequiredService<IBaseFile>().GetType())!;
                newFile.FileName = Path.GetFileNameWithoutExtension(file.Name);
                newFile.FileSize = file.Size;
                newFile.BaseFileType = Path.GetExtension(file.Name);
                newFile.MimeFileType = GetMimeTypeOfFile(file);

                if (newFile is IModeInjectServiceProvider injectModel)
                    injectModel.ServiceProvider = ServiceProvider;

                await OnCreateNewListEntryInstanceAsync(newFile);
                var handleArgs = new HandledEventArgs();
                await OnBeforeAddEntryAsync(newFile, handleArgs);
                if (handleArgs.Handled)
                    return;

                var tempFileStorePath = Options.TempFileStorePath;
                string tempFilePath;
                do
                {
                    newFile.TempFileId = Guid.NewGuid();
                    tempFilePath = Path.Join(tempFileStorePath, newFile.GetPhysicalTemporaryFileName());
                } while (File.Exists(tempFilePath));

                newFile.Hash = await WriteFileStreamToTempFileStore(file, newFile);
                propertyList.Add(newFile);

                await OnAfterAddEntryAsync(newFile);
                CurrentFileUploadNo++;
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

    protected object CreateGenericListInstance()
    {
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
        return Activator.CreateInstance(constructedListType)!;
    }

    protected async Task OnCreateNewListEntryInstanceAsync(object newEntry)
    {
        var onCreateNewListEntryInstanceArgs = new OnCreateNewListEntryInstanceArgs(Model, newEntry, EventServices);
        await Model.OnCreateNewListEntryInstance(onCreateNewListEntryInstanceArgs);

        if (newEntry is not IBaseModel newBaseEntry)
            return;

        var onCreateNewEntryInstanceArgs = new OnCreateNewEntryInstanceArgs(Model, EventServices);
        await newBaseEntry.OnCreateNewEntryInstance(onCreateNewEntryInstanceArgs);
    }

    protected async Task OnBeforeAddEntryAsync(object? newEntry, HandledEventArgs args, bool callAddEventOnListEntry = true)
    {
        var onBeforeAddListEntryArgs = new OnBeforeAddListEntryArgs(Model, newEntry, false, EventServices);
        await Model.OnBeforeAddListEntry(onBeforeAddListEntryArgs);
        if (onBeforeAddListEntryArgs.AbortAdding)
        {
            args.Handled = true;
            return;
        }

        if (callAddEventOnListEntry && newEntry is IBaseModel newBaseEntry)
        {
            var onBeforeAddEntryArgs = new OnBeforeAddEntryArgs(Model, false, EventServices);
            await newBaseEntry.OnBeforeAddEntry(onBeforeAddEntryArgs);
            if (onBeforeAddEntryArgs.AbortAdding)
            {
                args.Handled = true;
                return;
            }
        }
    }

    protected async Task OnAfterAddEntryAsync(object newEntry, bool callAddEventOnListEntry = true)
    {
        var onAfterAddListEntryArgs = new OnAfterAddListEntryArgs(Model, newEntry, EventServices);
        await Model.OnAfterAddListEntry(onAfterAddListEntryArgs);

        if (callAddEventOnListEntry && newEntry is IBaseModel addedBaseEntry)
            await addedBaseEntry.OnAfterAddEntry(new OnAfterAddEntryArgs(Model, EventServices));
    }
}
