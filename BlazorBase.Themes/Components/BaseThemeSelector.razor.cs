using BlazorBase.Themes.Models;
using Blazorise;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Themes.Components
{
    public partial class BaseThemeSelector
    {
        [CascadingParameter] Theme Theme { get; set; } = null!;

        List<ThemeContrast> ThemeContrasts { get; set; } = new List<ThemeContrast>();

        protected override Task OnInitializedAsync()
        {
            foreach (ThemeContrast theme in Enum.GetValues(typeof(ThemeContrast)))
                ThemeContrasts.Add(theme);

            return base.OnInitializedAsync();
        }

        void OnThemeChanged(ThemeContrast theme)
        {
            var baseTheme = (BaseTheme)Theme;

            baseTheme.ThemeContrast = theme;

            switch (theme)
            {
                case ThemeContrast.None:
                    LightTheme.ChangeTheme(Theme);
                    break;
                case ThemeContrast.Light:
                    LightTheme.ChangeTheme(Theme);
                    break;
                case ThemeContrast.Dark:
                    DarkTheme.ChangeTheme(Theme);
                    break;
                default:
                    break;
            }

            Theme.ThemeHasChanged();
        }


    }
}
