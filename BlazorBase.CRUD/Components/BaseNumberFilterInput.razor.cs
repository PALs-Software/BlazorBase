using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Components
{
    public partial class BaseNumberFilterInput
    {
        #region Parameter
        [Parameter] public EventCallback<ChangeEventArgs> OnInput { get; set; }
        #endregion

        protected override void OnInitialized()
        {
            base.OnInitialized();

            @CultureInfo.CurrentUICulture.Name
        }
    }
}
