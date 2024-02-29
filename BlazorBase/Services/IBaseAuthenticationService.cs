using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBase.Services;

public interface IBaseAuthenticationService
{
    Task<List<string>> GetUserRolesAsync();
}
