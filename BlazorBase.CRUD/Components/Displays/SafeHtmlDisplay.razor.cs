using BlazorBase.Modules;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.CRUD.Components.Displays;

public partial class SafeHtmlDisplay
{
    #region Parameters
    [Parameter] public string? Html { get; set; } = null!;
    #endregion

    #region Members
    protected MarkupString? SafeHtml;
    #endregion

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        SafeHtml = BaseMarkupStringValidator.GetWhiteListedMarkupString(Html);
    }
}
