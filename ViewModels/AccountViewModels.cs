using System.ComponentModel.DataAnnotations;

namespace Twix.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email      { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password   { get; set; } = "";

    public bool   RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required, MaxLength(50),
     RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Letters, numbers and underscores only.")]
    public string UserName    { get; set; } = "";

    [Required, EmailAddress]
    public string Email       { get; set; } = "";

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password    { get; set; } = "";

    [MaxLength(100)]
    public string DisplayName { get; set; } = "";

    [MaxLength(280)]
    public string Bio         { get; set; } = "";
}

public class EditProfileViewModel
{
    [MaxLength(100)]
    public string DisplayName { get; set; } = "";

    [MaxLength(280)]
    public string Bio         { get; set; } = "";

    [MaxLength(100)]
    public string Location    { get; set; } = "";
}
