using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.User.Controller
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    public class UserLoginController : ControllerBase
    {
        protected readonly SignInManager<IdentityUser> SignInManager;

        public UserLoginController(SignInManager<IdentityUser> signInManager)
        {
            SignInManager = signInManager;
        }
     
        [HttpPost()]
        public IActionResult Index([FromForm] string email, [FromForm] string password, [FromForm] string rememberMe)
        {
            _ = bool.TryParse(rememberMe, out bool res);
            var signInResult = SignInManager.PasswordSignInAsync(email, password, res, false);
            if (signInResult.Result.Succeeded)
            {
                return Redirect("/");
            }
            return Redirect("/login/" + signInResult.Result.Succeeded);
        }

        [HttpPost()]
        public async Task<IActionResult> Logout()
        {
            if (SignInManager.IsSignedIn(User))
            {
                await SignInManager.SignOutAsync();
            }
            return Redirect("/");
        }
    }
}
