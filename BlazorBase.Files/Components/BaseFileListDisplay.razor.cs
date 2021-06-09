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
using static BlazorBase.CRUD.Components.BaseDisplayComponent;

namespace BlazorBase.Files.Components
{
    public partial class BaseFileListDisplay : ComponentBase, IBasePropertyListDisplay
    {
        #region Parameters
        [Parameter] public IBaseModel Model { get; set; }
        [Parameter] public PropertyInfo Property { get; set; }
        [Parameter] public BaseService Service { get; set; }
        [Parameter] public IStringLocalizer ModelLocalizer { get; set; }
        [Parameter] public GUIType IsDisplayedInGuiType { get; set; }
        #endregion

        #region Injects
        [Inject] protected IStringLocalizer<BaseFileListDisplay> Localizer { get; set; }
        #endregion

        #region Member
        protected bool DisplayShowFileButton;
        protected bool DisplayDownloadFileButton;
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
            return Task.FromResult(typeof(BaseFile).IsAssignableFrom(displayItem.Property.PropertyType));
        }
        #endregion
    }
}
