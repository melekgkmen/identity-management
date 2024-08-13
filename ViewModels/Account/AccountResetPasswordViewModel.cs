using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels.Account
{
    public class AccountResetPasswordViewModel
    {
        [Required]
        public string? Token { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password does not match")]
        public string? ConfirmPassword { get; set; }

    }
}