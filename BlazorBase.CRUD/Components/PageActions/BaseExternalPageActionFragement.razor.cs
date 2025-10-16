using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.PageActions;

public partial class BaseExternalPageActionFragement
{
    #region Members
    protected RenderFragment? ExternalPageActionFragement { get; set; } = null;
    #endregion

    public Task SetPageActionFragementAsync(RenderFragment? newPageActionFragement)
    {
        ExternalPageActionFragement = newPageActionFragement;

        return InvokeAsync(StateHasChanged);
    }
}
