using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace BlazorBase.User.Extensions;
public static class IdentityResultExtension
{
    public static string GetErrorMessage(this IdentityResult result)
    {
        return String.Join(Environment.NewLine, result.Errors.Select(entry => $"{entry.Code}: {entry.Description}"));
    }
}