using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.SortableItem
{
    public interface ISortableItem
    {
        int SortIndex { get; set; }
    }
}
