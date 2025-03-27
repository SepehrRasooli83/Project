using System.ComponentModel.DataAnnotations;

public class RegisterModel
{
    [Required]
    public string EmailOrPhoneNumber { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords don't match.")]
    public string ConfirmPassword { get; set; }
}