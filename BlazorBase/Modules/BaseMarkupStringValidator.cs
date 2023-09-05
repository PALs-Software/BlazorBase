using Microsoft.AspNetCore.Components;
using System;
using System.Net;

namespace BlazorBase.Modules;

public static class BaseMarkupStringValidator
{
    public static MarkupString GetWhiteListedMarkupString(string? input)
    {
        if (input == null)
            return (MarkupString)String.Empty;

        input = input.Replace(Environment.NewLine, "<br />");
        return (MarkupString)ConvertWhiteListedHtmlBack(WebUtility.HtmlEncode(input));
    }

    private static string ConvertWhiteListedHtmlBack(string input)
    {
        return input.Replace("&lt;br /&gt;", "<br />");
    }
}

