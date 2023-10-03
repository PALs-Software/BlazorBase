using BlazorBase.CRUD.Resources.ValidationAttributes;
using BlazorBase.User.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.User.ViewModels;

public class ChangePasswordData
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [IdentityPasswordLengthValidation]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
