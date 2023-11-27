using BlazorBase.CRUD.Models;

namespace BlazorBase.CRUD.Components.SelectList;

public interface IBaseSelectList
{
    void ShowModal(object? additionalData = null);
    void HideModal();
    IBaseModel? GetSelectedEntry();
}
