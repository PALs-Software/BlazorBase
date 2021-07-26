using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    public class CustomSortAndFilterPropertyPathAttribute : Attribute
    {
        public string Path { get; set; }
        public Type PathType { get; set; }
    }
}
