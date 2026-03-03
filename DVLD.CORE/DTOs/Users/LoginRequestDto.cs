using System.ComponentModel.DataAnnotations;

namespace DVLD.CORE.DTOs.Users
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "UserName is required")]
        [StringLength(20, MinimumLength = 5)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = null!;
    }
}
