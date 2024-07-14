using BlazorBase.Abstractions.CRUD.Resources.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.User.ViewModels;

public class LoginData
{
    [Required(ErrorMessageResourceName = nameof(ValidationAttributesTranslations.RequiredAttribute), ErrorMessageResourceType = typeof(ValidationAttributesTranslations))]
    [EmailAddress(ErrorMessageResourceName = nameof(ValidationAttributesTranslations.EmailAddressAttribute), ErrorMessageResourceType = typeof(ValidationAttributesTranslations))]
    public string Email { get; set; } = null!;

    [Required(ErrorMessageResourceName = nameof(ValidationAttributesTranslations.RequiredAttribute), ErrorMessageResourceType = typeof(ValidationAttributesTranslations))]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}
