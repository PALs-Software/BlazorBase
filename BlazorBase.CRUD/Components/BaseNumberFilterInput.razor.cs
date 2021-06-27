using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseNumberFilterInput
    {
        #region Parameter
        [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }
        [Parameter] public string CultureName { get; set; } = CultureInfo.CurrentUICulture.Name;
        [Parameter] public string Value { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> AdditionalInputAttributes { get; set; }
        #endregion
    }
}
