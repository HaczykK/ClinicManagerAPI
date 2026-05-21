using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Auth
{
    public class AssignRoleDto
    {
        [Required(ErrorMessage = "Email użytkownika jest wymagany.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwa roli jest wymagana.")]
        public string Role { get; set; } = string.Empty;
    }
}
