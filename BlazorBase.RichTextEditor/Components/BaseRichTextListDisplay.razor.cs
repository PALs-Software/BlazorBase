using BlazorBase.Abstractions.CRUD.Enums;
using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Abstractions.CRUD.Structures;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorBase.RichTextEditor.Components;

public partial class BaseRichTextListDisplay : ComponentBase, IBasePropertyListDisplay
{
    #region Parameters
    [Parameter] public IBaseModel Model { get; set; } = null!;
    [Parameter] public PropertyInfo Property { get; set; } = null!;
    [Parameter] public IBaseDbContext DbContext { get; set; } = null!;
    [Parameter] public IStringLocalizer ModelLocalizer { get; set; } = null!;
    [Parameter] public IDisplayItem DisplayItem { get; set; } = null!;

    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    public Task<bool> IsHandlingPropertyRenderingAsync(IBaseModel model, IDisplayItem displayItem, EventServices eventServices)
    {
        var presentationDataType = displayItem.Property.GetCustomAttribute<DataTypeAttribute>()?.DataType;

        return Task.FromResult(presentationDataType == DataType.Html);
    }
    #endregion
}
