using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Inputs;
using BlazorBase.CRUD.Components.SelectList;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ModelServiceProviderInjection;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.SortableItem;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.SelectList.BaseTypeBasedSelectList;

namespace BlazorBase.CRUD.Components.List;

public partial class BaseListPart : BaseDisplayComponent, IAsyncDisposable
{
    #region Parameters

    #region Events
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

    [Parameter] public IBaseModel Model { get; set; } = null!;
    [Parameter] public PropertyInfo Property { get; set; } = null!;
    [Parameter] public IBaseDbContext DbContext { get; set; } = null!;
    [Parameter] public bool? ReadOnly { get; set; }
    [Parameter] public string? SingleDisplayName { get; set; }
    [Parameter] public string? PluralDisplayName { get; set; }

    [Parameter] public bool StickyRowButtons { get; set; } = true;
    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseListPart> Localizer { get; set; } = null!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = null!;
    #endregion

    #region Members
    protected Guid Id = Guid.NewGuid();
    protected EventServices EventServices = null!;

    protected IStringLocalizer ModelLocalizer { get; set; } = null!;
    protected Type IStringModelLocalizerType { get; set; } = null!;
    protected IList Entries { get; set; } = null!;
    protected Dictionary<object, bool> EntryIsInAddingMode { get; set; } = new();
    protected Type ModelListEntryType { get; set; } = null!;
    protected object? SelectedEntry = null;

    protected BaseListPartDisplayOptionsAttribute DisplayOptions { get; set; } = new();
    protected bool ModelImplementedISortableItem { get; set; }
    protected SortableItemComparer SortableItemComparer { get; set; } = new SortableItemComparer();

    protected RenderFragment? CardModalEdit { get; set; } = null;

    protected Virtualize<object>? VirtualizeList = null!;
    #region Property Infos

    protected bool IsReadOnly;
    protected List<BaseInput> BaseInputs = [];
    protected List<BaseSelectListInput> BaseSelectListInputs = [];
    protected List<BaseSelectListPopupInput> BaseSelectListPopupInputs = [];
    protected List<IBasePropertyListPartInput> BasePropertyListPartInputs = [];

    protected BaseInput AddToBaseInputs
    {
        set
        {
            BaseInputs.RemoveAll(entry => entry.Model == value.Model && entry.Property.Name == value.Property.Name);
            BaseInputs.Add(value);
        }
    }
    protected BaseSelectListInput AddToBaseInputSelectLists
    {
        set
        {
            BaseSelectListInputs.RemoveAll(entry => entry.Model == value.Model && entry.Property.Name == value.Property.Name);
            BaseSelectListInputs.Add(value);
        }
    }
    protected BaseSelectListPopupInput AddToBaseInputSelectListPopups
    {
        set
        {
            BaseSelectListPopupInputs.RemoveAll(entry => entry.Model == value.Model && entry.Property.Name == value.Property.Name);
            BaseSelectListPopupInputs.Add(value);
        }
    }

    protected List<IBasePropertyListPartInput> BaseInputExtensions = [];
    protected BaseTypeBasedSelectList BaseSelectList = null!;

    #endregion

    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        ModelListEntryType = Property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? Property.PropertyType.GenericTypeArguments[0];
        DisplayOptions = Property.GetCustomAttribute<BaseListPartDisplayOptionsAttribute>() ?? new BaseListPartDisplayOptionsAttribute();

        ModelImplementedISortableItem = ModelListEntryType.ImplementedISortableItem();

        IStringModelLocalizerType = typeof(IStringLocalizer<>).MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
        ModelLocalizer = StringLocalizerFactory.Create(Property.PropertyType.GenericTypeArguments[0]);
        EventServices = GetEventServices();

        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI(GUIType.ListPart, await BaseAuthenticationService.GetUserRolesAsync());
        else
            IsReadOnly = ReadOnly.Value;

        await SetUpDisplayListsAsync(ModelListEntryType, GUIType.ListPart);

        if (String.IsNullOrEmpty(SingleDisplayName))
            SingleDisplayName = ModelLocalizer[ModelListEntryType.Name];
        if (String.IsNullOrEmpty(PluralDisplayName))
            PluralDisplayName = ModelLocalizer[$"{ModelListEntryType.Name}_Plural"];

