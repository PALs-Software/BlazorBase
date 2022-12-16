using BlazorBase.Models;
using System.Security;

namespace BlazorBase.Mailing.Models
{
    public class BlazorBaseMailingOptions : IBlazorBaseMailingOptions
    {
        #region Constructors
        public BlazorBaseMailingOptions(IServiceProvider serviceProvider, Action<BlazorBaseMailingOptions> configureOptions)
        {
            (this as IBlazorBaseMailingOptions).ImportOptions(serviceProvider, configureOptions);
        }
        #endregion

        #region Properties
        public BaseOptionsImportMode OptionsImportMode { get; set; }

        public string WebsiteName { get; set; } = default!;

        public string Server { get; set; } = default!;
        public bool UseDefaultCredentials { get; set; } = false;
        public int Port { get; set; } = 587;
        public bool EnableSSL { get; set; } = true;
        public string SenderAddress { get; set; } = default!;

        public string? Domain { get; set; } = null;
        public string Host { get; set; } = default!;
        public SecureString HostPassword { get; set; } = default!;        
        #endregion


    }
}
