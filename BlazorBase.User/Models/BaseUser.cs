using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.User.Models
{
    public class BaseUser : IdentityUser
    {
        [Key]
        public override string Id { get => base.Id; set => base.Id = value; }

        [DisplayKey]
        public override string UserName { get => base.UserName; set => base.UserName = value; }
    }

    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<BaseUser, IdentityRole>
    {
        public AppClaimsPrincipalFactory(
            UserManager<BaseUser> userManager
            , RoleManager<IdentityRole> roleManager
            , IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
        { }

        public async override Task<ClaimsPrincipal> CreateAsync(BaseUser user)
        {
            return await base.CreateAsync(user);
        }
    }
}
