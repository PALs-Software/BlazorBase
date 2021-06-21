using BlazorBase.CRUD.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Extensions
{
    public static class EnumExtension
    {
        public static SortDirection GetNextSortDirection(this SortDirection direction)
        {
            switch (direction)
            {
                case SortDirection.None:
                    return SortDirection.Ascending;
                case SortDirection.Ascending:
                    return SortDirection.Descending;
                default:
                    return SortDirection.None;
            }
        }
    }
}
