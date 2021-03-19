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
        #region Colors
        
        public static string Black { get; set; } = "#000000";
        public static string White { get; set; } = "#ffffff";
        public static string Gray100 { get; set; } = "#f8f9fa";
        public static string Gray200 { get; set; } = "#ebebeb";
        public static string Gray300 { get; set; } = "#dee2e6";
        public static string Gray400 { get; set; } = "#ced4da";
        public static string Gray500 { get; set; } = "#adb5bd";
        public static string Gray600 { get; set; } = "#888888";
        public static string Gray700 { get; set; } = "#444444";
        public static string Gray800 { get; set; } = "#303030";
        public static string Gray900 { get; set; } = "#222222";
        public static string Blue { get; set; } = "#375a7f";
        public static string Indigo { get; set; } = "#6610f2";
        public static string Purple { get; set; } = "#6f42c1";
        public static string Pink { get; set; } = "#e83e8c";
        public static string Red { get; set; } = "#e74c3c";
        public static string Orange { get; set; } = "#fd7e14";
        public static string Yellow { get; set; } = "#f39c12";
        public static string Green { get; set; } = "#00bc8c";
        public static string Teal { get; set; } = "#20c997";
        public static string Cyan { get; set; } = "#3498db";
        #endregion

        public static void ChangeTheme(Theme theme)
        {
            theme.Black = Black;
            theme.White = White;
          
            theme.BackgroundOptions = new ThemeBackgroundOptions()
            {
                Body = Gray900,
                Primary = Blue,
                Secondary = Gray700,
                Success = Green,
                Info = Cyan,
                Warning = Yellow,
                Danger = Red,
                Light = Gray700,
                Dark = Gray800,
            };

            theme.ColorOptions = new ThemeColorOptions()
            {
                Primary = Blue,
                Secondary = Gray700,
                Success = Green,
                Info = Cyan,
                Warning = Yellow,
                Danger = Red,
                Light = Gray700,
                Dark = Gray800,
            };

            theme.ColorOptions = new ThemeColorOptions()
            {
                Primary = Blue,
                Secondary = Gray700,
                Success = Green,
                Info = Cyan,
                Warning = Yellow,
                Danger = Red,
                Light = Gray700,
                Dark = Gray800,
            };
        }
    }
}
