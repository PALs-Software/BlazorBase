using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Blazorise;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseInputSelectList : BaseInput
    {
        [Parameter]
        public Dictionary<string, string> Data { get; set; }

        protected override bool UseGenericNullString { get; set; } = true;
    }
}
