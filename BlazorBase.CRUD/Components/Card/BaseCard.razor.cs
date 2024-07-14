using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Extensions;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.Components.List;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ModelServiceProviderInjection;
using BlazorBase.Models;
using Blazorise.Snackbar;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.Card;

public partial class BaseCard<TModel> : BaseDisplayComponent, IBaseCard where TModel : class, IBaseModel, new()
{
    #region Parameter

    #region Events

    [Parameter] public EventCallback<string> OnTitleCalculated { get; set; }

    [Parameter] public EventCallback<OnCreateNewEntryInstanceArgs> OnCreateNewEntryInstance { get; set; }
    [Parameter] public EventCallback<OnGuiLoadDataArgs> OnGuiLoadData { get; set; }
    [Parameter] public EventCallback<OnShowEntryArgs> OnShowEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeAddEntryArgs> OnBeforeAddEntry { get; set; }
    [Parameter] public EventCallback<OnAfterAddEntryArgs> OnAfterAddEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeUpdateEntryArgs> OnBeforeUpdateEntry { get; set; }
    [Parameter] public EventCallback<OnAfterUpdateEntryArgs> OnAfterUpdateEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterCardSaveChangesArgs> OnAfterSaveChanges { get; set; }
    [Parameter] public EventCallback<OnBeforeCardSaveChangesArgs> OnBeforeSaveChanges { get; set; }

    #region List Events
    [Parameter] public EventCallback<OnCreateNewListEntryInstanceArgs> OnCreateNewListEntryInstance { get; set; }
    [Parameter] public EventCallback<OnBeforeAddListEntryArgs> OnBeforeAddListEntry { get; set; }
    [Parameter] public EventCallback<OnAfterAddListEntryArgs> OnAfterAddListEntry { get; set; }
    [Parameter] public EventCallback<OnBeforeConvertListPropertyTypeArgs> OnBeforeConvertListPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforeListPropertyChangedArgs> OnBeforeListPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterListPropertyChangedArgs> OnAfterListPropertyChanged { get; set; }
    [Parameter] public EventCallback<OnBeforeRemoveListEntryArgs> OnBeforeRemoveListEntry { get; set; }
    [Parameter] public EventCallback<OnAfterRemoveListEntryArgs> OnAfterRemoveListEntry { get; set; }
    [Parameter] public EventCallback<OnAfterMoveListEntryUpArgs> OnAfterMoveListEntryUp { get; set; }
    [Parameter] public EventCallback<OnAfterMoveListEntryDownArgs> OnAfterMoveListEntryDown { get; set; }
    #endregion

    #endregion

    [Parameter] public string? SingleDisplayName { get; set; }
    [Parameter] public bool Embedded { get; set; }
    [Parameter] public bool ShowEntryByStart { get; set; }
    [Parameter] public Func<OnEntryToBeShownByStartArgs, Task<IBaseModel>>? EntryToBeShownByStart { get; set; } = null;
    [Parameter] public TModel? ComponentModelInstance { get; set; } = null;
    [Parameter] public bool ShowActions { get; set; } = true;
    [Parameter] public RenderFragment<AdditionalHeaderPageActionsArgs> AdditionalHeaderPageActions { get; set; } = null!;
    #endregion

    #region Injects
    [Inject] protected IBaseDbContext DbContext { get; set; } = null!;
    [Inject] protected IStringLocalizer<TModel> ModelLocalizer { get; set; } = null!;
    [Inject] protected IStringLocalizer<BaseCard<TModel>> Localizer { get; set; } = null!;
    [Inject] protected IBlazorBaseOptions BlazorBaseOptions { get; set; } = null!;
    #endregion

    #region Properties
    public TModel? CurrentModelInstance { get { return Model; } }
    public IBaseModel? CurrentBaseModelInstance { get { return Model; } }
    #endregion

    #region Member
    protected EventServices EventServices = null!;

    protected Snackbar? Snackbar;

    protected TModel Model = null!;
    protected Type TModelType = null!;

