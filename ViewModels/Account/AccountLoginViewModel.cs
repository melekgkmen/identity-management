using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels.Account
{
    public class AccountLoginViewModel
    {
        [EmailAddress]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; } = true!;
    }
}