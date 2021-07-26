using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Models
{
    public class BlazorBaseOptions
    {
        #region Members

        private readonly IServiceProvider serviceProvider;

        private readonly Action<BlazorBaseOptions> configureOptions;

        #endregion

        #region Constructors

        public BlazorBaseOptions(IServiceProvider serviceProvider, Action<BlazorBaseOptions> configureOptions)
        {
            this.serviceProvider = serviceProvider;
            this.configureOptions = configureOptions;

            this.configureOptions?.Invoke(this);
        }

        #endregion
    }
}
