using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.CRUD.Components.List;

public partial class BaseMemoryList<TModel> : BaseGenericList<TModel> where TModel : class, IBaseModel, new()
{
    #region Parameters
    [Parameter] public List<TModel> Models { get; set; } = new();
    #endregion

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
