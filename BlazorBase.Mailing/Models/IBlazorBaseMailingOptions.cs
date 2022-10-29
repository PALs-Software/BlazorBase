using BlazorBase.Models;
using System.Security;

namespace BlazorBase.Mailing.Models;
public interface IBlazorBaseMailingOptions : IBaseOptions
{
    string WebsiteName { get; set; }

    string Server { get; set; }
    bool UseDefaultCredentials { get; set; }
    int Port { get; set; }
    bool EnableSSL { get; set; }
    string SenderAddress { get; set; }

    string? Domain { get; set; }
    string Host { get; set; }
    SecureString HostPassword { get; set; }
}