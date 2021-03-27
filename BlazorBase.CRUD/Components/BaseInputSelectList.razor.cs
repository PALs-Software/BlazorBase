using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Blazorise;
using BlazorBase.CRUD.Models;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseInputSelectList : BaseInput
    {
        [Parameter] public List<KeyValuePair<string, string>> Data { get; set; }

        protected Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
           
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await InvokeAsync(() =>
            {
                if (IsReadOnly)
                    Attributes.Add("disabled", "disabled");
            });
        }

    }
}