    protected bool ModelLoaded = false;
    protected bool AddingMode;
    protected bool ViewMode;

    protected ValidationContext ValidationContext = null!;

    protected string Title = String.Empty;
    protected string PageTitle = String.Empty;

    protected Dictionary<string, SkipExplicitNavigationLoadingOnCardOpenAttribute> SkipNavigationLoadingOnCardOpenProperties = new();
    #endregion

    #region Property Infos
    protected List<BaseInput?> BaseInputs = new();
    protected List<BaseSelectListInput?> BaseSelectListInputs = new();
    protected List<BaseListPart?> BaseListParts = new();
    protected List<IBasePropertyCardInput> BasePropertyCardInputs = new();

    protected BaseInput? AddToBaseInputs { set { BaseInputs.Add(value); } }
    protected BaseSelectListInput? AddToBaseSelectListInputs { set { BaseSelectListInputs.Add(value); } }
    protected BaseListPart? AddToBaseListParts { set { BaseListParts.Add(value); } }

    protected List<IBasePropertyCardInput> BaseInputExtensions = new();
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        EventServices = GetEventServices();

        TModelType = typeof(TModel);

        if (String.IsNullOrEmpty(SingleDisplayName))
            SingleDisplayName = ModelLocalizer[TModelType.Name];

        BaseInputExtensions = ServiceProvider.GetServices<IBasePropertyCardInput>().ToList();

        await SetUpDisplayListsAsync(TModelType, GUIType.Card, ComponentModelInstance);

        SkipNavigationLoadingOnCardOpenProperties = TModelType.GetProperties().Where(entry => Attribute.IsDefined(entry, typeof(SkipExplicitNavigationLoadingOnCardOpenAttribute))).ToDictionary(entry => entry.Name, entry => entry.GetCustomAttribute<SkipExplicitNavigationLoadingOnCardOpenAttribute>()!);

