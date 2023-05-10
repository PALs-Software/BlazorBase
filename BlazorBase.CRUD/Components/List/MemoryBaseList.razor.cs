using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using BlazorBase.CRUD.EventArguments;
using Microsoft.AspNetCore.WebUtilities;
using BlazorBase.MessageHandling.Enum;
using Blazorise;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using BlazorBase.CRUD.Components.General;
using BlazorBase.CRUD.Components.Card;
using BlazorBase.Models;
using Newtonsoft.Json;
using BlazorBase.CRUD.Components.PageActions.Models;

namespace BlazorBase.CRUD.Components.List
{
    public partial class MemoryBaseList<TModel> : BaseGenericList<TModel>, IDisposable where TModel : class, IBaseModel, new()
    {
        #region Parameters

        #endregion
            
        #region Injects

        #endregion

        #region Members
        #endregion

        #region Init

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        #endregion

        #region Data Loading

        protected virtual async ValueTask<ItemsProviderResult<TModel>> LoadListDataProviderAsync(ItemsProviderRequest request)
        {
            if (request.Count == 0)
                return new ItemsProviderResult<TModel>(new List<TModel>(), 0);

            var query = CreateLoadDataQuery();

            var totalEntries = await query.CountAsync();
            Entries = await query.Skip(request.StartIndex).Take(request.Count).ToListAsync();

            return new ItemsProviderResult<TModel>(Entries, totalEntries);
        }

        protected virtual IQueryable<TModel> CreateLoadDataQuery()
        {
            var baseService = ServiceProvider.GetService<BaseService>(); //Use own service for each call, because then the queries can run parallel, because this method get called multiple times at the same time

            var query = baseService.Set<TModel>();
            foreach (var sortedColumn in SortedColumns)
                foreach (var displayProperty in sortedColumn.DisplayPropertyPath.Split("|"))
                {
                    if (sortedColumn.SortDirection == Enums.SortDirection.Ascending)
                        query = query is IOrderedQueryable<TModel> orderedQuery ? orderedQuery.ThenBy(displayProperty) : query.OrderBy(displayProperty);
                    else
                        query = query is IOrderedQueryable<TModel> orderedQuery ? orderedQuery.ThenByDescending(displayProperty) : query.OrderByDescending(displayProperty);
                }

            if (DataLoadConditions != null)
                foreach (var dataLoadCondition in DataLoadConditions)
                    if (dataLoadCondition != null)
                        query = query.Where(dataLoadCondition).Cast<TModel>();

            foreach (var group in DisplayGroups)
                foreach (var displayItem in group.Value.DisplayItems)
                    query = query.Where(displayItem);

            if (ComponentModelInstance != null)
            {
                var args = new OnGuiLoadDataArgs(GUIType.List, ComponentModelInstance, query, EventServices);
                ComponentModelInstance.OnGuiLoadData(args);
                if (args.ListLoadQuery != null)
                    query = args.ListLoadQuery.Cast<TModel>();
            }

            if (ComponentModelInstance != null)
            {
                var args = new OnGuiLoadDataArgs(GUIType.List, ComponentModelInstance, query, EventServices);
                ComponentModelInstance.OnGuiLoadData(args);
                if (args.ListLoadQuery != null)
                    query = args.ListLoadQuery.Cast<TModel>();
            }

            return query;
        }

        #endregion              
    }
}
