using System.ComponentModel.DataAnnotations;

namespace PlacementMentorshipPortal.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "New Password is required.")]
        [StringLength(14, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 14 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,14}$",
            ErrorMessage = "Password must have at least one uppercase, one lowercase, one digit, and one special character.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}