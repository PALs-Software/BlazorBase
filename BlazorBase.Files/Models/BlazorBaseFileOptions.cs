using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Files.Models
{
    public class BlazorBaseFileOptions
    {
        #region Members

        protected readonly IServiceProvider ServiceProvider;

        protected readonly Action<BlazorBaseFileOptions> ConfigureOptions;

        #endregion

        #region Properties

        #endregion
        public string FileStorePath { get; set; } = @"C:\BlazorBaseFileStore";
        public string TempFileStorePath { get; set; } = @"C:\BlazorBaseFileStore\Temp";
        #region Constructors

        public BlazorBaseFileOptions(IServiceProvider serviceProvider, Action<BlazorBaseFileOptions> configureOptions)
        {
            ServiceProvider = serviceProvider;
            ConfigureOptions = configureOptions;

            ConfigureOptions?.Invoke(this);
        }

        #endregion
    }
}
