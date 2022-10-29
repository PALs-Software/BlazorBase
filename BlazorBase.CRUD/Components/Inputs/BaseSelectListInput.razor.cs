using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Blazorise;
using BlazorBase.CRUD.Models;
using System.Threading.Tasks;
using Blazorise.Components;

namespace BlazorBase.CRUD.Components.Inputs
{
    public partial class BaseSelectListInput : BaseInput
    {
        [Parameter] public List<KeyValuePair<string, string>> Data { get; set; }

        protected SelectList<KeyValuePair<string, string>, string> SelectList = default;
    }
}