        if (Property.GetValue(Model) == null)
            Property.SetValue(Model, CreateGenericListInstance());

        BaseInputExtensions = ServiceProvider.GetServices<IBasePropertyListPartInput>().ToList();

        dynamic entries = Property.GetValue(Model)!;
        if (ModelImplementedISortableItem)
            entries.Sort(SortableItemComparer);
        Entries = (IList)entries;

        Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;
        Model.OnRecalculateCustomLookupData += async (sender, e) => await Model_OnRecalculateCustomLookupDataAsync(sender, e);

        await PrepareForeignKeyProperties(DbContext, GUIType.ListPart);
        await PrepareCustomLookupData(Model, EventServices);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender)
            return;

        await JSRuntime.InvokeVoidAsync("window.blazorBase.crud.inputVisibilityObserver.createObserver", Id.ToString(), Id.ToString());
    }

    public ValueTask DisposeAsync()
    {
        return JSRuntime.InvokeVoidAsync("window.blazorBase.crud.inputVisibilityObserver.deleteObserver", Id.ToString());
    }

    protected async override Task OnParametersSetAsync()
    {
        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI(GUIType.ListPart, await BaseAuthenticationService.GetUserRolesAsync());
        else
            IsReadOnly = ReadOnly.Value;
    }

    protected void Model_OnForcePropertyRepaint(object? sender, string[] propertyNames)
    {
        if (!propertyNames.Contains(Property.Name))
            return;

        InvokeAsync(StateHasChanged);
    }

    protected async Task Model_OnRecalculateCustomLookupDataAsync(object? sender, EventArgs e)
    {
        await PrepareCustomLookupData(Model, EventServices);
        _ = InvokeAsync(StateHasChanged);
    }

    protected async Task<RenderFragment?> CheckIfPropertyRenderingIsHandledAsync(DisplayItem displayItem, bool isReadonly, IBaseModel model)
    {
        foreach (var baseinput in BaseInputExtensions)
            if (await baseinput.IsHandlingPropertyRenderingAsync(model, displayItem, EventServices))
                return GetBaseInputExtensionAsRenderFragment(displayItem, isReadonly, baseinput.GetType(), model);

        return null;
    }

    protected virtual RenderFragment GetBaseInputExtensionAsRenderFragment(DisplayItem displayItem, bool isReadonly, Type baseInputExtensionType, IBaseModel model) => builder =>
    {
        builder.OpenComponent(0, baseInputExtensionType);

        builder.AddAttribute(1, "Model", model);
        builder.AddAttribute(2, "Property", displayItem.Property);
        builder.AddAttribute(3, "ReadOnly", isReadonly);
        builder.AddAttribute(4, "DbContext", DbContext);
        builder.AddAttribute(5, "ModelLocalizer", ModelLocalizer);
        builder.AddAttribute(6, "DisplayItem", displayItem);
        builder.AddAttribute(7, "OnBeforeConvertPropertyType", EventCallback.Factory.Create<OnBeforeConvertPropertyTypeArgs>(this, (args) => OnBeforeConvertListPropertyType.InvokeAsync(new OnBeforeConvertListPropertyTypeArgs(args.Model, args.PropertyName, args.NewValue, args.EventServices))));
        builder.AddAttribute(8, "OnBeforePropertyChanged", EventCallback.Factory.Create<OnBeforePropertyChangedArgs>(this, (args) => OnBeforeListPropertyChanged.InvokeAsync(new OnBeforeListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.EventServices))));
        builder.AddAttribute(9, "OnAfterPropertyChanged", EventCallback.Factory.Create<OnAfterPropertyChangedArgs>(this, (args) => OnAfterListPropertyChanged.InvokeAsync(new OnAfterListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.OldValue, args.IsValid, args.EventServices))));

        builder.AddComponentReferenceCapture(10, (input) =>
        {
            var listPartInput = (IBasePropertyListPartInput)input;
            BaseSelectListPopupInputs.RemoveAll(entry => entry.Model == listPartInput.Model && entry.Property.Name == listPartInput.Property.Name);
            BasePropertyListPartInputs.Add(listPartInput);
        });

        builder.CloseComponent();
    };

    #endregion

    #region CRUD

    protected virtual ValueTask<ItemsProviderResult<object>> LoadListDataProviderAsync(ItemsProviderRequest request)
    {
        if (request.Count == 0)
            return ValueTask.FromResult(new ItemsProviderResult<object>([], 0));

        var listEntries = new List<object>();
        for (int i = 0; i < Entries.Count; i++)
        {
            var entry = Entries[i];
            if (i < request.StartIndex || entry == null)
                continue;

            listEntries.Add(entry);

            if (listEntries.Count >= request.Count)
                break;
        }

        return ValueTask.FromResult(new ItemsProviderResult<object>(listEntries, Entries.Count));
    }

    public virtual Task OnRowSelected(object entry)
    {
        if (entry == SelectedEntry)
            SelectedEntry = null;
        else
            SelectedEntry = entry;

        return Task.CompletedTask;
    }

    protected object CreateGenericListInstance()
    {
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
        return Activator.CreateInstance(constructedListType)!;
    }

    protected async Task AddEntryAsync(object? aboveEntry = null)
    {
        var newEntry = Activator.CreateInstance(ModelListEntryType)!;
        if (newEntry is IModelInjectServiceProvider injectModel)
            injectModel.ServiceProvider = ServiceProvider;

        await OnCreateNewListEntryInstanceAsync(newEntry);

        var args = new HandledEventArgs();
        await OnBeforeAddEntryAsync(newEntry, args);
        if (args.Handled)
            return;

        if (ModelImplementedISortableItem && aboveEntry != null)
            Entries.Insert(Entries.IndexOf(aboveEntry), newEntry);
        else
            Entries.Add(newEntry);

        EntryIsInAddingMode[newEntry] = true;
        SetSortIndex();

        await OnAfterAddEntryAsync(newEntry);
        await RefreshListViewAsync();
    }

    protected Task AddExistingEntryAsync(object? aboveEntry = null)
    {
        BaseSelectList.ShowModal(aboveEntry);

        return Task.CompletedTask;
    }

    protected async Task AddExistingEntrySelectListClosedAsync(OnSelectListClosedArgs args)
    {
        if (args.SelectedModel == null)
            return;

        var entryToAdd = await DbContext.FindAsync(ModelListEntryType, args.SelectedModel.GetPrimaryKeys());
        if (entryToAdd == null)
            return;

        var handledEventArgs = new HandledEventArgs();
        await OnBeforeAddEntryAsync(entryToAdd, handledEventArgs, callAddEventOnListEntry: false);
        if (handledEventArgs.Handled)
            return;

        var aboveEntry = args.AdditionalData;
        if (ModelImplementedISortableItem && aboveEntry != null)
            Entries.Insert(Entries.IndexOf(aboveEntry), entryToAdd);
        else
            Entries.Add(entryToAdd);

        EntryIsInAddingMode[entryToAdd] = true;
        SetSortIndex();

        await OnAfterAddEntryAsync(entryToAdd, callAddEventOnListEntry: false);
        await RefreshListViewAsync();
    }

    protected async Task RemoveEntryAsync(object entry)
    {
        var args = new HandledEventArgs();
        await OnBeforeRemoveEntryAsync(entry, args);
        if (args.Handled)
            return;

        Entries.Remove(entry);
        BaseInputs.RemoveAll(input => input.Model == entry);
        BaseSelectListInputs.RemoveAll(input => input.Model == entry);
        BaseSelectListPopupInputs.RemoveAll(input => input.Model == entry);
        BasePropertyListPartInputs.RemoveAll(input => input.Model == entry);

        var entityEntry = await DbContext.EntryAsync(entry);
        if (entityEntry.State == EntityState.Added)
            entityEntry.State = EntityState.Detached;

        await OnAfterRemoveEntryAsync(entry);
        await RefreshListViewAsync();
    }

    protected async Task MoveEntryUpAsync(object entry)
    {
        if (!ModelImplementedISortableItem)
            return;

        var index = Entries.IndexOf(entry);
        if (index == 0)
            return;

        SwapEntries(entry, index, index - 1);
        SetSortIndex();

        await OnAfterMoveListEntryUpAsync(entry);
        await RefreshListViewAsync();
    }

    protected async Task MoveEntryDownAsync(object entry)
    {
        if (!ModelImplementedISortableItem)
            return;

        var index = Entries.IndexOf(entry);
        if (index == Entries.Count - 1)
            return;

        SwapEntries(entry, index, index + 1);
        SetSortIndex();

        await OnAfterMoveListEntryDownAsync(entry);
        await RefreshListViewAsync();
    }

    protected void SwapEntries(object entry, int currentIndex, int targetIndex)
    {
        var tempEntry = Entries[targetIndex];
        Entries[targetIndex] = entry;
        Entries[currentIndex] = tempEntry;
    }

    protected void SetSortIndex()
    {
        if (!ModelImplementedISortableItem)
            return;

        for (int index = 0; index < Entries.Count; index++)
        {
            var entry = Entries[index];
            if (entry != null)
                ((ISortableItem)entry).SortIndex = index;
        }
    }
    #endregion

    #region Card Modal Edit

    protected Task ShowCardEditModalAsync(object entry)
    {
        CardModalEdit = GetBaseModalCardAsRenderFragment(new Func<OnEntryToBeShownByStartArgs, Task<IBaseModel>>((OnEntryToBeShownByStartArgs args) => Task.FromResult((IBaseModel)entry)));
        InvokeAsync(StateHasChanged);

        return Task.CompletedTask;
    }

    protected virtual RenderFragment GetBaseModalCardAsRenderFragment(Func<OnEntryToBeShownByStartArgs, Task<IBaseModel>> entryToBeShown) => builder =>
    {
        builder.OpenComponent(0, typeof(BaseModalCard<>).MakeGenericType(Property.PropertyType.GenericTypeArguments[0]));
        builder.AddAttribute(1, "ShowEntryByStart", true);
        builder.AddAttribute(2, "EntryToBeShownByStart", entryToBeShown);
        builder.AddAttribute(3, "ParentDbContext", DbContext);
        builder.AddAttribute(4, "CloseButtonText", Localizer["Back"].ToString());
        builder.AddAttribute(5, "OnCardClosed", EventCallback.Factory.Create(this, (args) =>
        {
            CardModalEdit = null;
            InvokeAsync(StateHasChanged);
        }));

        builder.CloseComponent();
    };

    #endregion

    #region Events
    protected async Task OnCreateNewListEntryInstanceAsync(object newEntry)
    {
        var onCreateNewListEntryInstanceArgs = new OnCreateNewListEntryInstanceArgs(Model, newEntry, EventServices);
        await OnCreateNewListEntryInstance.InvokeAsync(onCreateNewListEntryInstanceArgs);
        await Model.OnCreateNewListEntryInstance(onCreateNewListEntryInstanceArgs);

        if (newEntry is not IBaseModel newBaseEntry)
            return;

        var onCreateNewEntryInstanceArgs = new OnCreateNewEntryInstanceArgs(Model, EventServices);
        await newBaseEntry.OnCreateNewEntryInstance(onCreateNewEntryInstanceArgs);
    }

    protected async Task OnBeforeAddEntryAsync(object? newEntry, HandledEventArgs args, bool callAddEventOnListEntry = true)
    {
        var onBeforeAddListEntryArgs = new OnBeforeAddListEntryArgs(Model, newEntry, false, EventServices);
        await OnBeforeAddListEntry.InvokeAsync(onBeforeAddListEntryArgs);
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
        await OnAfterAddListEntry.InvokeAsync(onAfterAddListEntryArgs);
        await Model.OnAfterAddListEntry(onAfterAddListEntryArgs);

        if (callAddEventOnListEntry && newEntry is IBaseModel addedBaseEntry)
            await addedBaseEntry.OnAfterAddEntry(new OnAfterAddEntryArgs(Model, EventServices));
    }

    protected async Task OnBeforeRemoveEntryAsync(object entry, HandledEventArgs args)
    {
        var onBeforeRemoveListEntry = new OnBeforeRemoveListEntryArgs(Model, entry, false, EventServices);
        await OnBeforeRemoveListEntry.InvokeAsync(onBeforeRemoveListEntry);
        await Model.OnBeforeRemoveListEntry(onBeforeRemoveListEntry);
        if (onBeforeRemoveListEntry.AbortRemoving)
        {
            args.Handled = true;
            return;
        }

        if (entry is IBaseModel newBaseEntry)
        {
            var onBeforeRemoveEntryFromListArgs = new OnBeforeRemoveEntryFromListArgs(Model, false, EventServices);
            await newBaseEntry.OnBeforeRemoveEntryFromList(onBeforeRemoveEntryFromListArgs);
            if (onBeforeRemoveEntryFromListArgs.AbortRemoving)
            {
                args.Handled = true;
                return;
            }
        }
    }

    protected async Task OnAfterRemoveEntryAsync(object entry)
    {
        var onAfterRemoveListEntryArgs = new OnAfterRemoveListEntryArgs(Model, entry, EventServices);
        await OnAfterRemoveListEntry.InvokeAsync(onAfterRemoveListEntryArgs);
        await Model.OnAfterRemoveListEntry(onAfterRemoveListEntryArgs);

        if (entry is IBaseModel RemovedBaseEntry)
            await RemovedBaseEntry.OnAfterRemoveEntryFromList(new OnAfterRemoveEntryFromListArgs(Model, EventServices));
    }

    protected async Task OnAfterMoveListEntryUpAsync(object entry)
    {
        var args = new OnAfterMoveListEntryUpArgs(Model, entry, EventServices);
        await OnAfterMoveListEntryUp.InvokeAsync(args);
        await Model.OnAfterMoveListEntryUp(args);

        if (entry is IBaseModel baseModel)
            await baseModel.OnAfterMoveEntryUp(new OnAfterMoveEntryUpArgs(Model, EventServices));
    }

    protected async Task OnAfterMoveListEntryDownAsync(object entry)
    {
        var args = new OnAfterMoveListEntryDownArgs(Model, entry, EventServices);
        await OnAfterMoveListEntryDown.InvokeAsync(args);
        await Model.OnAfterMoveListEntryDown(args);

        if (entry is IBaseModel baseModel)
            await baseModel.OnAfterMoveEntryDown(new OnAfterMoveEntryDownArgs(Model, EventServices));
    }

    protected async Task OnBeforeListPropertyChangedAsync(OnBeforePropertyChangedArgs args)
    {
        var listArgs = new OnBeforeListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.OldValue, args.EventServices);
        await OnBeforeListPropertyChanged.InvokeAsync(listArgs);
        await Model.OnBeforeListPropertyChanged(listArgs);
    }

    protected async Task OnAfterListPropertyChangedAsync(OnAfterPropertyChangedArgs args)
    {
        var listArgs = new OnAfterListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.OldValue, args.IsValid, args.EventServices);
        await OnAfterListPropertyChanged.InvokeAsync(listArgs);
        await Model.OnAfterListPropertyChanged(listArgs);
    }

    #endregion

    #region Parent Events

    public void OnAfterCardSaveChanges()
    {
        foreach (var entry in Entries)
            EntryIsInAddingMode[entry] = false;
    }

    #endregion

    #region Validation
    public virtual async Task<bool> ListPartIsValidAsync()
    {
        var valid = true;

        foreach (var input in BaseInputs)
            if (!await input.ValidatePropertyValueAsync())
                valid = false;

        foreach (var input in BaseSelectListInputs)
            if (!await input.ValidatePropertyValueAsync())
                valid = false;

        foreach (var input in BaseSelectListPopupInputs)
            if (!await input.ValidatePropertyValueAsync())
                valid = false;

        foreach (var basePropertyListPartInput in BasePropertyListPartInputs)
            if (!await basePropertyListPartInput.ValidatePropertyValueAsync())
                valid = false;

        foreach (var item in Entries)
        {
            if (item is IBaseModel baseModel)
            {
                var validationContext = new ValidationContext(item, ServiceProvider, new Dictionary<object, object?>()
                {
                    [typeof(IStringLocalizer)] = ModelLocalizer,
                    [typeof(IBaseDbContext)] = DbContext
                });

                if (!baseModel.TryValidate(out List<ValidationResult> validationResults, validationContext))
                    valid = false;
            }
        }

        return valid;
    }
    #endregion

    #region MISC

    protected EventServices GetEventServices()
    {
        return new EventServices(ServiceProvider, DbContext, ModelLocalizer);
    }

    public bool CheckIfModelIsInAddingMode(object entry)
    {
        if (EntryIsInAddingMode.TryGetValue(entry, out bool isInAddingMode))
            return isInAddingMode;

        return false;
    }

    public virtual Task RefreshListViewAsync()
    {
        if (VirtualizeList == null)
            return Task.CompletedTask;

        return VirtualizeList.RefreshDataAsync();
    }
    #endregion
}
