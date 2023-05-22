using BlazorBase.CRUD.Models;
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

    public record OnBeforeOpenEditModalArgs(IBaseModel Model, EventServices EventServices)
    {
        public OnBeforeOpenEditModalArgs(bool isHandled, IBaseModel model, bool changeQueryUrl, EventServices eventServices) : this(model, eventServices)
        {
            IsHandled = isHandled;
            ChangeQueryUrl = changeQueryUrl;
        }

        public bool IsHandled { get; set; }
        public bool ChangeQueryUrl { get; set; }
    }

    public record OnBeforeOpenViewModalArgs(IBaseModel Model, EventServices EventServices)
    {
        public OnBeforeOpenViewModalArgs(bool isHandled, IBaseModel model, bool changeQueryUrl, EventServices eventServices) : this(model, eventServices)
        {
            IsHandled = isHandled;
            ChangeQueryUrl = changeQueryUrl;
        }

        public bool IsHandled { get; set; }
        public bool ChangeQueryUrl { get; set; }
    }

    public record OnBeforeNavigateToEntryArgs(IBaseModel Model, bool IsFirstPageLoadNavigation, EventServices EventServices)
    {
        public OnBeforeNavigateToEntryArgs(bool isHandled, IBaseModel model, bool isFirstPageLoadNavigation, EventServices eventServices) : this(model, isFirstPageLoadNavigation, eventServices)
        {
            IsHandled = isHandled;
        }

        public bool IsHandled { get; set; }
    }
}
