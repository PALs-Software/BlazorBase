using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AllowUserPasswordAccessAttribute : Attribute
    {
        public string? AllowAccessCallbackMethodName { get; set; }
    }
}
