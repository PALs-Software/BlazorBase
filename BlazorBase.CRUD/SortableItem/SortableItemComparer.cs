using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.SortableItem
{
    public class SortableItemComparer : IComparer<object>
    {
        public int Compare(object first, object second)
        {
            if (first != null && second != null)
                return (first as ISortableItem).SortIndex.CompareTo((second as ISortableItem).SortIndex);

            if (first == null && second == null)
                return 0;

            if (first != null)
                return -1;

            return 1;

        }
    }
}
