﻿using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Structures;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace BlazorBase.Abstractions.CRUD.Interfaces;

public interface IBasePropertyListPartInput
{
    #region Parameters
#pragma warning disable BL0007 // Component parameters should be auto properties

    [Parameter] IBaseModel Model { get; set; }

    [Parameter] PropertyInfo Property { get; set; }
    [Parameter] bool? ReadOnly { get; set; }
    [Parameter] IBaseDbContext DbContext { get; set; }
    [Parameter] IStringLocalizer ModelLocalizer { get; set; }
    [Parameter] IDisplayItem DisplayItem { get; set; }

    #region Events
    [Parameter] EventCallback<OnBeforeConvertPropertyTypeArgs> OnBeforeConvertPropertyType { get; set; }
    [Parameter] EventCallback<OnBeforePropertyChangedArgs> OnBeforePropertyChanged { get; set; }
    [Parameter] EventCallback<OnAfterPropertyChangedArgs> OnAfterPropertyChanged { get; set; }
    #endregion

#pragma warning restore BL0007 // Component parameters should be auto properties
    #endregion

    Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, IDisplayItem displayItem, EventServices eventServices);

    Task<bool> ValidatePropertyValueAsync(bool calledFromOnValueChangedAsync = false);

    void SetValidation(bool showValidation, bool isValid, string feedback);
}
