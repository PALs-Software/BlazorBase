using BlazorBase.Abstractions.CRUD.Interfaces;
using System.Collections.Generic;

namespace BlazorBase.CRUD.SortableItem;

public class SortableItemComparer : IComparer<object>
{
    public int Compare(object? first, object? second)
    {
        if (first != null && second != null)
            return ((ISortableItem)first).SortIndex.CompareTo(((ISortableItem)second).SortIndex);

        if (first == null && second == null)
            return 0;

        if (first != null)
            return -1;

        return 1;

    }
}
