using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels.User
{
    public class UserCreateViewModel
    {
        [Required]
        public string? FullName { get; set; }
        [Required]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
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