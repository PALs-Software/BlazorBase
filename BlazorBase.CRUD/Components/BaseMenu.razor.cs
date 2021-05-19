using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseMenu
    {
        [Parameter]
        public object MenuName { get; set; }

        [Parameter]
        public object MenuIcon { get; set; }

        [Parameter]
        public bool DropDownIsVisible { get; set; } = true;

        [Parameter]
        public List<NavigationEntry> NavigationEntries { get; set; } = new List<NavigationEntry>();

        public record NavigationEntry(string Name, string Link, string Icon);

        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object> AdditionalAttributes { get; set; }
    }
}
