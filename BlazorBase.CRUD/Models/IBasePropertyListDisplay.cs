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
#pragma warning disable BL0007 // Component parameters should be auto properties

        [Parameter] IBaseModel Model { get; set; }
        [Parameter] PropertyInfo Property { get; set; }
        [Parameter] BaseService Service { get; set; }
        [Parameter] IStringLocalizer ModelLocalizer { get; set; }
        [Parameter] DisplayItem DisplayItem { get; set; }

#pragma warning restore BL0007 // Component parameters should be auto properties
        #endregion

        Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, DisplayItem displayItem, EventServices eventServices);
    }
}
