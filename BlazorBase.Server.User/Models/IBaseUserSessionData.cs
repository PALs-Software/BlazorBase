namespace BlazorBase.Server.User.Models;

public interface IBaseUserSessionData
{
    DateTime? LastSessionCreatedOn { get; set; }
}
