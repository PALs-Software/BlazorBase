using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.RichTextEditor.Components;

public partial class BaseRichTextEditorInput : BaseInput, IBasePropertyCardInput, IBasePropertyListPartInput
{
    #region Member
    protected EventServices EventServices = null!;
    protected BaseRichTextEditor? BaseRichTextEditor;
    protected bool HasContentChanges = false;
    protected bool ChangesFromContentSaving = false;
    protected int InitCounter = 0;
    protected string? CurrentContentBuffer = null;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        EventServices = new EventServices(ServiceProvider, ModelLocalizer, Service, MessageHandler);

        SkipCustomSetParametersAsync = true;
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (Property == null || Model == null)
            return;

        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI();
        else
            IsReadOnly = ReadOnly.Value;
    }

    public Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
    {
        var presentationDataType = displayItem.Property.GetCustomAttribute<DataTypeAttribute>()?.DataType;

        return Task.FromResult(presentationDataType == DataType.Html);
    }

    public Task<bool> InputHasAdditionalContentChanges()
    {
        return Task.FromResult(HasContentChanges);
    }

    public async Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs? args)
    {
        if (BaseRichTextEditor == null)
            return;

        CurrentContentBuffer = await BaseRichTextEditor.GetContentAsync();

        HasContentChanges = false;
        ChangesFromContentSaving = true;

        await OnValueChangedAsync(CurrentContentBuffer, setCurrentValueAsString: false);
    }

    public Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args)
    {
        CurrentValueAsString = CurrentContentBuffer; // Delay setting CurrentValueAsString so file controller is able to serve new created image files
        return Task.CompletedTask;
    }

    protected void OnContentChanged()
    {
        if (InitCounter < 2) // Skip the first content changes due to the initialization of the editor
        {
            InitCounter++;
            return;
        }

        if (ChangesFromContentSaving)
        {
            ChangesFromContentSaving = false;
            return;
        }

        HasContentChanges = true;
    }
}
