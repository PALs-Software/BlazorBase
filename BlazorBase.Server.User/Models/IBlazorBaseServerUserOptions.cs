using BlazorBase.Models;

namespace BlazorBase.Server.User.Models;

public interface IBlazorBaseServerUserOptions : IBaseOptions
{  
    bool LogUserSessions { get; set; }
}