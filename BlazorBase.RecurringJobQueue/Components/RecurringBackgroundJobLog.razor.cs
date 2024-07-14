﻿using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using BlazorBase.CRUD.Models;
using BlazorBase.RecurringBackgroundJobQueue.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace BlazorBase.RecurringBackgroundJobQueue.Components;

public partial class RecurringBackgroundJobLog : ComponentBase, IBasePropertyCardInput
{
    #region Parameters

    [Parameter] public IBaseModel Model { get; set; } = null!;
    [Parameter] public PropertyInfo Property { get; set; } = null!;
    [Parameter] public bool? ReadOnly { get; set; }
    [Parameter] public IBaseDbContext DbContext { get; set; } = null!;
    [Parameter] public IStringLocalizer ModelLocalizer { get; set; } = null!;
    [Parameter] public IDisplayItem DisplayItem { get; set; } = null!;

    #region Events

    [Parameter] public EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
    [Parameter] public EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
    [Parameter] public EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }

    #endregion

    #endregion

    #region Interface Methods

    public Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, IDisplayItem displayItem, EventServices eventServices)
    {
        return Task.FromResult(displayItem.Property.DeclaringType == typeof(RecurringBackgroundJobEntry) && (displayItem.Property.Name == nameof(RecurringBackgroundJobEntry.Log) || displayItem.Property.Name == nameof(RecurringBackgroundJobEntry.LastErrors)));
    }

    public Task<bool> ValidatePropertyValueAsync(bool calledFromOnValueChangedAsync = false) => Task.FromResult(true);
    public void SetValidation(bool showValidation, bool isValid, string feedback) { }

    public Task<bool> InputHasAdditionalContentChanges() { return Task.FromResult(false); }

    public Task OnBeforeCardSaveChanges(OnBeforeCardSaveChangesArgs args) { return Task.CompletedTask; }

    public Task OnAfterCardSaveChanges(OnAfterCardSaveChangesArgs args) { return Task.CompletedTask; }

    #endregion

}
