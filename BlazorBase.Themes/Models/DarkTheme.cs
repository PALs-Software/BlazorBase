using Blazorise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Themes
{
    public static class DarkTheme
    {
        public static void ChangeTheme(Theme theme)
        {
            theme.BackgroundOptions = new ThemeBackgroundOptions()
            {
                Body = ThemeColors.Gray.Shades["900"].Value,
                Primary = ThemeColors.Blue.Shades["400"].Value,
                Secondary = ThemeColors.Gray.Shades["700"].Value,
                Success = ThemeColors.Green.Shades["500"].Value,
                Info = ThemeColors.Cyan.Shades["500"].Value,
                Warning = ThemeColors.Yellow.Shades["500"].Value,
                Danger = ThemeColors.Red.Shades["500"].Value,
                Light = ThemeColors.Gray.Shades["700"].Value,
                Dark = ThemeColors.Gray.Shades["800"].Value
            };

            theme.ColorOptions = new ThemeColorOptions()
            {
                Primary = ThemeColors.Blue.Shades["400"].Value,
                Secondary = ThemeColors.Gray.Shades["700"].Value,
                Success = ThemeColors.Green.Shades["500"].Value,
                Info = ThemeColors.Cyan.Shades["500"].Value,
                Warning = ThemeColors.Yellow.Shades["500"].Value,
                Danger = ThemeColors.Red.Shades["500"].Value,
                Light = ThemeColors.Gray.Shades["700"].Value,
                Dark = ThemeColors.Gray.Shades["800"].Value
            };

            theme.InputOptions = new ThemeInputOptions()
            {
                Color = ThemeColors.Gray.Shades["700"].Value,
                CheckColor = ThemeColors.Gray.Shades["900"].Value,
                SliderColor = ThemeColors.Green.Shades["700"].Value
            };

            theme.TextColorOptions = new ThemeTextColorOptions()
            {
                Body = ThemeColors.Gray.Shades["900"].Value,
                Black50 = ThemeColors.Gray.Shades["100"].Value,
                White50= ThemeColors.Gray.Shades["900"].Value,
                White = ThemeColors.Gray.Shades["900"].Value,
                Primary = ThemeColors.Blue.Shades["400"].Value,
                Secondary = ThemeColors.Gray.Shades["700"].Value,
                Success = ThemeColors.Green.Shades["500"].Value,
                Info = ThemeColors.Cyan.Shades["500"].Value,
                Warning = ThemeColors.Yellow.Shades["500"].Value,
                Danger = ThemeColors.Red.Shades["500"].Value,
                Light = ThemeColors.Gray.Shades["700"].Value,
                Dark = ThemeColors.Gray.Shades["800"].Value
                
            };

            theme.ModalOptions = new ThemeModalOptions()
            {
            };
        }
    }
}
