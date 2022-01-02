using System.Security;

namespace BlazorBase.Mailing.Models
{
    public class BlazorBaseMailingOptions : IBlazorBaseMailingOptions
    {
        #region Members
        protected readonly IServiceProvider ServiceProvider;
        protected readonly Action<BlazorBaseMailingOptions>? ConfigureOptions;
        #endregion

        #region Constructors
        public BlazorBaseMailingOptions(IServiceProvider serviceProvider, Action<IBlazorBaseMailingOptions> configureOptions)
        {
            ServiceProvider = serviceProvider;
            ConfigureOptions = configureOptions;

            ConfigureOptions?.Invoke(this);
        }
        #endregion

        #region Properties
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
