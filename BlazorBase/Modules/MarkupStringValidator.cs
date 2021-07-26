using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Modules
{
    public static class MarkupStringValidator
    {
        public static MarkupString GetWhiteListedMarkupString(string input)
        {
            input = input.Replace(Environment.NewLine, "<br />");
            return (MarkupString)ConvertWhiteListedHtmlBack(WebUtility.HtmlEncode(input));
        }

        private static string ConvertWhiteListedHtmlBack(string input)
        {
            return input.Replace("&lt;br /&gt;", "<br />");
        }
    }
}
