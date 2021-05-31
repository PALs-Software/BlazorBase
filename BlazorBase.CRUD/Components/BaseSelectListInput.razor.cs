using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Blazorise;
using BlazorBase.CRUD.Models;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseSelectListInput : BaseInput
    {
        [Parameter] public List<KeyValuePair<string, string>> Data { get; set; }
    }
}
