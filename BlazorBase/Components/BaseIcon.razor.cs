using Blazorise;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.Components;

public partial class BaseIcon : BaseComponent
{
    [Parameter] public object? IconName { get; set; }

    [Parameter] public IconStyle IconStyle { get; set; }
}
