using System.ComponentModel.DataAnnotations;

namespace StockManager.Contracts.User
{
    public class UserRegister
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; init; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; init; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; } = string.Empty;

        public ApplicationUser User { get; init; }
    }

}