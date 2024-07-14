using BlazorBase.Abstractions.CRUD.Interfaces;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.Card;

public interface IBaseCard
{
    public IBaseModel? CurrentBaseModelInstance { get; }

    Task ShowAsync(bool addingMode, bool viewMode, object?[]? primaryKeys = null, IBaseModel? template = null);
    Task ReloadEntityFromDatabase();
    Task<bool> SaveCardAsync(bool showSnackBar = true);
    void ResetCard();
    IBaseModel GetCurrentModel();
    Task StateHasChangedAsync();
    Task<bool> HasUnsavedChangesAsync();
    bool CardIsInAddingMode();
    bool CardIsInViewMode();
}
