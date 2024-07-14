using BlazorBase.Abstractions.CRUD.Interfaces;
using BlazorBase.Modules;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components.List;

public partial class BaseMemoryList<TModel> : BaseGenericList<TModel> where TModel : class, IBaseModel, new()
{
    #region Parameters
    [Parameter] public BaseObservableCollection<TModel> Models { get; set; } = [];
    #endregion

    #region Members
    protected BaseObservableCollection<TModel> ModelCollection = [];
    #endregion

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ModelCollection == Models)
            return;

        ModelCollection.CollectionChanged -= ModelCollection_CollectionChanged;
        ModelCollection = Models ?? [];
        ModelCollection.CollectionChanged += ModelCollection_CollectionChanged;
    }

    private void ModelCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        var action = e.Action;
        
        InvokeAsync(async () =>
        {
            if (VirtualizeList == null)
                return;

            await RefreshDataAsync();
            StateHasChanged();
        });
    }

    #region Data Loading

    protected override ValueTask<ItemsProviderResult<TModel>> LoadListDataProviderAsync(ItemsProviderRequest request)
    {
        if (request.Count == 0)
            return ValueTask.FromResult(new ItemsProviderResult<TModel>(new List<TModel>(), 0));

        var query = CreateLoadDataQuery(Models.AsQueryable(), useEFFilters: false);
        var allEntries = query.ToList();
        var totalEntries = allEntries.Count;
        Entries = allEntries.Skip(request.StartIndex).Take(request.Count).ToList();

        return ValueTask.FromResult(new ItemsProviderResult<TModel>(Entries, totalEntries));
    }

    #endregion              
}
