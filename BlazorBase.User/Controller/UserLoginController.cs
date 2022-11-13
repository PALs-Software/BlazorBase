using BlazorBase.User.Models;
using BlazorBase.User.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlazorBase.User.Controller
{
    [AllowAnonymous]
    [Route("[controller]/[action]")]
    public class UserLoginController : ControllerBase
    {
        protected readonly SignInManager<IdentityUser> SignInManager;
        protected readonly UserManager<IdentityUser> UserManager;
        protected readonly ILogger<UserLoginController> Logger;
        protected readonly IBlazorBaseUserOptions Options;

        public const string LoginPath = "/UserLogin/Login";
        public const string LogoutPath = "/UserLogin/Logout";

        public UserLoginController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<UserLoginController> logger, IBlazorBaseUserOptions options)
        {
            SignInManager = signInManager;
            UserManager = userManager;
            Logger = logger;
            Options = options;
        }

        [HttpPost()]
        public async Task<IActionResult> Login([FromForm] LoginData loginData, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var result = new Microsoft.AspNetCore.Identity.SignInResult();
            var user = await UserManager.FindByEmailAsync(loginData.Email);
            if (user != null)
                result = await SignInManager.PasswordSignInAsync(user.UserName, loginData.Password, loginData.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                Logger.LogInformation("User {Email} logged successfully in.", loginData.Email);
                return LocalRedirect(returnUrl);
            }

            return Redirect(Options.LoginPath);
        }

        [HttpPost()]
        public async Task<IActionResult> Logout()
        {
            if (SignInManager.IsSignedIn(User))
                await SignInManager.SignOutAsync();

            return Redirect("~/");
        }
    }
}
