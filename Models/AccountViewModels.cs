using System.ComponentModel.DataAnnotations;

namespace GrooveyTunes.Models
{
    // These classes describe the fields on each form and their validation rules.
    // The same rules are repeated in Scripts/auth.js so the browser can warn instantly.

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter your email address.")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$", ErrorMessage = "Enter a valid email, like name@example.com.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class SignupViewModel
    {
        [Required(ErrorMessage = "Choose a username.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be 3 to 20 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Use only letters, numbers and underscores.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Enter your email address.")]
        [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$", ErrorMessage = "Enter a valid email, like name@example.com.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Create a password.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password doesn't meet all the requirements yet.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Re-enter your password.")]
        [Compare("Password", ErrorMessage = "Passwords don't match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public bool AcceptTerms { get; set; }
    }
}
