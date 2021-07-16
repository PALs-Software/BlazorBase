using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.ViewModels;
using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public class PageAction
    {
        public string Caption { get; set; }
        public string ToolTip { get; set; }
        public Color Color { get; set; } = Color.Secondary;
        public object Image { get; set; }
        public GUIType[] VisibleInGUITypes { get; set; } = new GUIType[] { GUIType.Card, GUIType.List, GUIType.ListPart };
        public Func<EventServices, Task<bool>> Visible { get; set; } = x => Task.FromResult(true);
        public Func<EventServices, IBaseModel, Task> Action { get; set; }
    }
}
