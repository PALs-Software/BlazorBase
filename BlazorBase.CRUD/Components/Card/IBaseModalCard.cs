using BlazorBase.Abstractions.CRUD.Interfaces;
using Blazorise;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.Card;

public interface IBaseModalCard
{
    Task ShowModalAsync(bool addingMode = false, bool viewMode = false, object?[]? primaryKeys = null, IBaseModel? template = null);
    Task ReloadEntityFromDatabase();
    Task<bool> SaveModalAsync();
    Task SaveAndCloseModalAsync();
    void HideModal();
    Task OnModalClosing(ModalClosingEventArgs args);
    bool? CardIsInAddingMode();
    bool? CardIsInViewMode();
}
