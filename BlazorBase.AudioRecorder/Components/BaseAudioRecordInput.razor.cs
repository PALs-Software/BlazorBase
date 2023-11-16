using BlazorBase.AudioRecorder.Models;
using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ModelServiceProviderInjection;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using static BlazorBase.AudioRecorder.Services.JSAudioRecorder;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.AudioRecorder.Components;

public partial class BaseAudioRecordInput : BaseInput, IBasePropertyCardInput
{
    #region Inject    
    [Inject] protected IStringLocalizer<BaseAudioRecordInput> Localizer { get; set; } = null!;
    [Inject] protected IBaseFileService FileService { get; set; } = null!;

    #endregion

    #region Member

    protected EventServices EventServices = null!;
    protected BaseAudioRecorder? AudioRecorder;
    protected IBaseAudioRecord CurrentAudioRecord = null!;

    protected BaseInput? FileNameInput;
    protected DisplayItem FileNameDisplayItem = null!;

    protected bool ComponentIsInitialized = false;
    #endregion

    #region Init

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        EventServices = GetEventServices();

        var currentValue = Property.GetValue(Model);
        if (currentValue is IBaseAudioRecord audioRecord)
            CurrentAudioRecord = audioRecord;
        else
            await CreateCurrentAudioRecordInstanceAsync(currentValue);

        var fileNameProperty = ServiceProvider.GetRequiredService<IBaseAudioRecord>().GetType().GetProperty(nameof(IBaseAudioRecord.FileName))!;
        FileNameDisplayItem = DisplayItem.CreateFromProperty(fileNameProperty, DisplayItem.GUIType, ServiceProvider);
        ComponentIsInitialized = true;
    }

    protected async Task CreateCurrentAudioRecordInstanceAsync(object? oldValue)
    {
        CurrentAudioRecord = (IBaseAudioRecord)Activator.CreateInstance(ServiceProvider.GetRequiredService<IBaseAudioRecord>().GetType())!;
        if (CurrentAudioRecord is IModeInjectServiceProvider injectModel)
            injectModel.ServiceProvider = ServiceProvider;
        await CurrentAudioRecord.OnCreateNewEntryInstance(new OnCreateNewEntryInstanceArgs(Model, EventServices));

        var onBeforeAddEntryArgs = new OnBeforeAddEntryArgs(Model, false, EventServices);
        await CurrentAudioRecord.OnBeforeAddEntry(onBeforeAddEntryArgs);
        await CurrentAudioRecord.OnAfterAddEntry(new OnAfterAddEntryArgs(Model, EventServices));

        await TriggerOnBeforePropertyChangedAsync(CurrentAudioRecord, oldValue);
        Property.SetValue(Model, CurrentAudioRecord);
        await TriggerOnAfterPropertyChangedAsync(CurrentAudioRecord, oldValue);
    }

    #endregion

    #region Input Interface

    public virtual Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
    {
        return Task.FromResult(typeof(IBaseAudioRecord).IsAssignableFrom(displayItem.Property.PropertyType));
    }

    public virtual Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs args) { return Task.CompletedTask; }

    public virtual Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args) { return Task.CompletedTask; }

    public virtual Task<bool> InputHasAdditionalContentChanges()
    {
        return Task.FromResult(false);
    }

    public override async Task<bool> ValidatePropertyValueAsync()
    {
        var validationIsOk = await base.ValidatePropertyValueAsync();

        if (validationIsOk && !IsReadOnly && FileNameInput != null)
            validationIsOk = await FileNameInput.ValidatePropertyValueAsync();

        return validationIsOk;
    }

    #endregion

    #region Data Handling

    protected async Task OnRecordFinishedAsync(OnRecordFinishedArgs args)
    {
        ArgumentNullException.ThrowIfNull(AudioRecorder);
        if (IsReadOnly)
            return;

        using var memoryStream = new MemoryStream();
        var getAudioDataWasSuccessful = await AudioRecorder.GetRecordedAudioDataAsync(args.AudioByteSize, memoryStream, CancellationToken.None);
        if (!getAudioDataWasSuccessful)
            return;
        var audioBytes = memoryStream.ToArray();

        var oldValue = Property.GetValue(Model);
        var onBeforePropertyChangedArgs = await TriggerOnBeforePropertyChangedAsync(audioBytes, oldValue);
        audioBytes = (byte[])(onBeforePropertyChangedArgs.NewValue ?? Array.Empty<byte>());

        if (CurrentAudioRecord.AudioFile != null)
        {
            await CurrentAudioRecord.AudioFile.RemoveFileFromDiskAsync(deleteOnlyTemporary: true);
            var entry = Service.DbContext.Entry(CurrentAudioRecord.AudioFile);
            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
        }

        CurrentAudioRecord.AudioFile = await FileService.CreateFileAsync(EventServices, CurrentAudioRecord.FileName, ".webm", "audio/webm", audioBytes);
        CurrentAudioRecord.AudioFileId = CurrentAudioRecord.AudioFile.Id;
        CurrentAudioRecord.TempAudioFileId = CurrentAudioRecord.AudioFile.TempFileId;
        CurrentAudioRecord.MimeFileType = CurrentAudioRecord.AudioFile.MimeFileType;
        CurrentAudioRecord.Hash = CurrentAudioRecord.AudioFile.Hash;

        Property.SetValue(Model, CurrentAudioRecord);
        await TriggerOnAfterPropertyChangedAsync(CurrentAudioRecord, oldValue);
    }

    protected async Task DeleteAttachedAudioFilePropertyAsync()
    {
        if (Property.GetValue(Model) is not IBaseAudioRecord audioRecord || audioRecord.AudioFile == null || audioRecord.AudioFile.FileSize == 0)
            return;

        var eventServices = GetEventServices();
        var args = new OnBeforePropertyChangedArgs(Model, Property.Name, null, audioRecord, eventServices);
        await OnBeforePropertyChanged.InvokeAsync(args);
        await Model.OnBeforePropertyChanged(args);

        audioRecord.Hash = null;
        audioRecord.FileName = String.Empty;
        await audioRecord.AudioFile.ClearFileFromPropertyAsync(audioRecord, nameof(IBaseAudioRecord.AudioFile), Service);

        if (FileNameInput != null)
            await FileNameInput.ValidatePropertyValueAsync();

        var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, audioRecord, audioRecord, true, eventServices);
        await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
        await Model.OnAfterPropertyChanged(onAfterArgs);
    }

    protected Task OnAfterFileNamePropertyChanged(OnAfterPropertyChangedArgs args)
    {
        if (CurrentAudioRecord.AudioFile != null)
            CurrentAudioRecord.AudioFile.FileName = CurrentAudioRecord.FileName;

        return OnAfterPropertyChanged.InvokeAsync(args);
    }

    #endregion

    #region Model Events
    protected async Task<OnAfterPropertyChangedArgs> TriggerOnBeforePropertyChangedAsync(object? newValue, object? oldValue)
    {
        var args = new OnAfterPropertyChangedArgs(Model, Property.Name, newValue, oldValue, true, EventServices);
        await OnAfterPropertyChanged.InvokeAsync(args);
        await Model.OnAfterPropertyChanged(args);

        return args;
    }

    protected async Task TriggerOnAfterPropertyChangedAsync(object? newValue, object? oldValue)
    {
        var onAfterArgs = new OnAfterPropertyChangedArgs(Model, Property.Name, newValue, oldValue, true, EventServices);
        await OnAfterPropertyChanged.InvokeAsync(onAfterArgs);
        await Model.OnAfterPropertyChanged(onAfterArgs);
    }
    #endregion
}
