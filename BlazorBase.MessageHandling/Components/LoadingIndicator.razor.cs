using Microsoft.AspNetCore.Components;

namespace BlazorBase.MessageHandling.Components;

public partial class LoadingIndicator
{
    [Parameter] public RenderFragment? ChildContent { get; set; } = null;
    [Parameter] public bool Visible { get; set; }
}
