using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Files.Components
{
    public partial class LoadingIndicator
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        [Parameter] public bool Visible { get; set; }
    }
}
