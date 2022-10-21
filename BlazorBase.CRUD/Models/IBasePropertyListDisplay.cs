using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Reflection;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Components.General.BaseDisplayComponent;

namespace BlazorBase.CRUD.Models
{
    public interface IBasePropertyListDisplay
    {
        #region Parameters

        [Parameter] IBaseModel Model { get; set; }
        [Parameter] PropertyInfo Property { get; set; }
        [Parameter] BaseService Service { get; set; }
        [Parameter] IStringLocalizer ModelLocalizer { get; set; }

        #endregion

        Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices);
    }
}