        if (ShowEntryByStart)
        {
            IBaseModel? entry = null;
            var args = new OnEntryToBeShownByStartArgs(EventServices);
            var task = EntryToBeShownByStart?.Invoke(args);
            if (task != null)
                entry = await task;
            await ShowAsync(entry == null, args.ViewMode, entry?.GetPrimaryKeys());
        }
    }

    protected virtual async Task<RenderFragment?> CheckIfPropertyRenderingIsHandledAsync(IDisplayItem displayItem, bool isReadonly)
    {
        foreach (var baseinput in BaseInputExtensions)
            if (await baseinput.IsHandlingPropertyRenderingAsync(Model, displayItem, EventServices))
                return GetBaseInputExtensionAsRenderFragment(displayItem, isReadonly, baseinput.GetType(), Model);

        return null;
    }
    protected RenderFragment GetBaseInputExtensionAsRenderFragment(IDisplayItem displayItem, bool isReadonly, Type baseInputExtensionType, IBaseModel model) => builder =>
    {
        builder.OpenComponent(0, baseInputExtensionType);

        builder.AddAttribute(1, "Model", model);
        builder.AddAttribute(2, "Property", displayItem.Property);
        builder.AddAttribute(3, "ReadOnly", isReadonly);
        builder.AddAttribute(4, "DbContext", DbContext);
        builder.AddAttribute(5, "ModelLocalizer", ModelLocalizer);
        builder.AddAttribute(6, "DisplayItem", displayItem);

        builder.AddAttribute(7, "OnBeforeConvertPropertyType", EventCallback.Factory.Create<OnBeforeConvertPropertyTypeArgs>(this, (args) => OnBeforeConvertPropertyType.InvokeAsync(args)));
        builder.AddAttribute(8, "OnBeforePropertyChanged", EventCallback.Factory.Create<OnBeforePropertyChangedArgs>(this, (args) => OnBeforePropertyChanged.InvokeAsync(args)));
        builder.AddAttribute(9, "OnAfterPropertyChanged", EventCallback.Factory.Create<OnAfterPropertyChangedArgs>(this, (args) => OnAfterPropertyChanged.InvokeAsync(args)));

        builder.AddComponentReferenceCapture(10, (input) => BasePropertyCardInputs.Add((IBasePropertyCardInput)input));

        builder.CloseComponent();
    };

    #endregion

    #region Actions
    public virtual async Task ShowAsync(bool addingMode, bool viewMode, object?[]? primaryKeys = null, IBaseModel? template = null)
    {
        await DbContext.RefreshDbContextAsync();

        ModelLoaded = false;
        AddingMode = addingMode;
        ViewMode = viewMode;
        BaseInputs.Clear();
        BaseSelectListInputs.Clear();
        BaseListParts.Clear();
        BasePropertyCardInputs.Clear();
        ResetInvalidFeedback();

        TModel? model;
        if (AddingMode)
        {
            if (template == null)
                model = new TModel();
            else
                model = (TModel)template;

            if (model is IModelInjectServiceProvider injectModel)
                injectModel.ServiceProvider = ServiceProvider;

            var args = new OnCreateNewEntryInstanceArgs(model, EventServices);
            await OnCreateNewEntryInstance.InvokeAsync(args);
            await model.OnCreateNewEntryInstance(args);
        }
        else
            model = await DbContext.FindWithAllNavigationPropertiesAsync<TModel>(SkipNavigationLoadingOnCardOpenProperties.Keys, primaryKeys); //Load all properties so the dbcontext dont load entries via lazy loading in parallel and crash

        if (model == null)
            throw new CRUDException(Localizer["Can not find Entry with the Primarykeys {0} for displaying in Card", String.Join(", ", primaryKeys ?? new object())]);
        Model = model;

        var onGuiLoadDataArgs = new OnGuiLoadDataArgs(GUIType.Card, Model, null, EventServices);
        await OnGuiLoadData.InvokeAsync(onGuiLoadDataArgs);
        Model.OnGuiLoadData(onGuiLoadDataArgs);

        var onAfterShowEntryArgs = new OnShowEntryArgs(GUIType.Card, Model, addingMode, viewMode, VisibleProperties, DisplayGroups, EventServices);
        await OnShowEntry.InvokeAsync(onAfterShowEntryArgs);
        await Model.OnShowEntry(onAfterShowEntryArgs);

        await PrepareForeignKeyProperties(DbContext, Model);
        await PrepareCustomLookupData(Model, EventServices);

        ValidationContext = new ValidationContext(Model, ServiceProvider, new Dictionary<object, object?>()
        {
            [typeof(IStringLocalizer)] = ModelLocalizer,
            [typeof(IBaseDbContext)] = DbContext
        });

        CalculateTitle(addingMode: AddingMode);

        Model.OnReloadEntityFromDatabase += async (sender, e) => await Entry_OnReloadEntityFromDatabaseAsync(sender, e);
        Model.OnRecalculateCustomLookupData += async(sender, e) => await Entry_OnRecalculateCustomLookupDataAsync(sender, e);
        ModelLoaded = true;
    }

    protected Task Entry_OnReloadEntityFromDatabaseAsync(object? sender, EventArgs e)
    {
        return ReloadEntityFromDatabase();
    }

    public virtual async Task ReloadEntityFromDatabase()
    {
        if (Model == null)
            return;

        await ShowAsync(false, ViewMode, Model.GetPrimaryKeys());
        await InvokeAsync(StateHasChanged);
    }

    protected Task Entry_OnRecalculateCustomLookupDataAsync(object? sender, string[] propertyNames)
    {
        return RecalculateCustomLookupData(Model, EventServices, propertyNames);
    }

    public virtual async Task<bool> SaveCardAsync(bool showSnackBar = true)
    {
        ResetInvalidFeedback();

        if (!await CardIsValidAsync())
            return false;

        var success = true;
        try
        {
            if (AddingMode)
            {
                var args = new OnBeforeAddEntryArgs(Model, false, EventServices);
                await OnBeforeAddEntry.InvokeAsync(args);
                await Model.OnBeforeAddEntry(args);
                if (args.AbortAdding)
                    return false;
               
                if (await DbContext.ExistsAsync<TModel>(Model))
                {
                    ShowFormattedInvalidFeedback(Localizer["EntryAlreadyExistError", Model.GetPrimaryKeysAsString()]);
                    if (showSnackBar)
                        Snackbar?.Show();
                    return false;
                }
                await DbContext.AddAsync(Model);

                var onAfterArgs = new OnAfterAddEntryArgs(Model, EventServices);
                await OnAfterAddEntry.InvokeAsync(onAfterArgs);
                await Model.OnAfterAddEntry(onAfterArgs);
                CalculateTitle(addingMode: false);
            }
            else
            {
                var args = new OnBeforeUpdateEntryArgs(Model, false, EventServices);
                await OnBeforeUpdateEntry.InvokeAsync(args);
                await Model.OnBeforeUpdateEntry(args);
                if (args.AbortUpdating)
                    return false;

                await DbContext.UpdateAsync(Model);

                var onAfterArgs = new OnAfterUpdateEntryArgs(Model, EventServices);
                await OnAfterUpdateEntry.InvokeAsync(onAfterArgs);
                await Model.OnAfterUpdateEntry(onAfterArgs);
            }

            await InvokeOnBeforeSaveChangesEvents(EventServices);
            await DbContext.SaveChangesAsync();
            AddingMode = false;
            await InvokeOnAfterSaveChangesEvents(EventServices);
        }
        catch (CRUDException e)
        {
            ShowFormattedInvalidFeedback(ErrorHandler.PrepareExceptionErrorMessage(e));
            success = false;
        }
        catch (DbUpdateConcurrencyException)
        {
            ShowFormattedInvalidFeedback(Localizer["Another user has modified the data for this {0} after you retrieved it from the database. Reload the view and reenter your changes.", SingleDisplayName ?? String.Empty]);
            success = false;
        }
        catch (Exception e)
        {
            ShowFormattedInvalidFeedback(Localizer["UnknownSavingError", ErrorHandler.PrepareExceptionErrorMessage(e)]);
            success = false;
        }

        if (showSnackBar)
            Snackbar?.Show();
        return success;
    }

    public void ResetCard()
    {
        BaseInputs?.Clear();
        BaseSelectListInputs?.Clear();
        BaseListParts?.Clear();
        ForeignKeyProperties = null!;
        CachedForeignKeys = new Dictionary<Type, List<KeyValuePair<string?, string>>>();
        Model = null!;
        ModelLoaded = false;
    }

    public IBaseModel GetCurrentModel()
    {
        return Model;
    }

    public async Task StateHasChangedAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    protected void CalculateTitle(bool addingMode)
    {
        var singleDisplayName = SingleDisplayName ?? String.Empty;
        Title = $"{Localizer[ViewMode ? "View {0}" : "Edit {0}", singleDisplayName]}{(addingMode ? "" : $" • {@Model.GetDisplayKey("• ")}")}";
        PageTitle = $"{(addingMode ? "" : $"{@Model.GetDisplayKey("• ")} • ")}{Localizer[ViewMode ? "View {0}" : "Edit {0}", singleDisplayName]}";

        OnTitleCalculated.InvokeAsync(Title);
    }
    #endregion

    #region Events

    protected virtual async Task InvokeOnBeforeSaveChangesEvents(EventServices eventServices)
    {
        var onBeforeSaveChangesArgs = new OnBeforeCardSaveChangesArgs(Model, false, eventServices);
        await OnBeforeSaveChanges.InvokeAsync(onBeforeSaveChangesArgs);

        foreach (var basePropertyCardInput in BasePropertyCardInputs)
            await basePropertyCardInput.OnBeforeCardSaveChanges(onBeforeSaveChangesArgs);

        await Model.OnBeforeCardSaveChanges(onBeforeSaveChangesArgs);

        onBeforeSaveChangesArgs = new OnBeforeCardSaveChangesArgs(Model, true, eventServices);
        foreach (PropertyInfo property in TModelType.GetIBaseModelProperties())
        {
            if (SkipNavigationLoadingOnCardOpenProperties.ContainsKey(property.Name) && SkipNavigationLoadingOnCardOpenProperties[property.Name].SkipCardSaveChangesEventsForThisPropertyToPreventLoading)
                continue;

            if (property.IsListProperty())
            {
                if (property.GetValue(Model) is IList list)
                    foreach (IBaseModel item in list)
                        if (item != null)
                            await item.OnBeforeCardSaveChanges(onBeforeSaveChangesArgs);
            }
            else
            {
                if (property.GetValue(Model) is IBaseModel value)
                    await value.OnBeforeCardSaveChanges(onBeforeSaveChangesArgs);
            }
        }
    }

    protected virtual async Task InvokeOnAfterSaveChangesEvents(EventServices eventServices)
    {
        var onAfterSaveChangesArgs = new OnAfterCardSaveChangesArgs(Model, false, eventServices);
        await OnAfterSaveChanges.InvokeAsync(onAfterSaveChangesArgs);

        foreach (var basePropertyCardInput in BasePropertyCardInputs)
            await basePropertyCardInput.OnAfterCardSaveChanges(onAfterSaveChangesArgs);

        await Model.OnAfterCardSaveChanges(onAfterSaveChangesArgs);

        onAfterSaveChangesArgs = new OnAfterCardSaveChangesArgs(Model, true, eventServices);
        foreach (PropertyInfo property in TModelType.GetIBaseModelProperties())
        {
            if (SkipNavigationLoadingOnCardOpenProperties.ContainsKey(property.Name) && SkipNavigationLoadingOnCardOpenProperties[property.Name].SkipCardSaveChangesEventsForThisPropertyToPreventLoading)
                continue;

            if (property.IsListProperty())
            {
                if (property.GetValue(Model) is IList list)
                    foreach (IBaseModel item in list)
                        if (item != null)
                            await item.OnAfterCardSaveChanges(onAfterSaveChangesArgs);
            }
            else
            {
                if (property.GetValue(Model) is IBaseModel value)
                    await value.OnAfterCardSaveChanges(onAfterSaveChangesArgs);
            }
        }

        foreach (var baseListPart in BaseListParts)
            baseListPart?.OnAfterCardSaveChanges();
    }

    #endregion

    #region Validation
    public async Task<bool> HasUnsavedChangesAsync()
    {
        foreach (var basePropertyCardInput in BasePropertyCardInputs)
            if (await basePropertyCardInput.InputHasAdditionalContentChanges())
                return true;

        return await DbContext.HasUnsavedChangesAsync();
    }

    protected virtual async Task<bool> CardIsValidAsync()
    {
        var valid = true;

        foreach (var input in BaseInputs)
            if (input != null && !await input.ValidatePropertyValueAsync())
                valid = false;

        foreach (var input in BaseSelectListInputs)
            if (input != null && !await input.ValidatePropertyValueAsync())
                valid = false;

        foreach (var listPart in BaseListParts)
            if (listPart != null && !await listPart.ListPartIsValidAsync())
                valid = false;

        foreach (var basePropertyCardInput in BasePropertyCardInputs)
            if (basePropertyCardInput != null && !await basePropertyCardInput.ValidatePropertyValueAsync())
                valid = false;

        if (!Model.TryValidate(out _, ValidationContext))
            valid = false;

        if (!valid)
        {
            ShowFormattedInvalidFeedback(Localizer["Some properties are not valid"]);
            Snackbar?.Show();
        }

        return valid;
    }
    #endregion

    #region Other
    protected EventServices GetEventServices()
    {
        return new EventServices(ServiceProvider, DbContext, ModelLocalizer);
    }

    public bool CardIsInAddingMode()
    {
        return AddingMode;
    }

    public bool CardIsInViewMode()
    {
        return ViewMode;
    }
    #endregion
}