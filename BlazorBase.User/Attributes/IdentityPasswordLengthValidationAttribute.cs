using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BlazorBase.User.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IdentityPasswordLengthValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var userManager = validationContext.GetRequiredService<UserManager<IdentityUser>>();
        var localizer = validationContext.GetRequiredService<IStringLocalizer<IdentityPasswordLengthValidationAttribute>>();

        List<string> errors = new();
        var isValid = true;
        foreach (var v in userManager.PasswordValidators)
        {
            var result = v.ValidateAsync(userManager, null!, value as string).Result;
            if (!result.Succeeded)
            {
                if (result.Errors.Any())
                    foreach (var error in result.Errors)
                    {
                        string errorMessage = error.Code switch
                        {
                            nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => (string)localizer[error.Code, userManager.Options.Password.RequiredUniqueChars],
                            nameof(IdentityErrorDescriber.PasswordTooShort) => (string)localizer[error.Code, userManager.Options.Password.RequiredLength],
                            _ => (string)localizer[error.Code],
                        };
                        errors.Add(errorMessage);
                    }                        

                isValid = false;
            }
        }

        if (!isValid)
            return new ValidationResult(String.Join(Environment.NewLine, errors));

        return ValidationResult.Success;
    }
}
