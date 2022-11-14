using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Identity;
using System;

namespace BlazorBase.User.Models;

public interface IBaseUser : IBaseModel
{
    Guid Id { get; set; }

    string Email { get; set; }

    string UserName { get; set; }
    string IdentityUserId { get; set; }  
}

public interface IBaseUser<TIdentityUser, TIdentityRole> : IBaseUser
    where TIdentityUser : IdentityUser, new()
{   
    TIdentityUser IdentityUser { get; set; }
    TIdentityRole IdentityRole { get; set; }
}
