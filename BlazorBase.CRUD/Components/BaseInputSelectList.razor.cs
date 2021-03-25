using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Blazorise;
using BlazorBase.CRUD.Models;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseInputSelectList<TModel> : BaseInput<TModel> where TModel : IBaseModel
    {
        [Parameter]
        public Dictionary<string, string> Data { get; set; }

        protected override bool UseGenericNullString { get; set; } = true;

        protected override string GetInputType()
        {
            return string.Empty; // Is not needed for select list
        }

    }
}
