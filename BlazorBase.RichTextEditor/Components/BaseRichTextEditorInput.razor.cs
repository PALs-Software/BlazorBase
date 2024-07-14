using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.Models;
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

    protected bool HidePropertyName = false;
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        EventServices = new EventServices(ServiceProvider, DbContext, ModelLocalizer);

        SkipCustomSetParametersAsync = true;
    }

    public virtual Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, IDisplayItem displayItem, EventServices eventServices)
    {
        var presentationDataType = displayItem.Property.GetCustomAttribute<DataTypeAttribute>()?.DataType;

        return Task.FromResult(presentationDataType == DataType.Html);
    }

    public virtual Task<bool> InputHasAdditionalContentChanges()
    {
        return Task.FromResult(HasContentChanges);
    }

    public override async Task<bool> ValidatePropertyValueAsync(bool calledFromOnValueChangedAsync = false)
    {
        if (!calledFromOnValueChangedAsync)
            await OnBeforeCardSaveChanges(null);

        return await base.ValidatePropertyValueAsync();
    }

    public virtual async Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs? args)
    {
        CurrentContentBuffer = CurrentValueAsString;
        if (BaseRichTextEditor == null || IsReadOnly)
            return;

        CurrentContentBuffer = await BaseRichTextEditor.GetContentAsync();
        if (CurrentContentBuffer == "<p><br></p>")
            CurrentContentBuffer = null;

        HasContentChanges = false;
        ChangesFromContentSaving = true;

        await OnValueChangedAsync(CurrentContentBuffer, setCurrentValueAsString: false);
    }

    public virtual Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args)
    {
        CurrentValueAsString = CurrentContentBuffer; // Delay setting CurrentValueAsString so file controller is able to serve new created image files
        return Task.CompletedTask;
    }

    protected virtual void OnContentChanged()
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
