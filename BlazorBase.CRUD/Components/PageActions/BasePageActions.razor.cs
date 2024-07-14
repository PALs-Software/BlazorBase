using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.PageActions;

public partial class BasePageActions
{
    #region Parameters

    #region Events
    [Parameter] public EventCallback<Exception> OnPageActionInvoked { get; set; }
    #endregion

    [Parameter] public object? Source { get; set; }
    [Parameter] public IBaseModel? BaseModel { get; set; }
    [Parameter] public Type BaseModelType { get; set; } = default!;
    [Parameter] public EventServices EventServices { get; set; } = null!;
    [Parameter] public IStringLocalizer ModelLocalizer { get; set; } = null!;
    [Parameter] public GUIType GUIType { get; set; }
    [Parameter] public bool ShowOnlyButtons { get; set; }
    [Parameter] public object? InvokeActionParameter { get; set; }

    [Parameter] public RenderFragment<AdditionalHeaderPageActionsArgs>? AdditionalPageActions { get; set; } = null;
    #endregion

    #region Injects
    [Inject] public IStringLocalizer<PageActionGroup> PageActionGroupLocalizer { get; set; } = null!;
    #endregion

    #region Members

    protected List<PageActionGroup> VisiblePageActionGroups { get; set; } = new();
    protected string? SelectedPageActionGroup { get; set; }
    public IBaseModel? OldBaseModel { get; set; }

    protected RenderFragment? CurrentActionRenderFragment = null;

    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await GeneratePageActionsAsync();

        if (BaseModel != null)
            BaseModel.OnRecalculateVisibilityStatesOfActions += BaseModel_OnRecalculateVisibilityStatesOfActions!;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (OldBaseModel != BaseModel)
        {
            OldBaseModel = BaseModel;
            await GeneratePageActionsAsync();
        }
    }

    #endregion

    #region Generate Page Actions

    protected async Task GeneratePageActionsAsync()
    {
        var instance = BaseModel;
        if (instance == null)
            instance = Activator.CreateInstance(BaseModelType) as IBaseModel;

        VisiblePageActionGroups.Clear();
        var pageActionGroups = (await instance!.GeneratePageActionGroupsAsync(EventServices)) ?? new List<PageActionGroup>();
        foreach (var group in pageActionGroups)
            if (group.VisibleInGUITypes.Contains(GUIType) && await group.Visible(EventServices))
                VisiblePageActionGroups.Add(group);

        foreach (var group in VisiblePageActionGroups)
            foreach (var pageAction in group.PageActions.ToList())
                if (!pageAction.VisibleInGUITypes.Contains(GUIType) || pageAction.ShowAsRowButtonInList != ShowOnlyButtons || !await pageAction.Visible(EventServices))
                    group.PageActions.Remove(pageAction);

        VisiblePageActionGroups.RemoveAll(group => group.PageActions.Count == 0 && !group.PreventAutoRemovingByEmptyPageActions);
        SelectedPageActionGroup = VisiblePageActionGroups.FirstOrDefault()?.Caption;
    }

    private void BaseModel_OnRecalculateVisibilityStatesOfActions(object sender, EventArgs e)
    {
        InvokeAsync(async () =>
        {
            await GeneratePageActionsAsync();
            StateHasChanged();
        });
    }
    #endregion

    #region Page Actions

    private void SelectedPageActionGroupChanged(string name)
    {
        SelectedPageActionGroup = name;
    }

    private async Task InvokePageAction(PageAction action)
    {
        if (action.RenderComponentByActionArgs != null)
            CurrentActionRenderFragment = RenderComponentByActionArgs(action.RenderComponentByActionArgs);

        Exception? exception = null;
        try
        {
            await (action.Action?.Invoke(Source, EventServices, InvokeActionParameter) ?? Task.CompletedTask);
        }
        catch (Exception e)
        {
            exception = e;
        }

        await OnPageActionInvoked.InvokeAsync(exception);
    }

    protected RenderFragment RenderComponentByActionArgs(RenderComponentByActionArgs args) => builder =>
    {
        if (!typeof(IActionComponent).IsAssignableFrom(args.ComponentType))
            throw new NotSupportedException("The component must inherit from IActionComponent");

        builder.OpenComponent(0, args.ComponentType);
        builder.AddAttribute(1, "ComponentCanBeRemoved", EventCallback.Factory.Create(this, async (optionalParameter) => await OnComponentCanBeRemovedAsync(args, optionalParameter)));
        builder.AddAttribute(2, "Args", new ActionComponentArgs(Source, EventServices, BaseModel));

        if (args.Attributes != null)
            foreach (var arg in args.Attributes)
            {
                if (arg.Sequence <= 2)
                    throw new NotSupportedException("The sequence must be greater than 2, because the first sequences are already in use for the default parameters \"ComponentCanBeRemoved\" and \"Args\"");
#pragma warning disable ASP0006 // Do not use non-literal sequence numbers
                if (arg is ActionComponentParameterAttribute parameterAttribute)
                    builder.AddAttribute(parameterAttribute.Sequence, parameterAttribute.Name, parameterAttribute.Value);
                else if (arg is ActionComponentReferenceCaptureAttribute referenceCaptureAttribute)
                    builder.AddComponentReferenceCapture(referenceCaptureAttribute.Sequence, referenceCaptureAttribute.Value);
                else
                    throw new NotImplementedException();
#pragma warning restore ASP0006 // Do not use non-literal sequence numbers
            }

        builder.CloseComponent();
    };

    protected async Task OnComponentCanBeRemovedAsync(RenderComponentByActionArgs args, object? optionalParameter)
    {
        Exception? exception = null;
        try
        {
            await (args.OnComponentRemoved?.Invoke(Source, EventServices, BaseModel, optionalParameter) ?? Task.CompletedTask);
        }
        catch (Exception e)
        {
            exception = e;
        }

        CurrentActionRenderFragment = null;
        await OnPageActionInvoked.InvokeAsync(exception);
    }
    #endregion

}
