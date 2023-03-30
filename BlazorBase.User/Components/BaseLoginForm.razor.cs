using BlazorBase.User.Models;
using BlazorBase.User.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;
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
    [Parameter] public string? WebsiteName { get; set; }
    [Parameter] public RenderFragment Logo { get; set; }
    [Parameter] public string? LogoSrc { get; set; }
    [Parameter] public string? LogoHref { get; set; }
    [Parameter] public int LogoHeight { get; set; } = 150;
    [Parameter] public int LogoWidth { get; set; } = 150;

    [Parameter] public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentUICulture;

    [Parameter] public Blazorise.Color ButtonColor { get; set; } = Blazorise.Color.Primary;
    [Parameter] public bool ShowImageOfTheDayAsBackgroundImage { get; set; }
    #endregion

    #region Inject
    [Inject] protected IStringLocalizer<BaseLoginForm> Localizer { get; set; }
    [Inject] protected SignInManager<IdentityUser> SignInManager { get; set; }
    [Inject] protected UserManager<IdentityUser> UserManager { get; set; }
    [Inject] protected ILogger<BaseLoginForm> Logger { get; set; }
    [Inject] protected IJSRuntime JSRuntime { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; }
    [Inject] protected IBlazorBaseUserOptions Options{ get; set; }
    #endregion

    #region Properties
    protected static DateTime LastImageOfTheDayUrlPull { get; set; } = DateTime.MinValue;
    protected static string ImageOfTheDayUrl { get; set; }

    protected string AdditionalStyle { get; set; }

    protected LoginData LoginData { get; set; } = new();
    protected string ReturnUrl { get; set; }
    protected string Feedback { get; set; }
    #endregion

    #region Init
    protected override async Task OnInitializedAsync()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("returnUrl", out StringValues value))
            ReturnUrl = value;

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
        Feedback = String.Empty;
        var result = new SignInResult();
        var user = await UserManager.FindByEmailAsync(LoginData.Email);
        if (user != null)
            result = await SignInManager.CheckPasswordSignInAsync(user, LoginData.Password, lockoutOnFailure: false);

        if (result.Succeeded)
            // Needed because Identity Framework Login is not Possible over Blazor Session, so post values also to a standard controller
            await JSRuntime.InvokeVoidAsync("blazorBase.user.submitForm", "action-login-form");
        else
        {
            Logger.LogInformation("Login of User \"{Email}\" failed.", LoginData.Email);
            Feedback = Localizer["Invalid login attempt. Please check your e-mail address and the password you entered."];
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
