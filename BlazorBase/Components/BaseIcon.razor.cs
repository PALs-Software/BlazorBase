using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Components;

public partial class BaseIcon : BaseComponent
{
    [Parameter] public object IconName { get; set; }

    [Parameter] public IconStyle IconStyle { get; set; }
}
