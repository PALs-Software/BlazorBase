using BlazorBase.Models;
using System.Security;

namespace BlazorBase.User.Models;
public interface IBlazorBaseUserOptions : IBaseOptions
{
    string LoginPath { get; set; }
}