using System;

namespace BlazorBase.User.Models;

public interface IBaseUserSessionData
{
    DateTime? LastSessionCreatedOn { get; set; }
}
