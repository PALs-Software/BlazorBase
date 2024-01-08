using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.Files.Attributes;
using BlazorBase.Files.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.Files.Components;

public partial class BaseFileListDisplay : ComponentBase, IBasePropertyListDisplay
{
    #region Parameters
    [Parameter] public IBaseModel Model { get; set; } = null!;
    [Parameter] public PropertyInfo Property { get; set; } = null!;
    [Parameter] public BaseService Service { get; set; } = null!;
    [Parameter] public IStringLocalizer ModelLocalizer { get; set; } = null!;
    [Parameter] public DisplayItem DisplayItem { get; set; } = null!;
    [Parameter] public GUIType IsDisplayedInGuiType { get; set; }

    #endregion

    #region Injects
    [Inject] protected IStringLocalizer<BaseFileListDisplay> Localizer { get; set; } = null!;
    #endregion

    #region Member
    protected bool DisplayShowFileButton;
    protected bool DisplayDownloadFileButton;
    protected BaseFileModal? BaseFileModal = null;
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        DisplayShowFileButton = Property.GetCustomAttribute(typeof(HideShowFileButtonAttribute)) is not HideShowFileButtonAttribute hideShowButtonAttr ||
                                    !hideShowButtonAttr.HideInGUITypes.Contains(IsDisplayedInGuiType);
        DisplayDownloadFileButton = Property.GetCustomAttribute(typeof(HideDownloadFileButtonAttribute)) is not HideDownloadFileButtonAttribute hideDownloadButtonAttr ||
                                     !hideDownloadButtonAttr.HideInGUITypes.Contains(IsDisplayedInGuiType);
    }

    public Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices)
    {
        return Task.FromResult(typeof(IBaseFile).IsAssignableFrom(displayItem.Property.PropertyType));
    }
    #endregion
}
