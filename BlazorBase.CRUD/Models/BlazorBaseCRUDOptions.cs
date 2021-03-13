using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Models
{
    public class BlazorBaseCRUDOptions
    {
        #region Members

        private readonly IServiceProvider serviceProvider;

        private readonly Action<BlazorBaseCRUDOptions> configureOptions;

        #endregion

        #region Constructors

        public BlazorBaseCRUDOptions(IServiceProvider serviceProvider, Action<BlazorBaseCRUDOptions> configureOptions)
        {
            this.serviceProvider = serviceProvider;
            this.configureOptions = configureOptions;

            this.configureOptions?.Invoke(this);
        }

        #endregion
    }
}
