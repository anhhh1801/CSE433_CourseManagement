using System.ComponentModel.DataAnnotations;

namespace CourseManagement.Models
{
    public class EmailVerificationModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OTP { get; set; }
    }
}
