using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Extensions;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Inputs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.Abstractions.CRUD.Arguments.BasePrimitiveInputArgs;

namespace BlazorBase.CRUD.Components.List;

public partial class BasePrimitiveListPart : BaseDisplayComponent
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
    [Parameter] public string? Caption { get; set; } = null!;
    [Parameter] public IBaseDbContext DbContext { get; set; } = null!;
    [Parameter] public bool? ReadOnly { get; set; }

    [Parameter] public bool StickyRowButtons { get; set; } = true;
    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseListPart> Localizer { get; set; } = null!;
    #endregion

    #region Members
    protected EventServices EventServices = null!;

    protected IStringLocalizer ModelLocalizer { get; set; } = null!;
    protected Type IStringModelLocalizerType { get; set; } = null!;
    protected IList Entries { get; set; } = null!;
    protected Type ModelListEntryType { get; set; } = null!;
    protected int? SelectedEntryIndex = null;

    protected BaseListPartDisplayOptionsAttribute DisplayOptions { get; set; } = new();

    #region Property Infos

    protected bool IsReadOnly;
    protected List<BasePrimitiveInput> BaseInputs = [];

    protected BasePrimitiveInput AddToBaseInputs { set { BaseInputs.Add(value); } }

    #endregion

    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        ModelListEntryType = Property.GetCustomAttribute<RenderTypeAttribute>()?.RenderType ?? Property.PropertyType.GenericTypeArguments[0];
        DisplayOptions = Property.GetCustomAttribute<BaseListPartDisplayOptionsAttribute>() ?? new BaseListPartDisplayOptionsAttribute();

        var modelListEntryIsPrimitiveType = ModelListEntryType.IsPrimitive || ModelListEntryType == typeof(string) || ModelListEntryType == typeof(decimal);
        if (!modelListEntryIsPrimitiveType)
            throw new Exception("Only primitive types are allowed in BasePrimitiveListPart");

        IStringModelLocalizerType = typeof(IStringLocalizer<>).MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
        ModelLocalizer = StringLocalizerFactory.Create(Property.PropertyType.GenericTypeArguments[0]);
        EventServices = GetEventServices();

        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI(GUIType.ListPart, await BaseAuthenticationService.GetUserRolesAsync());
        else
            IsReadOnly = ReadOnly.Value;

        if (Property.GetValue(Model) == null)
            Property.SetValue(Model, CreateGenericListInstance());
        Entries = (IList)Property.GetValue(Model)!;

        Model.OnForcePropertyRepaint += Model_OnForcePropertyRepaint;
    }

    protected async override Task OnParametersSetAsync()
    {
        if (ReadOnly == null)
            IsReadOnly = Property.IsReadOnlyInGUI(GUIType.ListPart, await BaseAuthenticationService.GetUserRolesAsync());
        else
            IsReadOnly = ReadOnly.Value;
    }

    private void Model_OnForcePropertyRepaint(object? sender, string[] propertyNames)
    {
        if (!propertyNames.Contains(Property.Name))
            return;

        InvokeAsync(StateHasChanged);
    }

    #endregion

    #region CRUD

    protected async Task OnValueChangedAsync(OnValueChangedArgs args, int index)
    {
        args.NewValue = await OnBeforeListPropertyChangedAsync(new OnBeforePropertyChangedArgs(Model, Property.Name, args.NewValue, args.OldValue, EventServices));

        Entries[index] = args.NewValue;

        await OnAfterListPropertyChangedAsync(new OnAfterPropertyChangedArgs(Model, Property.Name, args.NewValue, args.OldValue, args.IsValid, EventServices));
    }

    public virtual void OnRowSelected(int index)
    {
        if (index == SelectedEntryIndex)
            SelectedEntryIndex = null;
        else
            SelectedEntryIndex = index;
    }

    protected object CreateGenericListInstance()
    {
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(Property.PropertyType.GenericTypeArguments[0]);
        return Activator.CreateInstance(constructedListType)!;
    }

    protected async Task AddEntryAsync(int? aboveEntryIndex = null)
    {
        object? newEntry;
        if (ModelListEntryType == typeof(string))
            newEntry = String.Empty;
        else
            newEntry = Activator.CreateInstance(ModelListEntryType)!;

        await OnCreateNewListEntryInstanceAsync(newEntry);

        var args = new HandledEventArgs();
        await OnBeforeAddEntryAsync(newEntry, args);
        if (args.Handled)
            return;

        if (aboveEntryIndex != null)
            Entries.Insert(aboveEntryIndex.Value, newEntry);
        else
            Entries.Add(newEntry);

        await OnAfterAddEntryAsync(newEntry);
    }

    protected async Task RemoveEntryAsync(int index)
    {
        var entry = Entries[index]!;
        var args = new HandledEventArgs();
        await OnBeforeRemoveEntryAsync(entry, args);
        if (args.Handled)
            return;

        Entries.RemoveAt(index);
        BaseInputs.RemoveAt(index);

        await OnAfterRemoveEntryAsync(entry);
    }

    protected async Task MoveEntryUpAsync(int index)
    {
        if (index == 0)
            return;

        SwapEntries(index, index - 1);

        await OnAfterMoveListEntryUpAsync(Entries[index - 1]!);
    }

    protected async Task MoveEntryDownAsync(int index)
    {
        if (index == Entries.Count - 1)
            return;

        SwapEntries(index, index + 1);

        await OnAfterMoveListEntryDownAsync(Entries[index + 1]!);
    }

    protected void SwapEntries(int currentIndex, int targetIndex)
    {
        var tempEntry = Entries[targetIndex];
        Entries[targetIndex] = Entries[currentIndex];
        Entries[currentIndex] = tempEntry;
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
    }

    protected async Task OnAfterMoveListEntryDownAsync(object entry)
    {
        var args = new OnAfterMoveListEntryDownArgs(Model, entry, EventServices);
        await OnAfterMoveListEntryDown.InvokeAsync(args);
        await Model.OnAfterMoveListEntryDown(args);
    }

    protected async Task<object?> OnBeforeListPropertyChangedAsync(OnBeforePropertyChangedArgs args)
    {
        var listArgs = new OnBeforeListPropertyChangedArgs(args.Model, args.PropertyName, args.NewValue, args.OldValue, args.EventServices);
        await OnBeforeListPropertyChanged.InvokeAsync(listArgs);
        await Model.OnBeforeListPropertyChanged(listArgs);

        return listArgs.NewValue;
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
    }

    #endregion

    #region Validation
    public virtual async Task<bool> ListPartIsValidAsync()
    {
        var valid = true;

        foreach (var input in BaseInputs)
            if (!await input.ValidatePropertyValueAsync())
                valid = false;

        return valid;
    }
    #endregion

    #region MISC
    protected EventServices GetEventServices()
    {
        return new EventServices(ServiceProvider, DbContext, ModelLocalizer);
    }
    #endregion
}
