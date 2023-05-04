using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Globalization;

namespace BlazorBase.CRUD.Components.Inputs;

public partial class BaseNumberFilterInput
{
    #region Parameter
    [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }
    [Parameter] public string? CultureName { get; set; } = CultureInfo.CurrentUICulture.Name;
    [Parameter] public string? Value { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalInputAttributes { get; set; } = new();
    #endregion
}
