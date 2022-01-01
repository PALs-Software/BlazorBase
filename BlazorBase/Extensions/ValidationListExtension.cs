using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.Extensions;

public static class ValidationListExtension
{
    public static string FormatResultsToString(this List<ValidationResult> results)
    {
        var formattedResult = String.Empty;
        foreach (var result in results)
            formattedResult += $"- {result.ErrorMessage}{Environment.NewLine}";

        return formattedResult;
    }
}
