using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseDataListInput
{
    #region Properties

    [Parameter] public string? PlaceHolder { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public List<(string Value, string Text)> Data { get; set; } = new List<(string Value, string Text)>();
    [Parameter] public EventCallback<string> OnValueChanged { get; set; }
    [Parameter] public bool ResetValueAfterSelection { get; set; }
    #endregion

    #region Injects
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    #endregion

    #region Member
    protected DotNetObjectReference<BaseDataListInput>? DotNetReference;
    protected string Id = Guid.NewGuid().ToString();
    protected string? CurrentInputValue;
    #endregion

    #region Init

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        DotNetReference = DotNetObjectReference.Create(this);
        await JSRuntime.InvokeVoidAsync("blazorBase.crud.blazorbaseDataListInput.init", Id, DotNetReference, ResetValueAfterSelection);
    }

    #endregion

    #region Events

    [JSInvokable]
    public void ValueChanged(string value)
    {
        OnValueChanged.InvokeAsync(value);
    }

    #endregion
}
