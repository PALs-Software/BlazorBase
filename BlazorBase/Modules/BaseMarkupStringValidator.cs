﻿using Microsoft.AspNetCore.Components;
using System;
using System.Net;

namespace BlazorBase.Modules;

public static class BaseMarkupStringValidator
{
    public static MarkupString GetWhiteListedMarkupString(string? input, bool htmlEncodeInput = true)
    {
        if (input == null)
            return (MarkupString)String.Empty;

        input = input.Replace(Environment.NewLine, "<br />");

        if (htmlEncodeInput)
            input = WebUtility.HtmlEncode(input);

        return (MarkupString)ConvertWhiteListedHtmlBack(input);
    }

    private static string ConvertWhiteListedHtmlBack(string input)
    {
        input = input.Replace("&lt;br /&gt;", "<br />");
        input = input.Replace("&quot;", "\"");
        
        input = AllowHtmlTag(input, "a");
        input = AllowHtmlTag(input, "b");
        input = AllowHtmlTag(input, "span");
        input = AllowHtmlTag(input, "p");
        input = AllowHtmlTag(input, "img");

        if (input.Contains("&lt;h"))
            for (int i = 1; i <= 6; i++)
                input = AllowHtmlTag(input, $"h{i}");

        input = input.Replace("&gt;", ">");
        return input;
    }

    private static string AllowHtmlTag(string input, string tagName)
    {
        input = input.Replace($"&lt;{tagName}", $"<{tagName}");
        input = input.Replace($"&lt;/{tagName}&gt;", $"</{tagName}>");
        return input;
    }
}

