using BlazorBase.User.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBase.User.Components;

public partial class BaseLoginForm : ComponentBase
{
    #region Parameter
    [Parameter] public string WebsiteName { get; set; }
    [Parameter] public RenderFragment Logo { get; set; }
    [Parameter] public string LogoSrc { get; set; }
    [Parameter] public string LogoHref { get; set; }
    [Parameter] public int LogoHeight { get; set; } = 150;
    [Parameter] public int LogoWidth { get; set; } = 150;

    [Parameter] public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;

    [Parameter] public Blazorise.Color ButtonColor { get; set; } = Blazorise.Color.Primary;
    [Parameter] public bool ShowImageOfTheDayAsBackgroundImage { get; set; }
    #endregion

    #region Inject
    [Inject] protected IStringLocalizer<BaseLoginForm> Localizer { get; set; }
    [Inject] protected SignInManager<IdentityUser> SignInManager { get; set; }
    [Inject] protected ILogger<BaseLoginForm> Logger { get; set; }
    #endregion

    #region Properties
    protected static DateTime LastImageOfTheDayUrlPull { get; set; } = DateTime.MinValue;
    protected static string ImageOfTheDayUrl { get; set; }

    protected string AdditionalStyle { get; set; }

    protected LoginData LoginData { get; set; } = new();
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        if (!ShowImageOfTheDayAsBackgroundImage)
            return;

        if (!String.IsNullOrEmpty(ImageOfTheDayUrl) && LastImageOfTheDayUrlPull.Date == DateTime.Now.Date)
            return;

        LastImageOfTheDayUrlPull = DateTime.Now;
        ImageOfTheDayUrl = await GetBingImageOfTheDayUrlAsync();
    }
    #endregion

    #region Submit Login
    public async Task HandleValidSubmit()
    {
        var result = await SignInManager.PasswordSignInAsync(LoginData.Email, LoginData.Password, LoginData.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            Logger.LogInformation($"User {LoginData.Email} logged in.");
            //return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            Logger.LogInformation($"User {LoginData.Email} account locked out.");
            //return RedirectToPage("./Lockout");
        }
        else
        {
            //ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            // return Page();
        }
    }
    #endregion

    #region Background Image
    protected async Task<string> GetBingImageOfTheDayUrlAsync()
    {
        var url = $"https://www.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt={CultureInfo.Name}";
        var client = new HttpClient();
        var response = await client.GetAsync(new Uri(url));
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        if (!result.TryGetValue("images", out JToken token))
            return null;

        var images = token as JArray;
        var firstImage = images?.FirstOrDefault() as JObject;
        if (firstImage is not null && firstImage.TryGetValue("url", out JToken imageUrl))
            return new Uri(new Uri("https://www.bing.com"), imageUrl.ToString()).ToString();

        return null;
    }
    #endregion
}
