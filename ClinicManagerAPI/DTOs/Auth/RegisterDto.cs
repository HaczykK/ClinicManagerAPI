using System.ComponentModel.DataAnnotations;

namespace ClinicManagerAPI.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [MinLength(6, ErrorMessage = "Hasło musi mieć minimum 6 znaków.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Specialization { get; set; }

        /// <summary>
        /// Rola użytkownika. Dozwolone: Lekarz, Rejestratorka. Domyślnie: Rejestratorka.
        /// Rola Admin nie może być przypisana przy rejestracji.
        /// </summary>
        [RegularExpression(@"^(Lekarz|Rejestratorka)$", ErrorMessage = "Dozwolone role: Lekarz, Rejestratorka.")]
        public string Role { get; set; } = "Rejestratorka";
    }
}
