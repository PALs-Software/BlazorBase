using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace BlazorBase.User.Models;
public interface IBaseUser<TIdentityUser, TIdentityRole> : IBaseModel
    where TIdentityUser : IdentityUser, new()
{
    Guid Id { get; set; }

    string Email { get; set; }

    string UserName { get; set; }
    string IdentityUserId { get; set; }
    TIdentityUser IdentityUser { get; set; }
    TIdentityRole IdentityRole { get; set; }
}
