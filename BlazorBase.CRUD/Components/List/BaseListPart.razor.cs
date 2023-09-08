using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.SelectList;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.SortableItem;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.CRUD.Components.Inputs;
using static BlazorBase.CRUD.Components.SelectList.BaseTypeBasedSelectList;
using Microsoft.AspNetCore.Components;
using System;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.Extensions.Localization;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using BlazorBase.CRUD.ModelServiceProviderInjection;

namespace BlazorBase.CRUD.Components.List;

public partial class BaseListPart : BaseDisplayComponent
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
    [Parameter] public BaseService Service { get; set; } = null!;
    [Parameter] public bool? ReadOnly { get; set; }
    [Parameter] public string? SingleDisplayName { get; set; }
    [Parameter] public string? PluralDisplayName { get; set; }

    [Parameter] public bool StickyRowButtons { get; set; } = true;
    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseListPart> Localizer { get; set; } = null!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = null!;
    [Inject] protected IMessageHandler MessageHandler { get; set; } = null!;
    #endregion

    #region Members
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

    #region Property Infos

    protected bool IsReadOnly;
    protected List<BaseInput> BaseInputs = new List<BaseInput>();
    protected List<BaseSelectListInput> BaseSelectListInputs = new List<BaseSelectListInput>();
    protected List<IBasePropertyListPartInput> BasePropertyListPartInputs = new List<IBasePropertyListPartInput>();

    protected BaseInput AddToBaseInputs { set { BaseInputs.Add(value); } }
    protected BaseSelectListInput AddToBaseInputSelectLists { set { BaseSelectListInputs.Add(value); } }
    protected List<IBasePropertyListPartInput> BaseInputExtensions = new List<IBasePropertyListPartInput>();

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
            IsReadOnly = Property.IsReadOnlyInGUI();
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

        await PrepareForeignKeyProperties(Service);
        await PrepareCustomLookupData(Model, EventServices);
    }

    protected override void OnParametersSet()
    {
        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI();
        else
            IsReadOnly = ReadOnly.Value;
    }

    private void Model_OnForcePropertyRepaint(object? sender, string[] propertyNames)
    {
        if (!propertyNames.Contains(Property.Name))
            return;

        InvokeAsync(() => StateHasChanged());
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
        builder.AddAttribute(4, "Service", Service);
        builder.AddAttribute(5, "ModelLocalizer", ModelLocalizer);
        builder.AddAttribute(6, "DisplayItem", displayItem);
        builder.AddAttribute(7, "OnBeforeConvertPropertyType", EventCallback.Factory.Create<OnBeforeConvertPropertyTypeArgs>(this, (args) => OnBeforeConvertListPropertyType.InvokeAsync(new OnBeforeConvertListPropertyTypeArgs(args.Model, args.PropertyName, args.NewValue, args.EventServices))));
        builder.AddAttribute(8, "OnBeforePropertyChanged", EventCallback.Factory.Create<OnBeforePropertyChangedArgs>(this, (args) => OnBeforeListPropertyChanged.InvokeAsync(new OnBeforeListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.EventServices))));
        builder.AddAttribute(9, "OnAfterPropertyChanged", EventCallback.Factory.Create<OnAfterPropertyChangedArgs>(this, (args) => OnAfterListPropertyChanged.InvokeAsync(new OnAfterListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.OldValue, args.IsValid, args.EventServices))));

        builder.AddComponentReferenceCapture(10, (input) => BasePropertyListPartInputs.Add((IBasePropertyListPartInput)input));

        builder.CloseComponent();
    };

    #endregion

    #region CRUD

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
        if (newEntry is IModeInjectServiceProvider injectModel)
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

        var entryToAdd = await Service.GetAsync(ModelListEntryType, args.SelectedModel.GetPrimaryKeys());
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
        BasePropertyListPartInputs.RemoveAll(input => input.Model == entry);

        var entityEntry = Service.DbContext.Entry(entry);
        if (entityEntry.State == EntityState.Added)
            entityEntry.State = EntityState.Detached;

        await OnAfterRemoveEntryAsync(entry);
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
            var onBeforeRemoveEntryArgs = new OnBeforeRemoveEntryArgs(Model, false, EventServices);
            await newBaseEntry.OnBeforeRemoveEntry(onBeforeRemoveEntryArgs);
            if (onBeforeRemoveEntryArgs.AbortRemoving)
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
            await RemovedBaseEntry.OnAfterRemoveEntry(new OnAfterRemoveEntryArgs(Model, EventServices));
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
                    [typeof(BaseService)] = Service
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
        return new EventServices(ServiceProvider, ModelLocalizer, Service, MessageHandler);
    }

    public bool CheckIfModelIsInAddingMode(object entry)
    {
        if (EntryIsInAddingMode.TryGetValue(entry, out bool isInAddingMode))
            return isInAddingMode;

        return false;
    }
    #endregion
}
