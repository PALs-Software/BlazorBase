using BlazorBase.CRUD.Resources.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace BlazorBase.User.ViewModels;

public class ChangePasswordData
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;
}
