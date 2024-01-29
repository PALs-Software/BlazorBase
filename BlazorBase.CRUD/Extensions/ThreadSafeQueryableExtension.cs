﻿using BlazorBase.CRUD.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Extensions;

public static class ThreadSafeQueryableExtension
{
    #region Aggregate
    public static async Task<TResult> AggregateTSAsync<TSource, TAccumulate, TResult>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func, Expression<Func<TAccumulate, TResult>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Aggregate(seed, func, selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TAccumulate> AggregateTSAsync<TSource, TAccumulate>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TAccumulate seed, Expression<Func<TAccumulate, TSource, TAccumulate>> func, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Aggregate(seed, func);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> AggregateTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TSource, TSource>> func, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Aggregate(func);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region All
    public static async Task<bool> AllTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.All(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> AllAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AllAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Any
    public static async Task<bool> AnyTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Any();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> AnyTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Any(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> AnyAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AnyAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> AnyAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region AsAsyncEnumerable

    public static async Task<IEnumerable<TSource>> AsEnumerableTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.AsEnumerable();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<IAsyncEnumerable<TSource>> AsAsyncEnumerableTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.AsAsyncEnumerable();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Average
    public static async Task<double> AverageTSAsync(this IQueryable<int> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageTSAsync(this IQueryable<int?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageTSAsync(this IQueryable<long> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageTSAsync(this IQueryable<long?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> AverageTSAsync(this IQueryable<float> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> AverageTSAsync(this IQueryable<float?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageTSAsync(this IQueryable<double> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageTSAsync(this IQueryable<double?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> AverageTSAsync(this IQueryable<decimal> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> AverageTSAsync(this IQueryable<decimal?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> AverageTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Average(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> AverageAsyncTSAsync(this IQueryable<decimal> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> AverageAsyncTSAsync(this IQueryable<decimal?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageAsyncTSAsync(this IQueryable<int> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageAsyncTSAsync(this IQueryable<int?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageAsyncTSAsync(this IQueryable<long> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageAsyncTSAsync(this IQueryable<long?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageAsyncTSAsync(this IQueryable<double> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageAsyncTSAsync(this IQueryable<double?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> AverageAsyncTSAsync(this IQueryable<float> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> AverageAsyncTSAsync(this IQueryable<float?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> AverageAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.AverageAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Contains
    public static async Task<bool> ContainsTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TSource item, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Contains(item);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> ContainsTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TSource item, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Contains(item, comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> ContainsAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TSource item, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ContainsAsync(item, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Count
    public static async Task<int> CountTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Count();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int> CountTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Count(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int> CountAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.CountAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int> CountAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.CountAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region ElementAt
    public static async Task<TSource> ElementAtTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, int index, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ElementAt(index);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> ElementAtTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Index index, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ElementAt(index);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> ElementAtAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, int index, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ElementAtAsync(index, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> ElementAtOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, int index, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ElementAtOrDefault(index);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> ElementAtOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Index index, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ElementAtOrDefault(index);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> ElementAtOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, int index, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ElementAtOrDefaultAsync(index, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region First
    public static async Task<TSource> FirstTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.First();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> FirstTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.First(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> FirstAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.FirstAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> FirstAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.FirstAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> FirstOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.FirstOrDefault();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> FirstOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TSource defaultValue, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.FirstOrDefault(defaultValue);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> FirstOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.FirstOrDefault(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> FirstOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.FirstOrDefault(predicate, defaultValue);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> FirstOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> FirstOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Last
    public static async Task<TSource> LastTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Last();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> LastTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Last(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> LastAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.LastAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> LastAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.LastAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> LastOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.LastOrDefault();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> LastOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TSource defaultValue, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.LastOrDefault(defaultValue);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> LastOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.LastOrDefault(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> LastOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.LastOrDefault(predicate, defaultValue);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> LastOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.LastOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> LastOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.LastOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Load
    public static async Task LoadTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            queryable.Load();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task LoadAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await queryable.LoadAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }
    #endregion

    #region LongCount
    public static async Task<long> LongCountTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.LongCount();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> LongCountTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.LongCount(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> LongCountAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.LongCountAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> LongCountAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.LongCountAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Max
    public static async Task<TResult?> MaxTSAsync<TSource, TResult>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Max(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MaxTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Max();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MaxTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, IComparer<TSource>? comparer, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Max(comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TResult> MaxAsyncTSAsync<TSource, TResult>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.MaxAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> MaxAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.MaxAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MaxByTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TKey>> keySelector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.MaxBy(keySelector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MaxByTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TKey>> keySelector, IComparer<TSource>? comparer, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.MaxBy(keySelector, comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Min
    public static async Task<TResult?> MinTSAsync<TSource, TResult>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Min(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MinTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Min();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MinTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, IComparer<TSource>? comparer, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Min(comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TResult> MinAsyncTSAsync<TSource, TResult>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.MinAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> MinAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.MinAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MinByTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TKey>> keySelector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.MinBy(keySelector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> MinByTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, TKey>> keySelector, IComparer<TSource>? comparer, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.MinBy(keySelector, comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region SequenceEqual
    public static async Task<bool> SequenceEqualTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, IEnumerable<TSource> source2, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.SequenceEqual(source2);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<bool> SequenceEqualTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, IEnumerable<TSource> source2, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.SequenceEqual(source2, comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Single
    public static async Task<TSource> SingleTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Single();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> SingleTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Single(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> SingleAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SingleAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> SingleAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SingleAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> SingleOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.SingleOrDefault();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> SingleOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, TSource defaultValue, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.SingleOrDefault(defaultValue);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> SingleOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.SingleOrDefault(predicate);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource> SingleOrDefaultTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, TSource defaultValue, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.SingleOrDefault(predicate, defaultValue);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> SingleOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource?> SingleOrDefaultAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SingleOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region Sum
    public static async Task<int> SumTSAsync(this IQueryable<int> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int?> SumTSAsync(this IQueryable<int?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> SumTSAsync(this IQueryable<long> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long?> SumTSAsync(this IQueryable<long?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> SumTSAsync(this IQueryable<float> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> SumTSAsync(this IQueryable<float?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> SumTSAsync(this IQueryable<double> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> SumTSAsync(this IQueryable<double?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> SumTSAsync(this IQueryable<decimal> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> SumTSAsync(this IQueryable<decimal?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int?> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long?> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> SumTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.Sum(selector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> SumAsyncTSAsync(this IQueryable<decimal> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> SumAsyncTSAsync(this IQueryable<decimal?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int> SumAsyncTSAsync(this IQueryable<int> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int?> SumAsyncTSAsync(this IQueryable<int?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> SumAsyncTSAsync(this IQueryable<long> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long?> SumAsyncTSAsync(this IQueryable<long?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> SumAsyncTSAsync(this IQueryable<double> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> SumAsyncTSAsync(this IQueryable<double?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> SumAsyncTSAsync(this IQueryable<float> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> SumAsyncTSAsync(this IQueryable<float?> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<decimal?> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<int?> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<long?> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<double?> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<float?> SumAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.SumAsync(selector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region ForEachAsync
    public static async Task ForEachAsyncTSAsync<T>(this IQueryable<T> queryable, IBaseDbContext dbContext, Action<T> action, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await queryable.ForEachAsync(action, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region ToArray

    public static async Task<TSource[]> ToArrayTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {            
            return queryable.ToArray();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<TSource[]> ToArrayAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region ToList
    public static async Task<List<TSource>> ToListTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ToList();
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<List<TSource>> ToListAsyncTSAsync<TSource>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion

    #region ToDictionary

    public static async Task<Dictionary<TKey, TSource>> ToDictionaryTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ToDictionary(keySelector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TElement>> ToDictionaryTSAsync<TSource, TKey, TElement>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ToDictionary(keySelector, elementSelector);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TSource>> ToDictionaryTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ToDictionary(keySelector, comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TElement>> ToDictionaryTSAsync<TSource, TKey, TElement>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return queryable.ToDictionary(keySelector, elementSelector, comparer);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsyncTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ToDictionaryAsync(keySelector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsyncTSAsync<TSource, TKey, TElement>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ToDictionaryAsync(keySelector, elementSelector, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TSource>> ToDictionaryAsyncTSAsync<TSource, TKey>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ToDictionaryAsync(keySelector, comparer, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsyncTSAsync<TSource, TKey, TElement>(this IQueryable<TSource> queryable, IBaseDbContext dbContext, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default) where TKey : notnull
    {
        await dbContext.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await queryable.ToDictionaryAsync(keySelector, elementSelector, comparer, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            dbContext.Semaphore.Release();
        }
    }

    #endregion
}
