using BlazorBase.Abstractions.CRUD.Interfaces;

namespace BlazorBase.CRUD.Components.SelectList;

public interface IBaseSelectList
{
    void ShowModal(object? additionalData = null);
    void HideModal();
    IBaseModel? GetSelectedEntry();
}
