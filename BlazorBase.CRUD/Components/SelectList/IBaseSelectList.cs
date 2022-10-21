using BlazorBase.CRUD.Models;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.SelectList;

public interface IBaseSelectList
{
    Task ShowModalAsync();
    void HideModal();
    IBaseModel GetSelectedEntry();
}
