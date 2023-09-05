using BlazorBase.CRUD.Components.Card;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.ViewModels;
using BlazorBase.User.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.User.Components
{
    public class BaseUserManageProfile : ComponentBase
    {
        #region Inject
        [Inject] protected IBaseUser BaseUser { get; set; } = null!;
        #endregion

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            builder.OpenComponent(0, typeof(BaseCard<>).MakeGenericType(BaseUser.GetType()));
            builder.AddAttribute(1, "Embedded", false);
            builder.AddAttribute(1, "ShowEntryByStart", true);
            builder.AddAttribute(1, "ShowActions", false);
            builder.AddAttribute(2, "EntryToBeShownByStart", new Func<EventServices, Task<IBaseModel?>>(GetShowOnlySingleEntryInstance));
            builder.CloseComponent();
        }

        public virtual async Task<IBaseModel?> GetShowOnlySingleEntryInstance(EventServices eventServices)
        {
            return (IBaseModel?)await eventServices.BaseService.Set(BaseUser.GetType()).FirstOrDefaultAsync();
        }
    }
}
