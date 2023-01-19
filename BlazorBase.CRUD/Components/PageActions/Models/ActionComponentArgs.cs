#nullable enable

using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;

namespace BlazorBase.CRUD.Components.PageActions.Models
{
    public record ActionComponentArgs(object? Source, EventServices EventServices, IBaseModel? BaseModel);
}
