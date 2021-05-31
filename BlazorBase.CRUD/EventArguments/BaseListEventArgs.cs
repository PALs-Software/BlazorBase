using BlazorBase.CRUD.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.EventArguments
{
    public record OnBeforeOpenAddModalArgs(EventServices EventServices)
    {
        public OnBeforeOpenAddModalArgs(bool isHandled, EventServices eventServices) : this(eventServices) => IsHandled = isHandled;
        public bool IsHandled { get; set; }
    }
}
