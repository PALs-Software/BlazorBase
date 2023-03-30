using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    public class UseCustomLookupData : Attribute
    {
        public string? LookupDataSourceMethodName { get; set; }
    }
}
